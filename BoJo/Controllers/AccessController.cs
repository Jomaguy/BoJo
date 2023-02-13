using Microsoft.AspNetCore.Mvc;
using BoJo.Models;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;

namespace BoJo.Controllers
{
    public class AccessController : Controller
    {

        //======== Database connection string and Session ======/
        public static ISession cSession;
        public static User current_user = new BoJo.Models.User();
        //public static string DBAdd = "Server=(localdb)\\MSSQLLocalDB;Database=BOJO_DB;Trusted_Connection=True;MultipleActiveResultSets=true";
        //static string DBAdd2 = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BOJO_DB;Integrated Security=True";
        public static string DB_String = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        //GET ACCESS 
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User cUser)
        {
            //collect data from view
            ViewData["fname_value"] = cUser.Fname;
            ViewData["lname_value"] = cUser.Lname;
            ViewData["email_value"] = cUser.Email;
            ViewData["password_value"] = cUser.Password;
            ViewData["comfirmpassword_value"] = cUser.ComfirmPassword;

            //check if password and confirm password are the same
            if (cUser.Password == cUser.ComfirmPassword)
            {
                //encrypt password
                cUser.Password = ConvertToSha256(cUser.Password);
            } else {
                //update view
                ViewData["Message"] = "The passwords does not match";
                return View();
            }

            //vars
            bool registrated;
            string message;

            //========  SQL Connection =======/
            using (SqlConnection conn = new SqlConnection(DB_String))
            {
                SqlCommand cmd = new SqlCommand("sp_RegisterUser", conn); //procedure
                //===== set up procesure's parameters ============//
                cmd.Parameters.AddWithValue("Fname",cUser.Fname);
                cmd.Parameters.AddWithValue("Lname",cUser.Lname);
                cmd.Parameters.AddWithValue("Email",cUser.Email);
                cmd.Parameters.AddWithValue("Password",cUser.Password);
                //===== setting up response values ====//
                cmd.Parameters.Add("Registrated",SqlDbType.BigInt).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("Message", SqlDbType.VarChar,100).Direction = ParameterDirection.Output;

                //type of command
                cmd.CommandType = CommandType.StoredProcedure;

                //open connection
                conn.Open();

                //execute command
                cmd.ExecuteNonQuery();

                //=== collect outputs ===//
                registrated = Convert.ToBoolean(cmd.Parameters["Registrated"].Value);
                message = (cmd.Parameters["Message"].Value).ToString();
            }
            //update view
            ViewData["Message"] = message;

            //===== IF REGISTRATION WAS SUCCESSFULL =======//
            if(registrated)
            {
                return RedirectToAction("Login", "Access"); //redirrect to login
            } else
            {
                return View();  //update view
            }
            return View();
        }//Register

        [HttpPost]
        public IActionResult Login(User cUser)
        {
            //collect information from view
            ViewData["email_value"] = cUser.Email;
            ViewData["password_value"] = cUser.Password;

            //encrypt password
            cUser.Password = ConvertToSha256(cUser.Password);

            //========  SQL Connection =======/
            using (SqlConnection conn = new SqlConnection(DB_String))
            {
                SqlCommand cmd = new SqlCommand("sp_ValidateUser", conn); //procedure 
                //===== set up parameters ============//
                cmd.Parameters.AddWithValue("Email", cUser.Email);
                cmd.Parameters.AddWithValue("Password", cUser.Password);

                //type of command
                cmd.CommandType = CommandType.StoredProcedure;

                //open connection
                conn.Open();

                //execute command
                using (SqlDataReader Reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (Reader.HasRows && Reader.Read() && Reader.GetName(0)!="notvalid")
                    {
                        //COLLECT USER INFO
                        cUser.IdUser = Convert.ToInt32(Reader.GetInt32(Reader.GetOrdinal("IdUser")));
                        cUser.Fname = Reader.GetString(Reader.GetOrdinal("Fname"));
                        cUser.Lname = Reader.GetString(Reader.GetOrdinal("Lname"));
                        cUser.Email = Reader.GetString(Reader.GetOrdinal("Email"));
                        cUser.ComfirmPassword = " ";
                    }
                }

            }

            //if user found serialize information so it can be moved around
            if (cUser != null && cUser.IdUser != 0)
            {
                HttpContext.Session.SetString("user", JsonConvert.SerializeObject(cUser));
                cSession = HttpContext.Session;
                current_user = cUser;
                return RedirectToAction("Index", "Home");
            } else
            {
                ViewData["Message"] = "This user does not exist";
                return View(); 
            }
        } //Login

        //EndSession /aka logout
        public IActionResult EndSession()
        {
            HttpContext.Session.SetString("user", ""); //clean storaged info
            HttpContext.Session.Clear();
            cSession = null;
            current_user = new BoJo.Models.User();
            return RedirectToAction("Login", "Access"); //redirrect to login
        } //EndSession

        //Encryption Function
        public static string ConvertToSha256(string passwd) { 
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(passwd));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        } //ConvertToSha256


    } //AccessController
} //BOJO_UserAuth namespace

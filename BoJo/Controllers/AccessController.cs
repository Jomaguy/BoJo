using Microsoft.AspNetCore.Mvc;
using BoJo.Models;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Newtonsoft.Json;
using BoJo.Services;

namespace BoJo.Controllers
{
    public class AccessController : Controller
    {

        //======== Database connection string  ======/
        
        //sql connection string
        public static string connString = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
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
            ViewData["DOB"] = cUser.DOB;

            //check if password and confirm password are the same
            if (cUser.Password == cUser.ComfirmPassword)
            {
                //encrypt password
                cUser.Password = ConvertToSha256(cUser.Password);
            }
            else
            {
                //update view
                ViewData["Message"] = "The passwords does not match";
                return View();
            }
            //
            string confirmationToken = Guid.NewGuid().ToString();

            //vars
            bool registrated;
            string message;

            //========  SQL Connection =======/
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("sp_RegisterUser", conn); //procedure
                //===== set up procesure's parameters ============//
                cmd.Parameters.AddWithValue("Fname", cUser.Fname);
                cmd.Parameters.AddWithValue("Lname", cUser.Lname);
                cmd.Parameters.AddWithValue("Email", cUser.Email);
                cmd.Parameters.AddWithValue("Password", cUser.Password);
                cmd.Parameters.AddWithValue("DOB", cUser.DOB);
                cmd.Parameters.AddWithValue("Role", cUser.Role);
                cmd.Parameters.AddWithValue("ConfirmationToken", confirmationToken);
                //===== setting up response values ====//
                cmd.Parameters.Add("Registrated", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("Message", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

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
            if (registrated)
            {
                string confirmationLink = $"https://localhost:7038/Access/ConfirmEmail?email={cUser.Email}&token={confirmationToken}";
                string emailBody = $"Please click the following link to confirm your email address: {confirmationLink}";
                //EmailService.SendEmail(cUser.Email, "Confirm your email address", emailBody);
                Send_Email(cUser.Email, "BoJo : Confirm your email address ", emailBody);

                return RedirectToAction("Login", "Access"); //redirrect to login
            }
            else
            {
                return View();  //update view
            }
            return View();
        }//Register

        [HttpPost]
        public async Task Send_Email(string email, string subject, string body)
        {
            EmailSender emailSender = new EmailSender();
            await emailSender.SendEmailAsync(email, subject, body);
        }

        public ActionResult ConfirmEmail(string email, string token)
        {
            bool Confirmed;

            //========  SQL Connection =======/
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("sp_ConfirmEmail", conn); //procedure
                //===== set up procesure's parameters ============//
                cmd.Parameters.AddWithValue("Email", email);
                cmd.Parameters.AddWithValue("ConfirmationToken", token);

                //===== setting up response values ====//
                cmd.Parameters.Add("Confirmed", SqlDbType.BigInt).Direction = ParameterDirection.Output;

                //type of command
                cmd.CommandType = CommandType.StoredProcedure;

                //open connection
                conn.Open();

                //execute command
                cmd.ExecuteNonQuery();

                //=== collect outputs ===//
                Confirmed = Convert.ToBoolean(cmd.Parameters["Confirmed"].Value);
            }  

            if (Confirmed)
            {
                ViewData["link_Confirmed"] = true;
                return RedirectToAction("Login", "Access"); //redirrect to login
            }
            else
            {
                return View("Invalid");
            }
        }

        [HttpPost]
        public void sendconfirmation(string email)
		{
            //
            string confirmationToken = Guid.NewGuid().ToString();
            int result = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("" +
                    "UPDATE bojo_users " +
                    " SET ConfirmationToken = @token" +
                    " WHERE Email = @email", 
                conn);
                cmd.Parameters.AddWithValue("@token", confirmationToken);
                cmd.Parameters.AddWithValue("@email", email);

                conn.Open();

                result = cmd.ExecuteNonQuery(); 

                conn.Close();
            }

            string confirmationLink = $"https://localhost:7038/Access/ConfirmEmail?email={email}&token={confirmationToken}";
            string emailBody = $"Please click the following link to confirm your email address: {confirmationLink}";
            //EmailService.SendEmail(cUser.Email, "Confirm your email address", emailBody);
            Send_Email(email, "BoJo : Confirm your email address ", emailBody);
            //return Json("Confirmation Successfully sent to: " + email);
		}
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            string confirmationToken = Guid.NewGuid().ToString();
            int result = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("" +
                    "UPDATE bojo_users " +
                    " SET ConfirmationToken = @token" +
                    " WHERE Email = @email",
                conn);
                cmd.Parameters.AddWithValue("@token", confirmationToken);
                cmd.Parameters.AddWithValue("@email", email);

                conn.Open();

                result = cmd.ExecuteNonQuery();

                conn.Close();
            }
            if (result != 0)
            {
                string confirmationLink = $"https://localhost:7038/Access/ForgotPasswordLink?token={confirmationToken}";
                string emailBody = $"Please click the following link to change your password: {confirmationLink}";
                //EmailService.SendEmail(cUser.Email, "Confirm your email address", emailBody);
                _ = Send_Email(email, "BoJo : Forgot Password Link ", emailBody);
                return View();
            }
            return View();
        }

        public IActionResult ForgotPasswordLink(string token)
        {
            //========  SQL Connection =======/
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("" +
                    "select IdUser from bojo_users " +
                    " WHERE ConfirmationToken = @token",
                conn);
                cmd.Parameters.AddWithValue("@token", token);

                //open connection
                conn.Open();

                //execute command
                using (SqlDataReader Reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (Reader.HasRows)
                    {
                        ViewData["Token"] = token;
                        return View();
                    }
                }

            }
            return View("Invalid");
        }

        [HttpPost]
        public IActionResult ForgotPasswordLink(string token,string pwd,string confirm_pwd)
        {
            int result = 0;
            //check if password and confirm password are the same
            if (pwd == confirm_pwd)
            {
                //encrypt password
                pwd = ConvertToSha256(pwd);
            }
            else
            {
                //update view
                ViewData["Message"] = "The passwords does not match";
                return View();
            }
            //========  SQL Connection =======/
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("" +
                    "UPDATE bojo_users " +
                    " SET Password = @pwd" +
                    " WHERE ConfirmationToken = @token",
                conn);
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@pwd", pwd);

                //open connection
                conn.Open();

                //execute command
                result = cmd.ExecuteNonQuery();

                conn.Close();

            }
            if (result == 1)
            {
                return RedirectToAction("Login", "Access"); 
            }
            return View("Invalid");
        }

        [HttpPost]
        public IActionResult Login(User cUser)
        {
            //collect information from view
            ViewData["email_value"] = cUser.Email;
            ViewData["password_value"] = cUser.Password;

            //encrypt password
            cUser.Password = ConvertToSha256(cUser.Password);

            //========  SQL Connection =======/
            using (SqlConnection conn = new SqlConnection(connString))
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
                    if (Reader.HasRows && Reader.Read() && Reader.GetName(0) != "notvalid")
                    {
                        //COLLECT USER INFO
                        cUser.IdUser = Convert.ToInt32(Reader.GetInt32(Reader.GetOrdinal("IdUser")));
                        cUser.Fname = Reader.GetString(Reader.GetOrdinal("Fname"));
                        cUser.Lname = Reader.GetString(Reader.GetOrdinal("Lname"));
                        cUser.Email = Reader.GetString(Reader.GetOrdinal("Email"));
                        cUser.DOB = Reader.GetDateTime("DOB").ToString("MM/dd/yyyy");
                        cUser.Role = Reader.GetString(Reader.GetOrdinal("Role"));
                        cUser.Confirmed = Convert.ToBoolean(Reader["EmailConfirmed"].ToString());
                    }
                }

            }

            //if user found serialize information so it can be moved around
            if (cUser != null && cUser.IdUser != 0)
            {
                if (cUser.Confirmed != true)
                {
                    ViewData["Confirmed"] = "false";
                    HttpContext.Session.SetString("emailtoconfirm", cUser.Email);
                    return View();
                }
                HttpContext.Session.SetString("user", JsonConvert.SerializeObject(cUser));
                HttpContext.Session.SetString("userfname",cUser.Fname);
                HttpContext.Session.SetString("userrole", cUser.Role);
                HttpContext.Session.SetInt32("userid", cUser.IdUser);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["Message"] = "This user does not exist";
                return View();
            }
        } //Login

        //EndSession /aka logout
        public IActionResult EndSession()
        {
            HttpContext.Session.SetString("user", ""); //clean storaged info
            HttpContext.Session.SetInt32("userid", -1);
            HttpContext.Session.Clear();
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

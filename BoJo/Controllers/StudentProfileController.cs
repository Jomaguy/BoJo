using Microsoft.AspNetCore.Mvc;
using BoJo.Models;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BoJo.Controllers
{
    public class StudentProfileController : Controller
    {
        //connection string
        string connString = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        //major list
        private List<string> majors = new List<string>()
        {
            "Computer Science",
            "Biology",
            "Physics",
            "Mathematics",
            "Chemestry",
            "Business"
        };
        
        //this function manages the student information profile
        public IActionResult StudentProfile()
        {
            //check if user is logged in
            if (HttpContext.Session.GetInt32("userid") != null)
            {
                //get necesary data
                ViewData["StudentProfile"] = GetSPInformation(HttpContext.Session.GetInt32("userid"));
                ViewData["User"] = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("user"));
                return View();
            }
            //if not registrated
            return RedirectToAction("Login", "Access");
        }

        //this function manages the student profile form 
        //where the user can modify their information
        public IActionResult StudentProfileForm()
        {
            //check if user is logged in
            if (HttpContext.Session.GetInt32("userid") != null)
            {
                //get necesary information
                ViewData["majorSelectList"] = majors;
                ViewData["StudentProfile"] = GetSPInformation(HttpContext.Session.GetInt32("userid"));
                return View();
            }
            //if not registrated
            return RedirectToAction("Login", "Access");
        }
        [HttpPost]
        public IActionResult StudentProfileForm(string ProgramType, string Major, string Location, string School_Size, int ACT_Score, int SAT_Score, float GPA,float GPA_outof)
        {
            //this if statement is kind of unecesary, but 
            //if i find a better alternative I will modify it
            if (ProgramType == "" || Major == "" || Location == "" || School_Size == "" || ACT_Score == null || SAT_Score == null || GPA == null || GPA_outof==null)
            {
                ViewData["Message"] = "Please fill all the requierements";
                return View();
            }

            //==== SQL Query ====//
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //stored procedure
                SqlCommand cmd = new SqlCommand("sp_UpdateSP", conn);

                //===== set up parameters ============//
                cmd.Parameters.AddWithValue("IdUser", HttpContext.Session.GetInt32("userid"));
                cmd.Parameters.AddWithValue("ProgramType", ProgramType);
                cmd.Parameters.AddWithValue("Major", Major);
                cmd.Parameters.AddWithValue("Location", Location);
                cmd.Parameters.AddWithValue("School_Size", School_Size);
                cmd.Parameters.AddWithValue("ACT_Score", ACT_Score);
                cmd.Parameters.AddWithValue("SAT_Score", SAT_Score);
                cmd.Parameters.AddWithValue("GPA", GPA);
                cmd.Parameters.AddWithValue("GPA_outof", GPA_outof);

                //type of command
                cmd.CommandType = CommandType.StoredProcedure;

                //open connection
                conn.Open();

                //execute command
                cmd.ExecuteNonQuery();

            }
            //redirect to information page
            return RedirectToAction("StudentProfile", "StudentProfile");
        }

        //this function takes a user id and
        //returns a StudentProfile object with the information
        public StudentProfile GetSPInformation(int? IdUser)
        {
            StudentProfile current_SP = new StudentProfile(); //local
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    //procedure
                    SqlCommand cmd = new SqlCommand("sp_getSP", conn); //procedure

                    //===== set up procesure's parameters ============//
                    cmd.Parameters.AddWithValue("@IdUser", IdUser);

                    //command type
                    cmd.CommandType = CommandType.StoredProcedure;

                    //open connection
                    conn.Open();

                    //====sql query ==//
                    using (SqlDataReader Reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (Reader.Read() && Reader.HasRows)
                        {
                            //get values
                            current_SP.ProgramType = Reader["ProgramType"].ToString();
                            current_SP.Major = Reader["Major"].ToString();
                            current_SP.Location = Reader["Location"].ToString();
                            current_SP.School_Size = Reader["School_Size"].ToString();
                            current_SP.ACT_Score = (int)Reader["ACT_Score"];
                            current_SP.SAT_Score = (int)Reader["SAT_Score"];
                            current_SP.GPA = (float)(Double)Reader["GPA"];
                            current_SP.GPA_outof = (float)(Double)Reader["GPA_outof"];
                        }
                    }
                    //conn
                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
            return current_SP; //return list of files
        }
    }
}

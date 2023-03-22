using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BoJo.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BoJo.Controllers
{
    public class MatchesController : Controller
    {
        //connection string
        string connString = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        // GET: /<controller>/
        public IActionResult DisplayUniversities()
        {
            if (HttpContext.Session.GetInt32("userid") == null)
            {
                return RedirectToAction("Login", "Access");
            }

            List<UniversityMatches> Institutions = getMatches(); //call get match function and get a list of 3 matches
            if (Institutions == null)
            {
                //redirect to student profile if insuficient information
                return RedirectToAction("StudentProfile", "StudentProfile");
            }
            return View(Institutions);
        }

        /*
         getMatches returns a list of 3 UniversityMatches after getting the
        top 3 universities that matches the user preferences 
         */
        public List<UniversityMatches> getMatches()
        {
            //variables initiation
            int pref_size = 3;  //amount of universities matches
            List<Institution> institutions = new List<Institution>(new Institution[pref_size]);
            List<UniversityMatches> matches = new List<UniversityMatches>();

            //get student preferences
            StudentProfile SP = GetSPInformation(HttpContext.Session.GetInt32("userid"));

            //check if necesary fields are filled
            if (SP.Major == null || SP.Location == null || SP.School_Size == null || SP.Competitive==null || SP.SportsOveralLevel==null || SP.ProgramType == null)
            {
                return null;
            }

            //SQL connection
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //command get all the universities
                SqlCommand MessageCmd = new SqlCommand("SELECT * FROM Institutions", conn);

                conn.Open(); //open connection
                using (SqlDataReader Reader = MessageCmd.ExecuteReader())
                {
                    while (Reader.Read())
                    {
                        var current_institution = new Institution();

                        current_institution.institutionID = (int)Reader["institutionID"];
                        current_institution.name = Reader["name"].ToString();
                        current_institution.about = Reader["about"].ToString();
                        current_institution.region = Reader["region"].ToString();
                        current_institution.size = Reader["size"].ToString();

                        //new
                        current_institution.Competitive = Reader["Competitive"].ToString();
                        current_institution.SportsOveralLevel = Reader["SportsOveralLevel"].ToString();
                        current_institution.RatioStudentFaculty = (int)Reader["RatioStudentFaculty"];
                        current_institution.GraduateProgram = Convert.ToBoolean(Reader["GraduateProgram"].ToString());
                        //
                        
                        current_institution.majors = JsonConvert.DeserializeObject<List<string>>(Reader["majors"].ToString());
                        current_institution.total_cost = (float)(Double)Reader["total_cost"];

                        //current_institution.average_GPA = (float)(Double)Reader["average_GPA"];
                        //current_institution.average_SAT = (int)Reader["average_SAT"];
                        //current_institution.average_ACT = (int)Reader["average_ACT"];
                        //current_institution.acceptance_rate = (float)(Double)Reader["acceptance_rate"];
                        //current_institution.graduation_rate = (float)(Double)Reader["graduation_rate"];
                        //current_institution.average_cost_after_aid = (float)(Double)Reader["average_cost_after_aid"];
                        //current_institution.apply_url = Reader["apply_url"].ToString();
                        //current_institution.website_url = Reader["website_url"].ToString();
                        //current_institution.level = Reader["level"].ToString();
                        //current_institution.control = Reader["control"].ToString();
                        //current_institution.address = Reader["address"].ToString();
                        //current_institution.city = Reader["city"].ToString();
                        //current_institution.state = Reader["state"].ToString();
                        //current_institution.zip = Reader["zip"].ToString();
                        //current_institution.tel_num = Reader["tel_num"].ToString();

                        //============== CALCULATE ================//
                        //get match % for current institution
                        float current_score = CalcPreferenceMatch(SP, current_institution);
                        current_institution.preference_match_percent = current_score;

                        //how many nulls in list
                        int nulls = institutions.Count(i => i == null);

                        //check if there any item with a match % less than the current institution's match %
                        //also takes in consideration cases where the array is empty
                        if (nulls != 0 || institutions.Any(i => i.preference_match_percent >= current_score))
                        {
                            //insert university
                            if (nulls != 0)
                            {
                                institutions[nulls - 1] = current_institution;
                            }
                            else
                            {
                                institutions[pref_size - 1] = current_institution;
                                institutions.OrderByDescending(i => i.preference_match_percent).ToList();
                            }
                        }
                    }
                }

                //sort on Descending Order
                institutions = institutions.OrderByDescending(i => i.preference_match_percent).Take(pref_size).ToList();

                //Copy to Jonathan's format
                int index = 0;
                foreach (Institution i in institutions)
                {
                    UniversityMatches current = new UniversityMatches();
                    current.UniversityNumber = i.institutionID;
                    current.UniversityName = i.name;
                    current.Size = i.size;
                    current.TuitonAndFees = i.total_cost;
                    current.Overview = i.about;
                    current.preference_match_percent = i.preference_match_percent;
                    current.admission_match_percent = 50;//as for now

                    matches.Add(current);
                }

            }

            return matches; //return result
        }

        //this function compares the user preferencences with an university's characteristics
        public float CalcPreferenceMatch(StudentProfile SP, Institution CI)
        {
            double score = 0;
            double possible_score = 8;
            if (SP.Location == CI.region || SP.Location=="Any") { score += 1; }
            if (SP.School_Size == CI.size || SP.School_Size == "Any") { score += 1; }
            if (CI.majors.Contains(SP.Major) || SP.Major == "Undecided") { score += 1; }
            if (SP.Competitive == CI.Competitive || SP.Competitive == "Any") { score += 1; }
            if (SP.SportsOveralLevel == CI.SportsOveralLevel || SP.SportsOveralLevel == "Any") { score += 1; }
            if (SP.RatioStudentFaculty == CI.RatioStudentFaculty || SP.RatioStudentFaculty == 0) { score += 1; }
            if ((SP.ProgramType == "Graduate" && CI.GraduateProgram == true) || SP.ProgramType == "Undergraduate") { score += 1; }
            if (SP.MaxCostAfterAid>=CI.average_cost_after_aid || SP.MaxCostAfterAid==0) { score += 1; }

            return (float)Math.Round(score / possible_score * 100, 2);
        }


        //This function is a copy from the GetSPInformation under StudentProfile.
        //but this one is for this environment
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
                            current_SP.Competitive = Reader["competitive"].ToString();
                            current_SP.SportsOveralLevel = Reader["SportsOveralLevel"].ToString();
                            current_SP.MaxCostAfterAid = (int)Reader["MaxCostAfterAid"];
                            current_SP.RatioStudentFaculty = (int)Reader["RatioStudentFaculty"];
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
   


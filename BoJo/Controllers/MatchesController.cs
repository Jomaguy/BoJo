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
            List<UniversityMatches> matches = new List<UniversityMatches>(new UniversityMatches[pref_size*3]);
            List<Institution> safe_schools = new List<Institution>(new Institution[pref_size]);
            List<Institution> fit_schools = new List<Institution>(new Institution[pref_size]);
            List<Institution> reach_schools = new List<Institution>(new Institution[pref_size]);

            //get student preferences
            StudentProfile SP = GetSPInformation(HttpContext.Session.GetInt32("userid"));

            //check if necesary fields are filled
            if (SP.Major=="")
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
                        current_institution.size = (int)Reader["size"];

                        //new
                        current_institution.Competitive = Reader["Competitive"].ToString();
                        current_institution.SportsOveralLevel = Reader["SportsOveralLevel"].ToString();
                        current_institution.RatioStudentFaculty = (int)Reader["RatioStudentFaculty"];
                        current_institution.GraduateProgram = Convert.ToBoolean(Reader["GraduateProgram"].ToString());
                        current_institution.majors = JsonConvert.DeserializeObject<List<string>>(Reader["majors"].ToString());
                        current_institution.total_cost = (float)(Double)Reader["total_cost"];
                        current_institution.average_cost_after_aid = (float)(Double)Reader["average_cost_after_aid"];
                        current_institution.control = Reader["control"].ToString();
                        
                        //newshii=======
                        current_institution.climate = Reader["climate"].ToString();
                        current_institution.dorming_percentage = (float)(Double)Reader["dorming_percentage"];
                        current_institution.greek_life = Convert.ToBoolean(Reader["greek_life"].ToString());
                        current_institution.SAT_25th = (int)Reader["SAT_25th"];
                        current_institution.SAT_75th = (int)Reader["SAT_75th"];
                        current_institution.ACT_25th = (int)Reader["ACT_25th"];
                        current_institution.ACT_75th = (int)Reader["ACT_75th"];
                        current_institution.GPA_25th = (float)(Double)Reader["GPA_25th"];
                        current_institution.GPA_75th = (float)(Double)Reader["GPA_75th"];

                        //============== CALCULATE ================//
                        //get match % for current institution
                        float current_score = CalcPreferenceMatch(SP, current_institution);
                        float admissionchances = EstimateAdmission(SP.SAT_Score,SP.ACT_Score,SP.GPA, current_institution.SAT_25th, current_institution.SAT_75th, current_institution.ACT_25th, current_institution.ACT_75th, current_institution.GPA_25th, current_institution.ACT_75th);
                        current_institution.preference_match_percent = current_score;
                        current_institution.admission_match_percent = admissionchances;

                        current_institution.picture = Reader["picture"].ToString();
                        //=============== INSERT IN CORRESPONDING CATEGORY ===========//
                        //safe
                        if (admissionchances >= 80.0)
                        {
                            //how many nulls in list
                            int s_nulls = safe_schools.Count(i => i == null);
                            //check if there any item with a match % less than the current institution's match %
                            //also takes in consideration cases where the array is empty
                            if (s_nulls != 0 || safe_schools.Any(i => i.preference_match_percent * i.admission_match_percent <= current_score*admissionchances))
                            {
                                //insert university
                                if (s_nulls != 0)
                                {
                                    //if there are empty spaces
                                    safe_schools[s_nulls - 1] = current_institution;
                                }
                                else
                                {
                                    //if there are no empty spaces we have to substitude the one with the lowest preference*admission 
                                    safe_schools = safe_schools.OrderBy(i => i.preference_match_percent * i.admission_match_percent).ToList();
                                    safe_schools[0] = current_institution;
                                }
                            }

                        } //safe

                        //fit from 40-80
                        else if (admissionchances < 80.0 && admissionchances >= 40.0)
                        {
                            //how many nulls in list
                            int f_nulls = fit_schools.Count(i => i == null);

                            if (f_nulls != 0 || fit_schools.Any(i => i.preference_match_percent * i.admission_match_percent <= current_score * admissionchances))
                            {
                                //insert university
                                if (f_nulls != 0)
                                {
                                    fit_schools[f_nulls - 1] = current_institution;
                                }
                                else
                                {
                                    fit_schools = fit_schools.OrderBy(i => i.preference_match_percent*i.admission_match_percent).ToList();
                                    fit_schools[0] = current_institution;
                                }
                            }

                        } //fit

                        //reach
                        else {
                            //how many nulls in list
                            int r_nulls = reach_schools.Count(i => i == null);
                            if (r_nulls != 0 || reach_schools.Any(i => i.preference_match_percent * i.admission_match_percent <= current_score * admissionchances))
                            {
                                //insert university
                                if (r_nulls != 0)
                                {
                                    reach_schools[r_nulls - 1] = current_institution;
                                }
                                else
                                {
                                    reach_schools = reach_schools.OrderBy(i => i.preference_match_percent * i.admission_match_percent).ToList();
                                    reach_schools[0] = current_institution;
                                }
                            }
                        }
                        //done getting match categories
                    }
                }

                //sort on Descending Order - keep NULL if needed
                safe_schools = safe_schools.OrderByDescending(i => i == null ? -1 : i.admission_match_percent).ToList();
                fit_schools = fit_schools.OrderByDescending(i => i==null ? -1 : i.admission_match_percent).ToList();
                reach_schools = reach_schools.OrderByDescending(i => i == null ? -1 : i.admission_match_percent).ToList();

                //make into Jonathan's format for displying 
                int index = 0;

                //insert safe schools
                foreach (Institution ss in safe_schools)
                {
                    UniversityMatches safe = new UniversityMatches();
                    if (ss != null)
                    {
                        safe.picture = ss.picture;
                        safe.UniversityNumber = ss.institutionID;
                        safe.UniversityName = ss.name;
                        safe.Size = ss.size;
                        safe.TuitonAndFees = ss.total_cost;
                        safe.Overview = ss.about;
                        safe.preference_match_percent = ss.preference_match_percent;
                        safe.admission_match_percent = ss.admission_match_percent;//as for now
                    }
                    matches[index] = safe;
                    index++;
                }

                //insert fit schools
                foreach (Institution fs in fit_schools)
                {
                    UniversityMatches fit = new UniversityMatches();
                    if (fs != null)
                    {
                        fit.picture = fs.picture;
                        fit.UniversityNumber = fs.institutionID;
                        fit.UniversityName = fs.name;
                        fit.Size = fs.size;
                        fit.TuitonAndFees = fs.total_cost;
                        fit.Overview = fs.about;
                        fit.preference_match_percent = fs.preference_match_percent;
                        fit.admission_match_percent = fs.admission_match_percent;//as for now
                    }
                    matches[index] = fit;
                    index++;
                }

                //insert reach schools
                foreach (Institution rs in reach_schools)
                {
                    UniversityMatches reach = new UniversityMatches();
                    if (rs != null)
                    {
                        reach.picture = rs.picture;
                        reach.UniversityNumber = rs.institutionID;
                        reach.UniversityName = rs.name;
                        reach.Size = rs.size;
                        reach.TuitonAndFees = rs.total_cost;
                        reach.Overview = rs.about;
                        reach.preference_match_percent = rs.preference_match_percent;
                        reach.admission_match_percent = rs.admission_match_percent;//as for now
                    }
                    matches[index] = reach;
                    index++;
                }
            }

            return matches; //return result
        }

        //this function compares the user preferencences with an university's characteristics
        public float CalcPreferenceMatch(StudentProfile SP, Institution CI)
        {
            double score = 0;
            double possible_score = 13;
            if (SP.Location == CI.region || SP.Location=="Any") { score += 1; }
            if (SP.School_Size > CI.size || SP.School_Size == 100000) { score += 1; }
            if (CI.majors.Contains(SP.Major) || SP.Major == "Undecided") { score += 1; }
            if (SP.Competitive == CI.Competitive || SP.Competitive == "Any") { score += 1; }
            if (SP.SportsOveralLevel == CI.SportsOveralLevel || SP.SportsOveralLevel == "Any") { score += 1; }
            if (SP.RatioStudentFaculty > CI.RatioStudentFaculty || SP.RatioStudentFaculty == 0) { score += 1; }
            if ((SP.ProgramType == "Graduate" && CI.GraduateProgram == true) || SP.ProgramType == "Undergraduate") { score += 1; }
            if (SP.MaxCostAfterAid>=CI.average_cost_after_aid || SP.MaxCostAfterAid==0) { score += 1; }
            
            if (SP.greek_life == "Any" || (CI.greek_life == true && SP.greek_life=="Yes") || (CI.greek_life == false && SP.greek_life == "No")) { score += 1; }
            if (CI.dorming_percentage <= SP.dorming_percentage) { score += 1; }
            if (CI.climate == SP.climate) { score += 1; }
            if (CI.control == SP.control) { score += 1; }
            if (CI.majors.Contains(SP.minor) || SP.minor == "Undecided") { score += 1; }


            return (float)Math.Round(score / possible_score * 100, 2);
        }
        public float EstimateAdmission(float satScore, float actScore, float gpa, float sat25, float sat75, float act25, float act75, float gpa25, float gpa75)
        {
            float satNorm = (satScore - sat25) / (sat75 - sat25);
            float actNorm = (actScore - act25) / (act75 - act25);
            float gpaNorm = (gpa - gpa25) / (gpa75 - gpa25);

            //return (satNorm + satNorm + gpaNorm) / 3;
            return (float)Math.Round((0.4 * satNorm + 0.3 * actNorm + 0.3 * gpaNorm)*100,2);
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
                            current_SP.School_Size = (int)Reader["School_Size"];
                            current_SP.Competitive = Reader["competitive"].ToString();
                            current_SP.SportsOveralLevel = Reader["SportsOveralLevel"].ToString();
                            current_SP.MaxCostAfterAid = (int)Reader["MaxCostAfterAid"];
                            current_SP.RatioStudentFaculty = (int)Reader["RatioStudentFaculty"];
                            current_SP.ACT_Score = (int)Reader["ACT_Score"];
                            current_SP.SAT_Score = (int)Reader["SAT_Score"];
                            current_SP.GPA = (float)(Double)Reader["GPA"];
                            current_SP.GPA_outof = (float)(Double)Reader["GPA_outof"];
                            current_SP.dorming_percentage = (float)(Double)Reader["dorming_percentage"];
                            current_SP.climate = Reader["climate"].ToString();
                            current_SP.control = Reader["control"].ToString();
                            current_SP.minor = Reader["minor"].ToString();
                            current_SP.greek_life = Reader["greek_life"].ToString();
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
   


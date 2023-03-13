using BoJo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BoJo.Controllers
{
    public class InstitutionsController : Controller
    {
        //sql connection string
        protected static string connString = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public IActionResult Institution()
        {
            Institution institution = GetInstitution(1);
            if(institution.intitutionID == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["institution"] = institution;
            return View();
        }
        
        public Institution GetInstitution(int id)
        {
            Institution current_institution = new Institution(); //local
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    //procedure
                    SqlCommand cmd = new SqlCommand("select * from institutions where intitutionID=@id", conn); //procedure

                    //===== set up procesure's parameters ============//
                    cmd.Parameters.AddWithValue("id", id);

                    //command type
                    //cmd.CommandType = CommandType.StoredProcedure;

                    //open connection
                    conn.Open();

                    //====sql query ==//
                    using (SqlDataReader Reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (Reader.Read() && Reader.HasRows)
                        {
                            //get values
                            current_institution.intitutionID = (int)Reader["intitutionID"];
                            current_institution.name = Reader["name"].ToString();
                            current_institution.about = Reader["about"].ToString();
                            current_institution.address = Reader["address"].ToString();
                            current_institution.city = Reader["city"].ToString();
                            current_institution.state = Reader["state"].ToString();
                            current_institution.zip = Reader["zip"].ToString();
                            current_institution.tel_num = Reader["tel_num"].ToString();
                            current_institution.region = Reader["region"].ToString();
                            current_institution.level = Reader["level"].ToString();
                            current_institution.control = Reader["control"].ToString();
                            current_institution.size = Reader["size"].ToString();
                            current_institution.acceptance_rate = (float)(Double)Reader["acceptance_rate"];
                            current_institution.graduation_rate = (float)(Double)Reader["graduation_rate"];
                            current_institution.total_cost = (float)(Double)Reader["total_cost"];
                            current_institution.average_cost_after_aid = (float)(Double)Reader["average_cost_after_aid"];
                            current_institution.apply_url = Reader["apply_url"].ToString();
                            current_institution.website_url = Reader["website_url"].ToString();
                            current_institution.majors = Reader["majors"].ToString();
                            current_institution.average_GPA = (float)(Double)Reader["average_GPA"];
                            current_institution.average_SAT = (int)Reader["average_SAT"];
                            current_institution.average_ACT = (int)Reader["average_ACT"];
                        }
                    }
                    //conn
                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
            return current_institution; //return list of files
        }
    }
}

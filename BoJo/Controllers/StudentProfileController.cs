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
        string connString = "Server=tcp:secondbojoserver.database.windows.net,1433;Initial Catalog=Bojo;Persist Security Info=False;User ID=Adminuser;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        //major list
        private List<string> majors = new List<string>()
        {
            "Accounting", "Acting", "Actuarial Science", "African American Studies", "African Languages", "African Studies", "Africana Studies", "Africana and Puerto Rican/Latino Studies", "Agricultural Education and Communication", "Agricultural Operations Management", "Agricultural and Biological Engineering", "Agricultural and Life Sciences Education", "Agricultural and Natural Resources Policy", "Agronomy", "American Sign Language", "American Studies", "Animal Sciences", "Anthropology", "Anthropology and Geography", "Applied Mathematics", "Applied Mathematics and Computer Science", "Applied Physiology and Kinesiology", "Applied Psychology", "Applied and Computational Mathematics", "Archaeology", "Architectural Studies", "Architecture", "Art", "Art History", "Art and Archaeology", "Art and Art History", "Asian Studies", "Asian/Pacific/American Studies", "Astronomy", "Astrophysical Sciences", "Athletic Training", "Biochemistry", "Bioinformatics", "Biological Engineering", "Biological Sciences", "Biology", "Biomedical Engineering", "Botany", "Building Construction", "Business & Political Economy", "Business Administration", "Business Analytics", "Business Economics", "Business Studies", "Chemical Engineering", "Chemical and Biological Engineering", "Chemistry", "Childhood Education (Grades 1-6)", "Chinese", "Cinema Studies", "Civil Engineering", "Civil and Environmental Engineering", "Classical Studies", "Classics", "Clinical Laboratory Sciences", "Cognitive Science", "Communication", "Communication Sciences and Disorders", "Communications", "Community Health", "Comparative Literature", "Computer Engineering", "Computer Engineering Technology", "Computer Science", "Construction Management", "Creative Writing", "Criminal Justice", "Criminology", "Dance", "Data Science", "Digital Arts and Sciences", "Digital Media", "Drama", "Drama Therapy", "Dramatic Writing", "Early Childhood Education", "Earth and Atmospheric Sciences", "East Asian Languages and Literatures", "East Asian Studies", "Ecology and Evolutionary Biology", "Economics", "Education", "Education Sciences", "Education Studies", "Electrical Engineering", "Elementary Education", "Elementary Education (Grades 1-6)", "Energy and Sustainability", "Engineering", "Engineering Physics", "Engineering Technology", "English", "Entomology and Nematology", "Entrepreneurship", "Environmental Earth Science", "Environmental Engineering Sciences", "Environmental Geosciences", "Environmental Resources", "Environmental Science", "Environmental Studies", "European & Mediterranean Studies", "Exercise Science", "Family, Youth and Community Sciences", "Film & Television", "Film Studies", "Film and Media Studies", "Finance", "Fine Arts", "Food Science", "Food and Resource Economics", "Forensic Science", "Forest Resources and Conservation", "French", "French and Italian", "Game Design", "Gender & Sexuality Studies", "Geography", "Geology", "Geomatics", "Geosciences", "German", "Global Liberal Studies", "Global Public Health", "Global Studies", "Graphic Communications Management & Technology", "Greek", "Greek & Roman Civilization", "Health", "Health Communication", "Health Education and Behavior", "Health Promotion", "Health Science", "Hebrew", "History", "History of Science", "Honors", "Horticultural Sciences", "Hospitality and Tourism Management", "Hotel & Tourism Management", "Hotel and Restaurant Management", "Human Development and Family Studies", "Human Nutrition and Foods", "Human Resource Development", "Human Resource Management", "Human Rights", "Independent Concentration", "Individualized Study", "Industrial Design", "Industrial Engineering", "Industrial and Systems Engineering", "Information Systems", "Information Systems and Operations Management", "Information and Logistics Technology", "Integrated Accounting Studies", "Integrated Business 3+1 Program", "Integrated Digital Media", "Integrative Neuroscience", "Interdisciplinary Arts", "Interdisciplinary Studies", "Interior Architecture", "Interior Design", "International Business", "International Relations", "International Studies", "Italian", "Italian Studies", "Japanese", "Jewish Studies", "Journalism", "Judaic Studies", "Kinesiology", "LGBTQ Studies", "Landscape and Nursery Horticulture", "Latin", "Latin American Studies", "Latin American and Caribbean Studies", "Latin Language & Literature", "Latin Language and Literature", "Latino/a and Mexican American Studies", "Law", "Legal Education Accelerated Program (LEAP)", "Liberal Arts and Sciences", "Linguistics", "Management", "Management Information Systems", "Management of Information Systems", "Marine Sciences", "Marketing", "Materials Engineering", "Materials Science and Engineering", "Mathematics", "Mechanical Engineering", "Mechanical and Aerospace Engineering", "Media & Communication", "Media Studies", "Media and Communication Studies", "Media, Culture, and Communication", "Medical Geography", "Medical Humanities", "Medical Humanities and Health Studies", "Medical Sciences", "Medical Technology", "Medieval Studies", "Medieval and Renaissance Studies", "Microbiology and Cell Science", "Middle Eastern Languages and Cultures", "Military Science", "Modern and Classical Languages", "Molecular Biology", "Music", "Music Education", "Music Merchandising", "Music Theory", "Musical Theater", "Natural Resource Conservation", "Near Eastern Studies", "Neural Science", "Neuroscience", "Nuclear and Radiological Engineering", "Nursing", "Nutrition", "Nutritional Sciences", "Occupational Therapy", "Operations & Supply Chain Management", "Operations Management", "Operations Research and Financial Engineering", "Optics and Photonics", "Organizational Behavior", "Organizational Leadership", "Parasitology", "Peace Studies", "Petroleum Engineering", "Pharmacy", "Philosophy", "Physical Education", "Physician Assistant", "Physics", "Physics and Astronomy", "Plant Science", "Political Science", "Politics", "Population Studies", "Portuguese", "Pre-Dental", "Pre-Health", "Pre-Law", "Pre-Medical", "Pre-Occupational Therapy", "Pre-Physical Therapy", "Pre-Veterinary", "Psychology", "Public Health", "Public Policy & Management", "Public Relations", "Public Service", "Publishing Studies", "Radio Production", "Reading", "Real Estate", "Recreation, Parks and Tourism", "Religion", "Religious Studies", "Retailing and Consumer Science", "Romance Languages and Literatures", "Russian", "Russian & Slavic Studies", "Russian Studies", "School of Art and Art History", "School of Music", "Science Education", "Secondary Education", "Secondary Education (Grades 7-12)", "Slavic Languages and Literatures", "Social & Cultural Analysis", "Social Entrepreneurship", "Social Media and Society", "Social Work", "Sociology", "Soil and Water Sciences", "Spanish", "Spanish & Portuguese", "Spanish and Portuguese", "Special Education", "Speech-Language-Hearing Sciences", "Sport Management", "Sports Administration", "Sports Management", "Sports Science", "Statistics", "Studio Art", "Supply Chain Management", "Supply Chain and Logistics Technology", "Sustainability", "Sustainability Studies", "Sustainable Urban Environments", "Technology Leadership and Innovation", "Telecommunication", "Television & Radio", "Television Production", "Theater", "Theater Arts Education", "Theater and Dance", "Theatre", "Tourism, Hospitality and Event Management", "Urban Design & Architecture Studies", "Urban Studies", "Urban and Public Affairs", "Video/Television", "Web Development", "Wildlife Ecology and Conservation", "Women and Gender Studies", "Women's Studies", "Women's, Gender and Sexuality Studies", "Writing", "Writing Studies", "Writing and Rhetoric", "Writing for Film & Television", "Yiddish Studies"
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
        public IActionResult StudentProfileForm(string ProgramType, 
            string Major, string Location, int School_Size, 
            string Competitive, string SportsOveralLevel, int MaxCostAfterAid, 
            int RatioStudentFaculty, int ACT_Score, int SAT_Score, float GPA,
            float GPA_outof, string greek_life,float dorming_percentage,string climate,
            string control, string minor)
        {
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
                cmd.Parameters.AddWithValue("Competitive", Competitive);
                cmd.Parameters.AddWithValue("SportsOveralLevel", SportsOveralLevel);
                cmd.Parameters.AddWithValue("MaxCostAfterAid", MaxCostAfterAid);
                cmd.Parameters.AddWithValue("RatioStudentFaculty", RatioStudentFaculty);
                cmd.Parameters.AddWithValue("ACT_Score", ACT_Score);
                cmd.Parameters.AddWithValue("SAT_Score", SAT_Score);
                cmd.Parameters.AddWithValue("GPA", GPA);
                cmd.Parameters.AddWithValue("GPA_outof", GPA_outof);
                cmd.Parameters.AddWithValue("greek_life", greek_life);
                cmd.Parameters.AddWithValue("dorming_percentage", dorming_percentage);
                cmd.Parameters.AddWithValue("climate", climate);
                cmd.Parameters.AddWithValue("control", control);
                cmd.Parameters.AddWithValue("minor", minor);

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
                            //string Competitive, string SportsOveralLevel, int MaxCostAfterAid, int RatioStudentFaculty
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

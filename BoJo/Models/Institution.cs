namespace BoJo.Models
{
    public class Institution
    {
        public int institutionID { get; set; }
        public string name { get; set; }
        public string about { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string tel_num { get; set; }
        public string region { get; set; }
        public string level { get; set; }
        public string control { get; set; }
        public int size { get; set; }
        public string Competitive { get; set; }
        public string SportsOveralLevel { get; set; }
        public int RatioStudentFaculty { get; set; }
        public bool GraduateProgram { get; set; }
        public float acceptance_rate { get; set; }
        public float graduation_rate { get; set; }
        public float total_cost { get; set; }
        public float average_cost_after_aid { get; set; }
        public string apply_url { get; set; }
        public string website_url { get; set; }
        public List<string> majors { get; set; }
        public string climate { get; set; }
        public float dorming_percentage { get; set; }
        public float average_GPA { get; set; }
        public int average_SAT { get; set; }
        public int average_ACT { get; set; }
        public bool greek_life { get; set; }
        public int SAT_25th { get; set; }
        public int SAT_75th { get; set; }
        public int ACT_25th { get; set; }
        public int ACT_75th { get; set; }
        public float GPA_25th { get; set; }
        public float GPA_75th { get; set; }
        public float preference_match_percent { get; set; }
        public float admission_match_percent { get; set; }
        public string picture { get; set; }
    }
}

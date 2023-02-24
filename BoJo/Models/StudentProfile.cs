namespace BoJo.Models
{
    public class StudentProfile
    {
        public String ProgramType { get; set; }
        public String Major { get; set; }
        public String Location { get; set; }
        public String School_Size { get; set; }
        public int ACT_Score { get; set; }
        public int SAT_Score { get; set; }
        public float GPA { get; set; }
        public float GPA_outof { get; set; }

    }
    public class Major
    {
        public String Id { get; set; }
        public string Title { get; set; }
    }
}

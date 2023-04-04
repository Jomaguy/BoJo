namespace BoJo.Models
{
    public class User
    {
        public int IdUser { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ComfirmPassword { get; set; }
        public string DOB { get; set; }
        public string Role { get; set; } = "regular";
        public bool Confirmed { get; set; }
    }
}
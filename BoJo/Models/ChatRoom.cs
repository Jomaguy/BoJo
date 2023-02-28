namespace BoJo.Models
{
    public class ChatRoom
    {
        public int ChatRoomId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime Created { get; set; }= DateTime.Now;
        public int UserId { get; set; }
        //public User User { get; set; }
        //public List<Message> messages { get; set; }
    }
}

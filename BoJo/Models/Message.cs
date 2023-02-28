namespace BoJo.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public int ChatRoomId { get; set; }
        public int UserId { get; set; }

    }
}

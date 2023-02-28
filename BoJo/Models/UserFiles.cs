namespace BoJo.Models
{
    //This class helps to model the data comming from the sql table
    public class UserFiles
    {
        public int FileId { get; set; }
        public int UserId { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
    }
}

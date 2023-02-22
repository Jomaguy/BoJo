using BoJo.Models;

public interface IBufferedFileUploadService
{
    Task<bool> UploadFile(IFormFile file,int? userid);
    List<UserFiles> GetFiles(int? userid);
}
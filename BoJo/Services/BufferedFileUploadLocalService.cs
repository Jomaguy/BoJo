using System.Data.SqlClient;
using BoJo.Models;
using BoJo.Controllers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Data;
using System.Web;

public class BufferedFileUploadLocalService : IBufferedFileUploadService
{
    //connection string
    string connString = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

    //======= UploadFile ========/
    //this function takes a file and user id.
    //it first serialzies the file to byte array
    //then uploads it to a database using a procedure
    public async Task<bool> UploadFile(IFormFile file,int? userid)
    {
        try
        {
            int result = 0;
            //if there is a file 
            if (file != null && file.Length > 0)
            {
                //translate file to byte array
                var ms = new MemoryStream();
                file.CopyTo(ms);
                byte[] bytes3 = ms.ToArray();

                //===== connection to sql=====//
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("sp_FileUpload_Insert", conn); //procedure

                    //===== set up procesure's parameters ============//
                    cmd.Parameters.AddWithValue("UserId", userid);
                    cmd.Parameters.AddWithValue("@ContentType", file.ContentType.ToString());
                    cmd.Parameters.AddWithValue("@FileName", file.FileName.ToString());
                    cmd.Parameters.AddWithValue("@Data", bytes3);

                    //command type
                    cmd.CommandType = CommandType.StoredProcedure;

                    //open connection
                    conn.Open();

                    //execute command
                    result = cmd.ExecuteNonQuery(); //return 1 or 0
                    conn.Close();
                }
                return (result == 1); //if the insertion was successful
            }
            //else
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("File Copy Failed", ex);
        }
    }//UploadFile


    //======GetFiles ======//
    //this function retrives all the files for a 
    //specific user id. It is usually call when 
    //first accessing the File uploader index
    public List<UserFiles> GetFiles(int? userid)
    {
        List<UserFiles> user_files = new List<UserFiles>(); //local
        try
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //procedure
                SqlCommand cmd = new SqlCommand("sp_FileUpload_Get", conn); //procedure

                //===== set up procesure's parameters ============//
                cmd.Parameters.AddWithValue("@UserId", userid);

                //command type
                cmd.CommandType = CommandType.StoredProcedure;

                //open connection
                conn.Open();

                using (SqlDataReader Reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (Reader.HasRows)
                    {
                        //COLLECT FILES INFO
                        //loop all the lines retrived from sql
                        while (Reader.Read())
                        {
                            //get file info
                            var UF = new UserFiles(); //userfile
                            UF.FileId = (int)Reader["FileId"];
                            UF.UserId = (int)Reader["UserId"];
                            UF.ContentType = Reader["ContentType"].ToString();
                            UF.FileName = Reader["FileName"].ToString();
                            UF.Data = (byte[])Reader["Data"];
                            //insert into list
                            user_files.Add(UF);
                        }
                    }
                }
                //conn
                conn.Close();
            }
        }
        catch (Exception ex)
        {

        }
        return user_files; //return list of files
    }

}
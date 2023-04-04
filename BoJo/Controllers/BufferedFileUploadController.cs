using BoJo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using BoJo.Controllers;
using Newtonsoft.Json;

namespace ProCodeGuide.Samples.FileUpload.Controllers
{
    
    public class BufferedFileUploadController : Controller
    {
        //connection string
        protected static string connString = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        readonly IBufferedFileUploadService _bufferedFileUploadService;

        public BufferedFileUploadController(IBufferedFileUploadService bufferedFileUploadService)
        {
            _bufferedFileUploadService = bufferedFileUploadService;
        }

        //Downloads a specific file by ID
        public void linkDownloadFile_onclick(int fileid)
        {

            //========  SQL Connection =======/
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand("sp_fileDownload", conn); //procedure

                //===== set up procesure's parameters ============//
                cmd.Parameters.AddWithValue("@FileId", fileid);

                //command type
                cmd.CommandType = CommandType.StoredProcedure;

                //open connection
                conn.Open();

                //Run SQL
                using (SqlDataReader Reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    //Reader contains the SQL table returned
                    if (Reader.Read() && Reader.HasRows)
                    {
                        //this if statement checks that the user downloading the file
                        //is the user that owns the file
                        if ((int)Reader["UserId"] == HttpContext.Session.GetInt32("userid"))
                        {
                            //collect information from sql
                            string filename = Reader["FileName"].ToString();
                            byte[] bytes = (byte[])Reader["Data"];
                            string contentType = Reader["ContentType"].ToString();

                            //download file
                            Response.Clear();
                            Response.Body.Dispose();
                            Response.Headers.Add("Content-Disposition", "attachment; filename=" + filename);
                            Response.ContentType = contentType;
                            Response.Body.WriteAsync(bytes, 0, bytes.Length);
                        }
                    }
                }
                conn.Close();//close conncetion
            }
        }

        public IActionResult Index()
        {
            
            //only allow logged in users
            if (HttpContext.Session.GetInt32("userid") != null)
            {
                if (HttpContext.Session.GetString("userrole") != "premium")
                {
                    return RedirectToAction("Index", "Payment");
                }
                //get new list of files
                List<UserFiles> user_files = _bufferedFileUploadService.GetFiles(HttpContext.Session.GetInt32("userid")); //get files
                //pass list to html view                                                                                     //pass the files to html view
                ViewData["UserFiles"] = user_files;
                return View();
            } 
            //if not registrated
            return RedirectToAction("Login", "Access");
        }

        [HttpPost]
        public async Task<ActionResult> Index(IFormFile file)
        {
            try
            {
                if (await _bufferedFileUploadService.UploadFile(file,HttpContext.Session.GetInt32("userid")))
                {
                    ViewBag.Message = "File Upload Successful";
                }
                else
                {
                    ViewBag.Message = "File Upload Failed";
                }
            }
            catch (Exception ex)
            {
                //Log ex
                ViewBag.Message = "File Upload Failed";
            }
            //get new list of files
            List<UserFiles> user_files = _bufferedFileUploadService.GetFiles(HttpContext.Session.GetInt32("userid")); //get files
            //pass the files to html view
            ViewData["UserFiles"] = user_files;
            return View();
            
        }
    }
}

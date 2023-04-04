using BoJo.Models;
using Microsoft.AspNetCore.Mvc;
using BoJo.Controllers;
using System.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace BoJo.Controllers
{
    public class ChatController : Controller
    {
        //MIGHT NEED TO BE CALL DIFFERENT VAR IN ACCESS CONTROLER
        private string DBSTRING = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        //FIGURE OUT HOW TO GET USER HttpContext.Session.GetInt32("userid")
        private int GetChatRoomId()
        {
            int id = -1;
            using (SqlConnection conn = new SqlConnection(DBSTRING))
            {
                SqlCommand cmd = new SqlCommand("SELECT ChatRoomId FROM ChatRoom WHERE Status = 'Active' AND UserId = @id", conn);
                //THIS WILL BE CHANGED TO USER SESSION ID;
                cmd.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("userid"));


                conn.Open();
                using (SqlDataReader Reader = cmd.ExecuteReader())
                {
                    while (Reader.Read())
                    {
                        id = (Int32)Reader["ChatRoomId"];
                    }
                }
                if(id != null && id != -1)
                {
                    return id;
                    //return 17;
                }
                else
                {

                    SqlCommand cmd2 = new SqlCommand("INSERT INTO ChatRoom (Status, Created, UserId) VALUES('Active', @time, @uid)",conn);
                    cmd2.Parameters.AddWithValue("@uid", HttpContext.Session.GetInt32("userid"));
                    cmd2.Parameters.AddWithValue("@time", DateTime.Now);

                    cmd2.ExecuteNonQuery();

                    return GetChatRoomId();
                }
            }
        }

        [Route("/Chat/ChatRoom")]
        public IActionResult ChatRoom()
        {
            if (HttpContext.Session.GetInt32("userid") == null)
            {
                return RedirectToAction("Login", "Access");
            }
            List<Message> messages = new List<Message>();
            int ChatId = GetChatRoomId();
            
            using (SqlConnection conn = new SqlConnection(DBSTRING))
            {
                SqlCommand MessageCmd = new SqlCommand("SELECT * FROM Message WHERE ChatRoomId = @id ORDER BY Created", conn);
                MessageCmd.Parameters.AddWithValue("@id", ChatId);

                conn.Open();
                using (SqlDataReader read = MessageCmd.ExecuteReader())
                {
                    while (read.Read())
                    {
                        var msg = new Message();
                        msg.MessageId = (int)read["MessageId"];
                        msg.ChatRoomId = ChatId;
                        msg.Text = (string)read["message"];
                        Console.Write(msg.Text);
                        msg.Created = (DateTime)read["Created"];
                        msg.UserId= (int)read["UserId"];
                        messages.Add(msg);
                    }
                }
                ViewData["MessageObject"] = messages;
            }

            return PartialView("_Chatroom");
        }
        [HttpGet]
        public IActionResult GetLatestMessage()
        {
            int ChatId = GetChatRoomId();

            using (SqlConnection conn = new SqlConnection(DBSTRING))
            {
                SqlCommand getMessage = new SqlCommand("SELECT * FROM Message WHERE ChatroomId = @id AND MessageId=(SELECT max(MessageId) FROM Message)", conn);
                getMessage.Parameters.AddWithValue("@id",ChatId);
                conn.Open();

                using (SqlDataReader read = getMessage.ExecuteReader())
                {
                    while (read.Read())
                    {
                        var message = new Message();
                        message.MessageId = (int)read["MessageId"];
                        message.ChatRoomId = (int)read["ChatRoomId"];
                        message.Text = (string)read["message"];
                        message.Created = (DateTime)read["Created"];
                        message.UserId = (int)read["UserId"];

                        JsonSerializer.Serialize(message);

                        return Json(message);
                    }
                }
            }
            //WANT NOTHING TO HAPPEN
            return new EmptyResult();

        }
        [HttpPost]
        public IActionResult CreateMessage(string messageBox)
        {
            //TODO: FIGURE OUT HOW DATA WILL BE PASSED OVER. VIEWDATA MOST LIKELY.
            string msg = messageBox;
            Console.Write("Hello this is MY MESSAGE!!!!" + msg);
            int id = GetChatRoomId();
            if (msg != null || msg == "")
            {
                using (SqlConnection conn = new SqlConnection(DBSTRING))
                {
                    SqlCommand CreateMsgCmd = new SqlCommand("INSERT INTO MESSAGE (message, Created, ChatRoomId, UserId) VALUES(@msg,@time,@chatid,@userid)", conn);
                    CreateMsgCmd.Parameters.AddWithValue("@msg", msg);
                    CreateMsgCmd.Parameters.AddWithValue("@time", DateTime.Now);
                    CreateMsgCmd.Parameters.AddWithValue("@chatid", id);
                    CreateMsgCmd.Parameters.AddWithValue("userid", HttpContext.Session.GetInt32("userid"));

                    conn.Open();
                    CreateMsgCmd.ExecuteNonQuery();

                    return Json(msg);
                }
            }
            //MESSAGE IS NULL -> RETURN BAD CODE
            return StatusCode(204);
        }

        public void CloseChat()
        {
            //TODO: IF USER ROLE EQUALS ADMIN CHANGE CHATROOM STATUS TO CLOSED
        }
    }
}

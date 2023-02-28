using BoJo.Models;
using Microsoft.AspNetCore.Mvc;
using BoJo.Controllers;
using System.Data.SqlClient;
using System.Data;

namespace BoJo.Controllers
{
    public class ChatController : Controller
    {
        //MIGHT NEED TO BE CALL DIFFERENT VAR IN ACCESS CONTROLER
        private string DBSTRING = BoJo.Controllers.AccessController.connString;
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
                }
                else
                {
                    Console.Write("CXReatinmg new chatroom lijkhagsdfjklhasldkhfjkals");
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
            List<Message> messages = new List<Message>();
            int ChatId = GetChatRoomId();

            using (SqlConnection conn = new SqlConnection(DBSTRING))
            {
                SqlCommand MessageCmd = new SqlCommand("SELECT * FROM Message WHERE ChatRoomId = @id AND UserId = @UID ORDER BY Created", conn);
                MessageCmd.Parameters.AddWithValue("@id", ChatId);
                MessageCmd.Parameters.AddWithValue("@UID", HttpContext.Session.GetInt32("userid"));

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

            return View();
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
                }
            }
            return RedirectToAction("ChatRoom");
        }
        
        public void CloseChat()
        {
            //TODO: IF USER ROLE EQUALS ADMIN CHANGE CHATROOM STATUS TO CLOSED
        }
    }
}

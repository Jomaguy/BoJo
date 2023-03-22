﻿using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace BoJo.Controllers
{
    public class PaymentController : Controller
    {
        //connection string
        string connString = "Server=tcp:bojosqlserver.database.windows.net,1433;Initial Catalog=BoJo;Persist Security Info=False;User ID=warlynrn;Password=BoJo2023@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BecomePremium(string data)
        {
            var s = data;

            //update localdata
            HttpContext.Session.SetString("userrole", "premium");

            //update database
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //procedure
                SqlCommand cmd = new SqlCommand("" +
                    "update bojo_users "+
                    "set role = 'premium'"+
                    "where IdUser = @id",
                    conn); //query

                //===== set up procesure's parameters ============//
                cmd.Parameters.AddWithValue("id", HttpContext.Session.GetInt32("userid"));

                //open connection
                conn.Open();

                //====sql query ==//
                cmd.ExecuteReader(CommandBehavior.CloseConnection);
                //conn
                conn.Close();
            }
             return RedirectToAction("Index", "Home");

        }
    }
}
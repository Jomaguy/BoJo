using System.Net;
using System.Net.Mail;

namespace BoJo.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string body)
        {
            string mail = "Bojo_Testing@outlook.com";
            string pwd = "BoJo2023@";

            var client = new SmtpClient("smtp-mail.outlook.com", 587)
            {
                EnableSsl = true,
                //UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mail, pwd)
            };

            return client.SendMailAsync(
                new MailMessage(
                        from: mail,
                        to: email,
                        subject: subject,
                        body: body
                    )
                );
        }
    }
}

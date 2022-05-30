using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MimeKit;
using NLog;

namespace CRM_Upack_kz.Jobs
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string emailClient, string subject, string text)
        {
            try
            {
                var emailAdmin = "Alex_til@mail.ru";
                var pass = "PTTQUZU1wCmakFcbcpTP";
                
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Upack.kz", emailAdmin));
                message.To.Add(new MailboxAddress("Администрация","alextil88@gmail.com"));
                message.Subject = subject;
                message.Body = new BodyBuilder(){HtmlBody = $"<h2>Здравствуйте наступил срок по данным обращениям:</h2>\n <p>{text}</p>"}.ToMessageBody();
                
                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect("smtp.mail.ru", 465, true);
                client.Authenticate("alex_til@mail.ru", pass);
               
                return client.SendAsync(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetBaseException().Message);
                throw;
            }
            
        }
    }
}
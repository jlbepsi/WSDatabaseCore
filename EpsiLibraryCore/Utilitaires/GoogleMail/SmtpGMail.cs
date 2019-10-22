using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.Utilitaires.GoogleMail
{
    public class SmtpGMail : SmtpClient
    {
        private string email;

        public SmtpGMail(string email, string password)
            : base("smtp.gmail.com", 587)
        {
            this.email = email;
            EnableSsl = true;
            Credentials = new NetworkCredential(email, password);
        }

        public void SendMessage(string to, string subject, string body)
        {
            MailMessage mail = new MailMessage(this.email, to, subject, body);
            mail.IsBodyHtml = true;

            Send(mail);
        }
    }
}

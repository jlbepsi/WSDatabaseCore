using System;
using System.Collections.Generic;
using System.Text;

using EpsiLibraryCore.Utilitaires.GoogleMail;

namespace EpsiLibraryCore.Utilitaires.Appenders
{
    public class MailLogAppender : ILogAppender
    {
        private string mail;

        public MailLogAppender(string mail)
        {
            this.mail = mail;
        }

        public void Write(string level, object message)
        {
            switch (level)
            {
                case "ERROR":
                case "FATAL":
                    // Envoie du message à l'utilisateur
                    SmtpGMail client = new SmtpGMail(ConfigurationManager.GetConfigurationManager().GetValue("gmail.user.login"),
                                                 ConfigurationManager.GetConfigurationManager().GetValue("gmail.user.password"));

                    string subject = "Erreur dans l'application ExtranetWebApplication.";
                    StringBuilder body = new StringBuilder();
                    body.AppendFormat("Bonjour, </b><br />Voici l'erreur :<br /> {0}", message);


                    body.Append(@"</b><br /><br />L'administrateur réseau<br />EPSI Montpellier");
                    client.SendMessage(this.mail, subject, body.ToString());
                    break;
            }
        }
    }
}

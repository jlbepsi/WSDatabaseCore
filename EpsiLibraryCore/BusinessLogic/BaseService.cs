﻿using System;

using EpsiLibraryCore.Models;
using EpsiLibraryCore.Utilitaires;
using EpsiLibraryCore.Utilitaires.GoogleMail;

namespace EpsiLibraryCore.BusinessLogic
{
    public abstract class BaseService
    {
        protected ServiceEpsiContext db;

        public ServiceEpsiContext ServiceEpsiContext
        {
            get { return db; }
        }

        public BaseService(ServiceEpsiContext context)
        {
            this.db = context;
        }

        protected void WriteLogs(string message)
        {
            if (ConfigurationManager.GetConfigurationManager().GetValue("logs.useLogs").Equals("true"))
                LogManager.GetLogger().Info(message);
        }

        protected void SendMail(string to, string subject, string body)
        {
            try
            {
                SmtpGMail client = new SmtpGMail(ConfigurationManager.GetConfigurationManager().GetValue("gmail.user.login"),
                                                 ConfigurationManager.GetConfigurationManager().GetValue("gmail.user.password"));
                client.SendMessage(to, subject, body);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }
        }
    }
}

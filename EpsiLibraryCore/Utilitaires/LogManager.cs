using System;
using System.Collections.Generic;
using System.Text;

using EpsiLibraryCore.Utilitaires.Appenders;

namespace EpsiLibraryCore.Utilitaires
{
    public sealed class LogManager
    {
        private static Logger logger = null;

        public static Logger GetLogger()
        {
            if (logger == null)
            {
                logger = new Logger();

                string value = ConfigurationManager.GetConfigurationManager().GetValue("logger.appender.textfile");
                if (! String.IsNullOrEmpty(value))
                    logger.Appenders.Add(new FileLogAppender(value));

                value = ConfigurationManager.GetConfigurationManager().GetValue("logger.appender.mail");
                if (!String.IsNullOrEmpty(value))
                    logger.Appenders.Add(new MailLogAppender(value));
            }

            return logger;
        }
    }
}

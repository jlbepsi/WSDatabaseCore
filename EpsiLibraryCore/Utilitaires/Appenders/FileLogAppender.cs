using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace EpsiLibraryCore.Utilitaires.Appenders
{
    public class FileLogAppender : ILogAppender
    {
        private string filename;

        public FileLogAppender(string filename)
        {
            int index = (filename != null ? filename.IndexOf("%d") : -1);
            if (index != -1)
                this.filename = filename.Substring(0, index) + DateTime.Now.ToString("yyyyMMdd") + filename.Substring(index + 2);
            else
                this.filename = filename;
        }

        public void Write(string level, object message)
        {
            FileStream stream = null;
            StreamWriter writer = null;
            try
            {
                stream = new FileStream(filename, FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(stream);
                // Ecriture du message
                writer.WriteLine(message);
                writer.Flush();
            }
            catch
            {
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (stream != null)
                    stream.Close();
            }
        }
    }
}

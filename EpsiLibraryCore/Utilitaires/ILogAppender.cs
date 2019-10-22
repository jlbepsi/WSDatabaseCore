using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpsiLibraryCore.Utilitaires
{
    public interface ILogAppender
    {
        void Write(string level, object message);
    }
}

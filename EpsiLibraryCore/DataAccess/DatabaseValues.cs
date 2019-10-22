using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.DataAccess
{
    public class DatabaseValues
    {
        public const string DBMOCK_TYPE = "TEST";
        public const string MYSQL_TYPE = "MYSQL";
        public const string SQLSERVER_TYPE = "SQLSERVER";
        public const string ORACLE_TYPE = "ORACLE";


        public const int ADMINISTRATEUR = 1;
        public const int MODIFICATION = 2;
        public const int ECRITURE = 3;
        public const int LECTURE = 4;
    }
}

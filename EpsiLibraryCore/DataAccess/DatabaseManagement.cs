using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.DataAccess
{
    public abstract class DatabaseManagement
    {
        #region Attributes
        // Connection à la BD
        private string connectionString;
        protected DbConnection connection;
        #endregion
        
        #region Builders

        public DatabaseManagement(string connectionString)
        {
            this.connectionString = connectionString;
        }
        #endregion

        #region Connection
        public void Open()
        {
            if (connection != null && connection.State == ConnectionState.Closed)
                connection.Open();
        }
        public void Close()
        {
            if (connection != null && connection.State == ConnectionState.Open)
                connection.Close();
        }
        #endregion

        #region Base de données

        public abstract string GetDatabaseServerType();

        public abstract bool TestUserConnection(string databaseName, string sqlLogin, string password);
        public abstract bool ExistsSqlLoginInDatabase(string sqlLogin, string databaseName);
        public virtual bool IsCorrectName(string databaseName)
        {
            return EpsiLibraryCore.Utilitaires.RegularExpression.IsCorrectFileName(databaseName);
        }
        public abstract bool ExistsDatabase(string databaseName);

        public abstract void CreateDatabase(string databaseName, string sqlLogin);
        public abstract void RemoveDatabase(string databaseName);
        public abstract List<string> ListDatabases(string sqlLogin);

        #endregion

        #region Utilisateurs
        public abstract string MakeSqlLogin(string userLogin);
        public abstract bool ExistsSqlLogin(string sqlLogin);
        public abstract void AddOrUpdateUser(string sqlLogin, string password);
        public abstract void RemoveUser(string sqlLogin);

        public abstract void AddContributor(string databaseName, string sqlLogin, int groupType, string password);
        public abstract void UpdateContributor(string databaseName, string sqlLogin, int groupType, string password);
        public abstract void RemoveContributorFromDatabase(string databaseName, string sqlLogin);

        #endregion


        public static DatabaseManagement CreateDatabaseManagement(string serverCode, string adresseIP, int port)
        {
            serverCode = serverCode.Trim();

            EpsiLibraryCore.Utilitaires.ConfigurationManager config = EpsiLibraryCore.Utilitaires.ConfigurationManager.GetConfigurationManager();
            if (config.GetValue("database.mock") == "1" || serverCode.Equals(DatabaseValues.DBMOCK_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                return new MockDatabaseManagement();
            }

            if (serverCode.Equals(DatabaseValues.MYSQL_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                // On fixe l'adresse IP du serveur
                string connectionString = string.Format(config.GetValue("database.server.mysql.connectionstring"), adresseIP, port);
                return new DatabaseManagementMySQL(connectionString);
            }
            else if (serverCode.Equals(DatabaseValues.SQLSERVER_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                string connectionString = string.Format(config.GetValue("database.server.sqlserver.connectionstring"), adresseIP, port);
                return new DatabaseManagementSQLServer(connectionString);
            }
            else if (serverCode.Equals(DatabaseValues.ORACLE_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                string connectionString = string.Format(config.GetValue("database.server.oracle.connectionstring"), adresseIP, port);
                return new DatabaseManagementOracle(connectionString);
            }

            return null;
        }
    }
}

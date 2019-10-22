using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.DataAccess
{
    public abstract class BaseManagement
    {
        #region Attributes
        // Connection à la BD
        private string connectionString;
        protected DbConnection connection;
        #endregion


        #region Builders
        public BaseManagement(string connectionString)
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

        public abstract bool TestUserConnection(string databaseName, string sqlLogin, string password);
        public abstract bool ExistsSqlLoginInDatabase(string sqlLogin, string databaseName);
        public abstract bool ExistsSqlLogin(string sqlLogin);

        #endregion

        #region Utilisateurs
        protected abstract string MakeSQLLogin(string userLogin);
        /*
        {
            if (userId.Length > 16)
            {
                // Coupure du login
                userId = userId[0] + userId.Substring(userId.IndexOf('.'));
            }
            // Suppression du -
            int charToRemove = userId.IndexOf('-');
            if (charToRemove > 0)
            {
                userId = userId.Substring(0, charToRemove) + userId.Substring(charToRemove + 1);
            }

            if (userId.Length > 15)
            {
                // Coupure "brute" du login
                userId = userId.Substring(0, 15);
            }

            if (userId.Length > 16)
                throw new DatabaseException("Le login ne doit pas excéder 16 caractères.");

            return userId.ToLower();
        }*/
        public abstract string CreateOrUpdateOwnerPwd(string sqlLogin, string password);
        public abstract void RemoveOwnerAccount(string sqlLogin);

        // Vérifie l'existence d'un utilisateur
        public abstract bool ExistsUser(string userLogin);

        public abstract string AddContributor(string name, int groupType, string userLogin, string password, bool isUserEPSI);
        public abstract void UpdateContributor(string name, string userLogin, int groupType, string password);
        public abstract void RemoveContributor(string name, string userLogin);

        #endregion
    }
}
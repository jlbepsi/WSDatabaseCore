using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.DataAccess
{
    public class MockDatabaseManagement : DatabaseManagement
    {
        #region Builders
        public MockDatabaseManagement()
            : base("")
        {
        }
        #endregion


        public override bool TestUserConnection(string databaseName, string sqlLogin, string password)
        {
            return true;
        }

        #region User Management
        // Vérifie l'existence d'un utilisateur
        public override bool ExistsSqlLoginInDatabase(string sqlLogin, string databaseName)
        {
            return false;
        }

        // Vérifie l'existence d'un utilisateur
        public override bool ExistsSqlLogin(string sqlLogin)
        {
            return false;
        }

        public override void AddOrUpdateUser(string sqlLogin, string password)
        {
        }

        public override void RemoveUser(string sqlLogin)
        {
        }
        #endregion

        #region Database Service
        public override bool IsCorrectName(string databaseName)
        {
            return EpsiLibraryCore.Utilitaires.RegularExpression.IsCorrectFileName(databaseName);
        }

        public override bool ExistsDatabase(string databaseName)
        {
            return false;
        }

        public override string GetDatabaseServerType() { return DatabaseValues.MYSQL_TYPE; }


        // Liste tous les base de données de l'utilisateur, format renvoyé
        public override List<string> ListDatabases(string sqlLogin)
        {
            List<string> liste = new List<string>();
            return liste;
        }

        // Créé une base de données si elle n'existe pas
        public override void CreateDatabase(string databaseName, string sqlLogin)
        {
        }

        // Supprime une base de donnée (mais pas l'utilisateur associé)
        public override void RemoveDatabase(string databaseName)
        {
        }

        #endregion

        #region Contributeurs

        // Donne les droits rights à l'utilisateur sqlLogin sur la base de données databaseName
        public override void AddContributor(string databaseName, string sqlLogin, int groupType, string password)
        {

        }
        public override void UpdateContributor(string databaseName, string sqlLogin, int groupType, string password)
        {

        }

        // Supprime les droits rights à l'utilisateur user sur la base de données databaseName
        public override void RemoveContributorFromDatabase(string databaseName, string sqlLogin)
        {
        }

        public override string MakeSqlLogin(string userLogin)
        {
            int indexPoint = userLogin.IndexOf('.');
            int indexTiret = userLogin.IndexOf('-');

            string sqlLogin = userLogin;
            if (indexTiret > 0)
            {
                sqlLogin = userLogin.Substring(0, indexTiret) + userLogin.Substring(indexTiret + 1);
            }

            if (sqlLogin.Length > 16)
            {
                // Coupure du login
                string prenom = userLogin.Substring(0, indexPoint);
                if (indexTiret > 0)
                {
                    prenom = prenom[0].ToString() + prenom[indexTiret + 1].ToString();
                    sqlLogin = prenom;
                }
                else
                {
                    prenom = prenom[0].ToString();
                }

                sqlLogin = prenom + userLogin.Substring(indexPoint);

                if (sqlLogin.Length > 16)
                {
                    // Coupure "brute" du login
                    sqlLogin = sqlLogin.Substring(0, 15);
                }
            }

            return sqlLogin.ToLower();
        }

        #endregion
    }
}

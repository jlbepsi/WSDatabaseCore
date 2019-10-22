using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;

using EpsiLibraryCore.Models;
using EpsiLibraryCore.Utilitaires;

namespace EpsiLibraryCore.DataAccess
{
    public class DatabaseManagementOracle : DatabaseManagement
    {
        #region Builders
        public DatabaseManagementOracle(string connectionString)
            : base(connectionString)
        {
            connection = new OracleConnection(connectionString);
        }
        #endregion


        private OracleConnection GetSqlConnection() { return this.connection as OracleConnection; }

        public override bool TestUserConnection(string databaseName, string sqlLogin, string password)
        {
            string connectionString = string.Format("user id={1}; password={2};DBA Privilege=SYSDBA;data source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = 1521))(CONNECT_DATA = (SID = bdaolap)))",
                                                    this.connection.DataSource, sqlLogin, password);

            bool result = true;
            OracleConnection connectionTest = new OracleConnection(connectionString);
            try
            {
                connectionTest.Open();
            }
            catch (OracleException)
            {
                result = false;
            }
            finally
            {
                connectionTest.Close();
            }

            return result;
        }

        #region User Management
        // Vérifie l'existence d'un utilisateur
        public override bool ExistsSqlLoginInDatabase(string sqlLogin, string databaseName)
        {
            /*
             * Oracle ne crée pas de BD mais seulement un utilisateur pour le tablespace
             * 
             */
            return false;
        }

        // Vérifie l'existence d'un utilisateur
        public override bool ExistsSqlLogin(string sqlLogin)
        {
            try
            {
                Open();
                string storeProcedure = "DatabaseExistsUser";
                OracleCommand cmd = new OracleCommand(storeProcedure, GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new OracleParameter("@userName", sqlLogin));
                OracleParameter exists = cmd.Parameters.Add("@userExists", OracleDbType.Int32);
                exists.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                return (exists.Value.ToString().Equals("1"));
            }
            catch (OracleException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        public override void AddOrUpdateUser(string sqlLogin, string password)
        {
            try
            {
                Open();
                string storeProcedure = "DatabaseAddOrUpdateUser";
                OracleCommand cmd = new OracleCommand(storeProcedure, GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new OracleParameter("@login", sqlLogin));
                cmd.Parameters.Add(new OracleParameter("@password", password));

                cmd.ExecuteNonQuery();
            }
            catch (OracleException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        public override void RemoveUser(string sqlLogin)
        {
            try
            {
                Open();
                string storeProcedure = "DatabaseRemoveUser";
                OracleCommand cmd = new OracleCommand(storeProcedure, GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new OracleParameter("@login", sqlLogin));

                cmd.ExecuteNonQuery();
            }
            catch (OracleException ex)
            {
                LogManager.GetLogger().Error(ex);

                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }
        #endregion

        #region Database Service
        public override bool ExistsDatabase(string databaseName)
        {
            /*
             * Oracle ne crée pas de BD mais seulement un utilisateur pour le tablespace
             * 
             */
            return false;
        }

        public override string GetDatabaseServerType() { return DatabaseValues.ORACLE_TYPE; }


        // Liste tous les base de données de l'utilisateur, format renvoyé
        public override List<string> ListDatabases(string sqlLogin)
        {
            return new List<string>();
        }

        // Créé une base de données si elle n'existe pas
        public override void CreateDatabase(string databaseName, string sqlLogin)
        {
            /*
             * Oracle ne crée pas de BD mais seulement un utilisateur pour le tablespace
             * 
             */
        }

        // Supprime une base de donnée (mais pas l'utilisateur associé)
        public override void RemoveDatabase(string databaseName)
        {
            /*
             * Oracle ne crée pas de BD mais seulement un utilisateur pour le tablespace
             * 
             */
        }

        #endregion

        #region Contributeurs

        // Donne les droits rights à l'utilisateur sqlLogin sur la base de données databaseName
        public override void AddContributor(string databaseName, string sqlLogin, int groupType, string password)
        {
            /*
             * Oracle ne crée pas de contributeurs
             * 
             */
        }
        public override void UpdateContributor(string databaseName, string sqlLogin, int groupType, string password)
        {
            /*
             * Oracle ne crée pas de contributeurs
             * 
             */
        }

        // Supprime les droits rights à l'utilisateur user sur la base de données databaseName
        public override void RemoveContributorFromDatabase(string databaseName, string sqlLogin)
        {
            /*
             * Oracle ne crée pas de contributeurs
             * 
             */
        }

        public override string MakeSqlLogin(string userLogin)
        {
            int indexPoint = userLogin.IndexOf('.');
            int indexTiret = userLogin.IndexOf('-');

            string sqlLogin = userLogin;
            // Suppression du -
            if (indexTiret > 0)
            {
                sqlLogin = userLogin.Substring(0, indexTiret) + userLogin.Substring(indexTiret + 1);
            }
            // Suppression du .
            if (indexPoint > 0)
            {
                sqlLogin = sqlLogin.Substring(0, indexPoint) + sqlLogin.Substring(indexPoint + 1);
            }

            // Ajout du préfixe C##
            // Voir http://www.dba-oracle.com/t_ora_65096_create_user_12c_without_c_prefix.htm
            sqlLogin = "C##" + sqlLogin;
            return sqlLogin.ToUpper();
        }

        #endregion
    }
}

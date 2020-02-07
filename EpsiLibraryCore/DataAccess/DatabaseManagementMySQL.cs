using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

using EpsiLibraryCore.Models;
using EpsiLibraryCore.Utilitaires;

namespace EpsiLibraryCore.DataAccess
{
    public class DatabaseManagementMySQL : DatabaseManagement
    {
        #region Builders
        public DatabaseManagementMySQL(string connectionString)
            : base(connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }
        #endregion


        protected void InternalExecuteNonQuery(string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, GetSqlConnection());
            cmd.ExecuteNonQuery();
        }
        private MySqlConnection GetSqlConnection() { return this.connection as MySqlConnection; }

        public override bool TestUserConnection(string databaseName, string sqlLogin, string password)
        {
            string connectionString = string.Format("server={0};user={1};password={2};database={3};port=3306;",
                                                    this.connection.DataSource, sqlLogin, password, databaseName);

            bool result = true;
            MySqlConnection connectionTest = new MySqlConnection(connectionString);
            try
            {
                connectionTest.Open();
            }
            catch (MySqlException)
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
            try
            {
                Open();
                string storeProcedure = "ExistsUserInDB";
                MySqlCommand cmd = new MySqlCommand(storeProcedure, GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("@dbName", databaseName));
                cmd.Parameters.Add(new MySqlParameter("@userName", sqlLogin));
                MySqlParameter exists = cmd.Parameters.Add("@userExists", MySqlDbType.Int32);
                exists.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                return Convert.ToBoolean(exists.Value);
            }
            catch (MySqlException ex)
            {
                LogManager.GetLogger().Error(ex);

                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        // Vérifie l'existence d'un utilisateur
        public override bool ExistsSqlLogin(string sqlLogin)
        {
            try
            {
                Open();
                string storeProcedure = "ExistsUser";
                MySqlCommand cmd = new MySqlCommand(storeProcedure, GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("@userName", sqlLogin));
                MySqlParameter exists = cmd.Parameters.Add("@userExists", MySqlDbType.Int32);
                exists.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                return Convert.ToBoolean(exists.Value);
            }
            catch (MySqlException ex)
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
                string storeProcedure = "AddOrUpdateUser";
                MySqlCommand cmd = new MySqlCommand(storeProcedure, GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("@userName", sqlLogin));
                cmd.Parameters.Add(new MySqlParameter("@userPassword", password));

                cmd.ExecuteNonQuery();

                InternalExecuteNonQuery(String.Format("GRANT USAGE ON * . * TO '{0}'@'%';", sqlLogin));
            }
            catch (MySqlException ex)
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
            string storeProcedure = "DropUser";
            MySqlCommand cmd = new MySqlCommand(storeProcedure, GetSqlConnection());
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                Open();
                cmd.Parameters.Add(new MySqlParameter("@userName", sqlLogin));
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
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
            try
            {
                Open();
                MySqlCommand cmd = new MySqlCommand("ExistDB", GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("@dbName", databaseName));
                MySqlParameter exists = cmd.Parameters.Add("@dbExists", MySqlDbType.Int32, 11);
                exists.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                return Convert.ToBoolean(exists.Value);
            }
            catch (MySqlException ex)
            {
                LogManager.GetLogger().Error(ex);

                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        public override string GetDatabaseServerType() { return DatabaseValues.MYSQL_TYPE; }


        // Liste tous les base de données de l'utilisateur, format renvoyé
        public override List<string> ListDatabases(string sqlLogin)
        {
            List<string> liste = new List<string>();
            try
            {
                Open();

                string storeProcedure = "ListDatabases";
                MySqlCommand cmd = new MySqlCommand(storeProcedure, GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("@userName", sqlLogin));
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        liste.Add(reader.GetString(0));
                    }
                }

            }
            catch (MySqlException ex)
            {
                LogManager.GetLogger().Error(ex);

                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
            return liste;
        }

        // Créé une base de données si elle n'existe pas
        public override void CreateDatabase(string databaseName, string sqlLogin)
        {
            try
            {
                Open();

                InternalExecuteNonQuery(String.Format("CREATE DATABASE IF NOT EXISTS {0} ;", databaseName));
                InternalExecuteNonQuery(String.Format("GRANT {0} ON {1}.* TO '{2}'@'%' ;", "ALL", databaseName, sqlLogin));
            }
            catch (MySqlException ex)
            {
                LogManager.GetLogger().Error(ex);

                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        // Supprime une base de donnée (mais pas l'utilisateur associé)
        public override void RemoveDatabase(string databaseName)
        {
            try
            {
                Open();

                Open();
                MySqlCommand cmd = new MySqlCommand("DropDB", GetSqlConnection());
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new MySqlParameter("@dbName", databaseName));
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
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

        #region Contributeurs

        // Donne les droits rights à l'utilisateur sqlLogin sur la base de données databaseName

        public override void AddContributor(string databaseName, string sqlLogin, int groupType, string password)
        {
            // Ajout des droits
            string mysqlRights = GetServerRights(groupType);

            try
            {
                Open();
                if (!String.IsNullOrWhiteSpace(password))
                {
                    // Ajout de l'utilisateur sur le serveur
                    MySqlCommand cmd = new MySqlCommand("AddOrUpdateUser", GetSqlConnection());
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("@userName", sqlLogin));
                    cmd.Parameters.Add(new MySqlParameter("@userPassword", password));

                    cmd.ExecuteNonQuery();
                }

                // Ajout de l'utilisateur pour la base de données
                InternalExecuteNonQuery(String.Format("GRANT {0} ON {1}.* TO '{2}'@'%' ;", mysqlRights, databaseName, sqlLogin));
            }
            catch (MySqlException ex)
            {
                LogManager.GetLogger().Error(ex);

                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        public override void UpdateContributor(string databaseName, string sqlLogin, int groupType, string password)
        {
            // L'utilisateur sqlLogin doit exister

            // Ajout des droits
            string mysqlRights = GetServerRights(groupType);

            try
            {
                Open();
                InternalExecuteNonQuery(String.Format("REVOKE ALL PRIVILEGES ON {0}.* FROM '{1}'@'%';", databaseName, sqlLogin));
                InternalExecuteNonQuery(String.Format("GRANT {0} ON {1}.* TO '{2}'@'%' ;", mysqlRights, databaseName, sqlLogin));


                if (!String.IsNullOrWhiteSpace(password))
                {
                    string storeProcedure = "UpdateUserPassword";
                    MySqlCommand cmd = new MySqlCommand(storeProcedure, GetSqlConnection());
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new MySqlParameter("@userName", sqlLogin));
                    cmd.Parameters.Add(new MySqlParameter("@userPassword", password));


                    MySqlParameter userUpdated = cmd.Parameters.Add("@userUpdated", MySqlDbType.Int32);
                    userUpdated.Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                LogManager.GetLogger().Error(ex);

                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        // Supprime les droits rights à l'utilisateur user sur la base de données databaseName
        public override void RemoveContributorFromDatabase(string databaseName, string sqlLogin)
        {
            try
            {
                Open();
                if (string.IsNullOrWhiteSpace(databaseName))
                    databaseName = "*";

                InternalExecuteNonQuery(String.Format("REVOKE ALL ON {0}.* FROM '{1}'@'%' ;", databaseName, sqlLogin));
            }
            catch { }
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

        public string GetServerRights(int groupType)
        {
            string serverRights = "";
            switch (groupType)
            {
                case EpsiLibraryCore.DataAccess.DatabaseValues.ADMINISTRATEUR:
                case EpsiLibraryCore.DataAccess.DatabaseValues.MODIFICATION:
                    serverRights = "SELECT , UPDATE , INSERT , DELETE , CREATE , DROP , INDEX , ALTER , CREATE VIEW , CREATE ROUTINE, TRIGGER, SHOW VIEW , EXECUTE"; break;
                case EpsiLibraryCore.DataAccess.DatabaseValues.ECRITURE: serverRights = "SELECT, UPDATE, INSERT, DELETE, EXECUTE"; break;
                case EpsiLibraryCore.DataAccess.DatabaseValues.LECTURE: serverRights = "SELECT"; break;
            }

            return serverRights;
        }


        #endregion
    }
}

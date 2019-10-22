using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

using EpsiLibraryCore.Models;
using EpsiLibraryCore.Utilitaires;

namespace EpsiLibraryCore.DataAccess
{
    public class DatabaseManagementSQLServer : DatabaseManagement
    {
        #region Builders
        public DatabaseManagementSQLServer(string connectionString)
            : base(connectionString)
        {
            connection = new SqlConnection(connectionString);
        }
        #endregion


        private SqlConnection GetSqlConnection() { return this.connection as SqlConnection; }

        public override bool TestUserConnection(string databaseName, string sqlLogin, string password)
        {
            string connectionString = string.Format("server={0}; uid={1}; pwd={2}; database={3}; Connect Timeout = 10",
                                                    this.connection.DataSource, sqlLogin, password, databaseName);

            bool result = true;
            SqlConnection connectionTest = new SqlConnection(connectionString);
            try
            {
                connectionTest.Open();
            }
            catch (SqlException)
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
                SqlCommand cmd = new SqlCommand("DatabaseExistsUserInDB", GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@dbName", databaseName));
                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                SqlParameter exists = cmd.Parameters.Add("@exists", SqlDbType.Int);
                exists.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                return Convert.ToBoolean(exists.Value);
            }
            catch (SqlException ex)
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
                SqlCommand cmd = new SqlCommand("DatabaseExistsUser", GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                SqlParameter exists = cmd.Parameters.Add("@exists", SqlDbType.Int);
                exists.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                return Convert.ToBoolean(exists.Value);
            }
            catch (SqlException ex)
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
                SqlCommand cmd = new SqlCommand("DatabaseAddOrUpdateUser", GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                cmd.Parameters.Add(new SqlParameter("@password", password));
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
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
                SqlCommand cmd = new SqlCommand(storeProcedure, GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));

                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
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
                SqlCommand cmd = new SqlCommand("DatabaseExistsDB", GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@dbName", databaseName));
                SqlParameter exists = cmd.Parameters.Add("@exists", SqlDbType.Int);
                exists.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                return Convert.ToBoolean(exists.Value);
            }
            catch (SqlException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        public override string GetDatabaseServerType() { return DatabaseValues.SQLSERVER_TYPE; }


        // Liste tous les base de données de l'utilisateur, format renvoyé
        public override List<string> ListDatabases(string sqlLogin)
        {
            List<string> liste = new List<string>();
            try
            {
                Open();

                SqlCommand cmd = new SqlCommand("DatabaseListDatabases", GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    liste.Add(reader.GetString(0));
                }
                reader.Close();
            }
            catch (SqlException ex)
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

                // Création de la table
                SqlCommand cmd = new SqlCommand("DatabaseCreateDatabase", GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@dbName", databaseName));
                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException("Erreur dans le création de la base de données : " + ex.Message);
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

                // Suppression de la table
                string storeProcedure = "DatabaseRemoveDatabase";
                SqlCommand cmd = new SqlCommand(storeProcedure, GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@dbName", databaseName));
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
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
        public override void AddContributor(string databaseName, string sqlLogin, int groupType, string password)
        {
            try
            {
                Open();

                // Ajout de l'utilisateur sur le serveur
                SqlCommand cmd;
                if (!String.IsNullOrWhiteSpace(password))
                {
                    // Ajout de l'utilisateur sur le serveur
                    cmd = new SqlCommand("DatabaseAddOrUpdateUser", GetSqlConnection())
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                    cmd.Parameters.Add(new SqlParameter("@password", password));
                    cmd.ExecuteNonQuery();
                }

                // Les droits pour l'utilisateur
                string sqlserverRights = GetServerRights(groupType);

                // Ajout de l'utilisateur pour la base de données
                cmd = new SqlCommand("DatabaseAddContributor", GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@dbName", databaseName));
                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                cmd.Parameters.Add(new SqlParameter("@userRights", sqlserverRights));
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
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
            try
            {
                Open();

                SqlCommand cmd;

                // Modification des droits
                string sqlserverRights = GetServerRights(groupType);

                cmd = new SqlCommand("DatabaseAddOrUpdateContributorGroupType", GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@dbName", databaseName));
                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                cmd.Parameters.Add(new SqlParameter("@userRights", sqlserverRights));
                cmd.Parameters.Add(new SqlParameter("@doUpdate", 1));
                cmd.ExecuteNonQuery();

                // Modification du mot de passe
                if (!String.IsNullOrWhiteSpace(password))
                {
                    // Ajout de l'utilisateur
                    cmd = new SqlCommand("DatabaseAddOrUpdateUser", GetSqlConnection())
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));
                    cmd.Parameters.Add(new SqlParameter("@password", password));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
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

                string storeProcedure = "DatabaseRemoveContributor";
                SqlCommand cmd = new SqlCommand(storeProcedure, GetSqlConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@dbName", databaseName));
                cmd.Parameters.Add(new SqlParameter("@login", sqlLogin));

                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        public override string MakeSqlLogin(string userLogin)
        {
            return userLogin.ToLower();
        }

        public string GetServerRights(int groupType)
        {
            string serverRights = "";
            switch (groupType)
            {
                case EpsiLibraryCore.DataAccess.DatabaseValues.ADMINISTRATEUR:
                case EpsiLibraryCore.DataAccess.DatabaseValues.MODIFICATION:
                    serverRights = "ALTER, BACKUP DATABASE, CREATE DEFAULT, CREATE FUNCTION, CREATE PROCEDURE, CREATE RULE, CREATE SCHEMA, CREATE SYNONYM, CREATE TABLE, CREATE TYPE, CREATE VIEW, DELETE, EXECUTE, INSERT, SELECT, UPDATE, VIEW DEFINITION"; break;
                case EpsiLibraryCore.DataAccess.DatabaseValues.ECRITURE: serverRights = "SELECT, UPDATE, INSERT, DELETE, EXECUTE"; break;
                case EpsiLibraryCore.DataAccess.DatabaseValues.LECTURE: serverRights = "SELECT"; break;
            }

            return serverRights;
        }

        #endregion
    }

}

using System;
using System.Collections.Generic;
using System.Linq;

using EpsiLibraryCore.DataAccess;
using EpsiLibraryCore.Models;
using EpsiLibraryCore.Utilitaires;
using Microsoft.EntityFrameworkCore;

namespace EpsiLibraryCore.BusinessLogic
{
    public class DatabaseService : BaseService
    {
        public DatabaseService(ServiceEpsiContext contexte)
            : base(contexte)
        {
        }

        #region Gestion des bases de données
        public List<DatabaseDb> GetDatabases()
        {
            IQueryable<DatabaseDb> list = db.DatabaseDb.Include(sn => sn.Server).Include(u => u.Users).OrderBy(db => db.ServerId);
            return list.ToList();
        }

        public List<DatabaseDb> GetDatabasesByServerId(int serverId)
        {
            var list = db.DatabaseDb.Include(sn => sn.Server).Include(u => u.Users).Where(db => db.Server.Id == serverId);
            return list.ToList();
        }

        public List<DatabaseDb> GetDatabasesByServerCode(string serverCode)
        {
            var databases = db.DatabaseDb.Include(sn => sn.Server).Include(u => u.Users).AsEnumerable();
            var list = databases.Where(db => db.Server.Code.Equals(serverCode, StringComparison.InvariantCultureIgnoreCase));
            return list.ToList();
        }

        public List<DatabaseDb> GetDatabasesByLogin(string userLogin)
        {
            var databases = db.DatabaseDb.Include(sn => sn.Server).Include(u => u.Users).AsEnumerable();
            var list = databases.Where(db =>
                db.Users.FirstOrDefault(u => u.UserLogin.Equals(userLogin, StringComparison.InvariantCultureIgnoreCase)) != null);
            
            /*var list = 
                from dbs in db.DatabaseDbs
                join dgu in db.DatabaseGroupUser
                on dbs.Id equals dgu.DbId
                where dgu.UserLogin.Equals(userLogin, StringComparison.InvariantCultureIgnoreCase)
                select dbs;*/
            return list.ToList();
        }

        public DatabaseDb GetDatabase(int id)
        {
            return db.DatabaseDb.Include(sn => sn.Server).Include(u => u.Users)
                .FirstOrDefault(db => db.Id == id);
        }

        public DatabaseDb GetDatabase(int serverId, string nomBD)
        {
            try
            {
                /*DatabaseDb databaseDb = db.DatabaseDb.Include(sn => sn.Server).Include(u => u.Users)
                    .SingleOrDefault(db => db.ServerId == serverId && db.NomBd.Equals(nomBD, StringComparison.InvariantCultureIgnoreCase));*/
                
                var list = db.DatabaseDb.Include(sn => sn.Server).Include(u => u.Users).AsEnumerable();
                DatabaseDb databaseDb = list.SingleOrDefault(db => db.ServerId == serverId && db.NomBd.Equals(nomBD, StringComparison.InvariantCultureIgnoreCase));
                return databaseDb;
            }
            catch(Exception)
            {

            }

            return null;
        }

        public DatabaseDb AddDatabase(DatabaseModel database)
        {
            // Vérification du nom
            if (! RegularExpression.IsCorrectFileName(database.NomBD))
                throw new DatabaseException("Le nom ne doit pas comporter des caractères spéciaux.");

            // Le nom de la base de données doit être unique sur un serveur donné
            if (GetDatabase(database.ServerId, database.NomBD) != null)
                throw new DatabaseException("Le nom de la base de données existe déjà.");

            string serverName = "Serveur non trouvé";
            DatabaseServerUser databaseServerUser = null;
            try
            {
                // Obtention du serveur
                DatabaseServerName databaseServerName = db.DatabaseServerName.Find(database.ServerId);
                if (databaseServerName == null)
                    return null;

                // Obtention du compte utilisateur du serveur
                ServerAccountService serverAccountService = new ServerAccountService(this.ServiceEpsiContext);
                databaseServerUser = serverAccountService.GetAccountByServerLogin(database.ServerId, database.UserLogin);
                if (databaseServerUser == null)
                    return null;

                // Obtention du serveur réel : MySQL, SQL Server, ... avec son adresse IP
                DatabaseManagement management = DatabaseManagement.CreateDatabaseManagement(databaseServerName.Code, databaseServerName.Iplocale, databaseServerName.PortLocal);
                if (management == null)
                    return null;

                // et céation de la base de données sur le serveur de BD
                management.CreateDatabase(database.NomBD, databaseServerUser.SqlLogin);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(string.Format("Erreur dans l'ajout de la base de données {0} sur le serveur '{1}'", database.ToString(), serverName), ex);
            }

            // Ajout de la base de données dans le référentiel
            DatabaseDb databaseDB = new DatabaseDb
            {
                ServerId = database.ServerId,
                NomBd = database.NomBD,
                DateCreation = DateTime.Now,
                Commentaire = database.Commentaire
            };
            db.DatabaseDb.Add(databaseDB);

            try
            {
                db.SaveChanges();

                // puis du créateur comme contributeur avec tous les droits
                DatabaseGroupUser databaseGroupUser = new DatabaseGroupUser
                {
                    DbId = databaseDB.Id,
                    UserLogin = databaseServerUser.UserLogin,
                    UserFullName = database.UserFullName,
                    SqlLogin = databaseServerUser.SqlLogin,
                    GroupType = DatabaseValues.ADMINISTRATEUR,
                    AddedByUserLogin = databaseServerUser.UserLogin
                };
                db.DatabaseGroupUser.Add(databaseGroupUser);
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!Exists(databaseDB.ServerId))
                {
                    return null;
                }
                else
                {
                    LogManager.GetLogger().Error(ex);
                    throw new DatabaseException(string.Format("Erreur dans l'ajout de la base de données dans le référentiel", database.ToString()), ex);
                }
            }

            // Enregistrement dans les logs
            WriteLogs("BDD Création - " + string.Format("L'utilisateur '<b>{0}</b>' a créé la bdd '{1}' de type '{2}'", database.UserLogin, database.NomBD, database.ServerId));
            return databaseDB;
        }

        public bool UpdateDatabase(int id, DatabaseModel database)
        {
            DatabaseDb databaseDB = GetDatabase(database.Id);
            if (databaseDB == null)
                return false;

            // Modification des données
            databaseDB.Commentaire = database.Commentaire;

            db.Entry(databaseDB).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new Exception(string.Format("Erreur dans la modification de la base de données '{0}' dans le référentiel", database.ToString()));
            }

            return true;
        }

        /*public bool RemoveDatabase(DatabaseDb database)
        {
            if (database == null)
            {
                return false;
            }
            // L'objet passé en paramètre est de source inconnue, donc on recharge la base de données
            DatabaseDb databaseDB = GetDatabase(database.Id);
            if (databaseDB == null)
                return false;

            return InternalRemoveDatabase(databaseDB);
        }*/

        public DatabaseDb RemoveDatabase(int id)
        {
            DatabaseDb databaseDB = GetDatabase(id);
            if (databaseDB == null)
                return null;

            if (InternalRemoveDatabase(databaseDB))
                return databaseDB;

            return null;
        }
        #endregion

        /*************************************************************************************************************************************/

        #region Gestion des contrbuteurs

        public List<DatabaseGroupUser> GetDatabaseGroupUsers()
        {
            return db.DatabaseGroupUser.ToList();
        }
        public DatabaseGroupUser GetDatabaseGroupUser(string userLogin, int id)
        {
            try
            {
                var list = db.DatabaseGroupUser.AsEnumerable();
                return list.FirstOrDefault(u => u.DbId == id && u.UserLogin.Equals(userLogin, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception)
            { }

            return null;
        }

        public DatabaseGroupUser GetDatabaseGroupUserWithSqlLogin(string sqlLogin, int id)
        {
            try
            {
                var list = db.DatabaseGroupUser.Include(d => d.Db).AsEnumerable();
                return list.FirstOrDefault(gu => gu.DbId == id && gu.SqlLogin.Equals(sqlLogin, StringComparison.InvariantCultureIgnoreCase));
            }
            catch (Exception)
            { }

            return null;
        }

        public List<DatabaseGroupUser> GetDatabaseGroupUserWithSqlLogin(string sqlLogin)
        {
            var users = db.DatabaseGroupUser.AsEnumerable();
            var list = users.Where(gu => gu.SqlLogin.Equals(sqlLogin, StringComparison.InvariantCultureIgnoreCase));
            return list.ToList();
        }

        public bool IsAdministrateur(string userLogin, int id)
        {
            try
            {
                DatabaseGroupUser databaseGroupUser = GetDatabaseGroupUser(userLogin, id);
                if (databaseGroupUser != null)
                    return databaseGroupUser.GroupType == DatabaseValues.ADMINISTRATEUR;
            }
            catch (Exception)
            { }

            return false;
        }

        public bool IsAdministrateur(string userLogin, List<DatabaseGroupUser> groupUsers)
        {
            foreach (DatabaseGroupUser groupUser in groupUsers)
            {
                if (userLogin.Equals(groupUser.UserLogin, StringComparison.InvariantCultureIgnoreCase) &&
                    groupUser.GroupType == DatabaseValues.ADMINISTRATEUR)
                {
                    return true;
                }
            }

            return false;
        }

        public DatabaseGroupUser AddContributor(string userLogin, GroupUserModel groupUserModel)
        {
            string serverName = null;
            try
            {
                // Obtention de la base de données
                DatabaseDb databaseDB = GetDatabase(groupUserModel.DbId);
                if (databaseDB == null)
                    return null;

                // Obtention du serveur
                DatabaseServerName databaseServerName = db.DatabaseServerName.Find(databaseDB.ServerId);
                if (databaseServerName == null)
                    return null;
                serverName = databaseServerName.Name;

                // Obtention du serveur réel : MySQL, SQL Server, ... avec son adresse IP
                DatabaseManagement management = DatabaseManagement.CreateDatabaseManagement(databaseServerName.Code, databaseServerName.Iplocale, databaseServerName.PortLocal);
                if (management == null)
                    return null;

                management.AddContributor(databaseDB.NomBd, groupUserModel.SqlLogin, groupUserModel.GroupType, groupUserModel.Password);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(string.Format("Erreur dans l'ajout du contributeur {0} sur le serveur '{1}'", groupUserModel.ToString(), serverName), ex);
            }

            DatabaseGroupUser contributor = new DatabaseGroupUser
            {
                DbId = groupUserModel.DbId,
                SqlLogin = groupUserModel.SqlLogin,
                AddedByUserLogin = userLogin,
                GroupType = groupUserModel.GroupType,
                UserLogin = groupUserModel.UserLogin,
                UserFullName = groupUserModel.UserFullName
            };

            try
            {
                // Ajout du contributeur dans le groupe
                this.db.DatabaseGroupUser.Add(contributor);
                this.db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(string.Format("Erreur dans l'ajout du contributeur {0} dans le référentiel", groupUserModel.ToString()), ex);
            }

            // Envoi du mail

            /*
            if (userLoginSQL != null)
                contributor.UserLoginSQL = userLoginSQL;


                if (userEpsi != null)
                {
                    contributor.UserEpsiFullName = userEpsi.Nom + " " + userEpsi.Prenom;

                    // Envoi du mail
                    if (!string.IsNullOrWhiteSpace(userEpsi.Mail))
                    {
                        StringBuilder body = new StringBuilder();
                        body.AppendFormat("Bonjour, </b><br /><br />Vous avez été ajouté comme contributeur dans la base de données '{0}' par l'utilisateur '{1}'", infos.Name, infos.Createur);
                        if (accountAdded)
                        {
                            body.AppendFormat("<br /><br />Un compte a été créé sur le serveur '{0}' avec un mot de passe aléatoire.<br />", infos.Server.Name);
                            body.Append("Vous devez vous connecter à <a href='https://ingenium.montpellier.epsi.fr/'>Ingénium</a> pour modifier le mot de passe de ce compte.");
                        }
                        body.Append("Connectez-vous à <a href='https://ingenium.montpellier.epsi.fr/'>Ingénium</a> pour voir vos base de données.");
                        body.Append(@"</b><br /><br />L'administrateur réseau<br />EPSI Montpellier");

                        SendMail(userEpsi.Mail, "EPSI - Base de données - Ajout de contributeur", body.ToString());
                    }
                }
            }*/

            return contributor;
            /*
            {
                Message = message,
                Status = status,
                UserEpsiFullName = contributor.UserEpsiFullName,
                UserId = userId,
                LoginSQL = contributor.UserLoginSQL,
                GroupType = groupType,
                GroupTypeDescription = DatabaseService.GetGroupDescription(groupType)
            });*/
        }


        public bool UpdateContributor(GroupUserModel groupUserModel)
        {
            DatabaseGroupUser databaseGroupUser = GetDatabaseGroupUserWithSqlLogin(groupUserModel.SqlLogin, groupUserModel.DbId);
            if (databaseGroupUser == null)
                return false;

            // Modification du mot de passe sur le serveur
            string serverName = null;
            try
            {
                // Obtention de la base de données
                DatabaseDb databaseDB = GetDatabase(groupUserModel.DbId);
                if (databaseDB == null)
                    return false;

                // Obtention du serveur
                DatabaseServerName databaseServerName = db.DatabaseServerName.Find(databaseDB.ServerId);
                if (databaseServerName == null)
                    return false;
                serverName = databaseServerName.Name;

                // Obtention du serveur réel : MySQL, SQL Server, ... avec son adresse IP
                DatabaseManagement management = DatabaseManagement.CreateDatabaseManagement(databaseServerName.Code, databaseServerName.Iplocale, databaseServerName.PortLocal);
                if (management == null)
                    return false;

                management.UpdateContributor(databaseDB.NomBd, groupUserModel.SqlLogin, groupUserModel.GroupType, groupUserModel.Password);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(string.Format("Erreur dans l'ajout du contributeur {0} sur le serveur '{1}'", groupUserModel.ToString(), serverName), ex);
            }

            try
            {
                // Maj du type
                databaseGroupUser.GroupType = groupUserModel.GroupType;
                // Modification du contributeur dans le groupe
                db.Entry(databaseGroupUser).State = EntityState.Modified;
                this.db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(string.Format("Erreur dans la modification du contributeur {0} dans le référentiel", groupUserModel.ToString()), ex);
            }

            return true;
        }


        public DatabaseGroupUser RemoveContributor(string sqlLogin, int databaseId)
        {
            DatabaseGroupUser databaseGroupUser = GetDatabaseGroupUserWithSqlLogin(sqlLogin, databaseId);
            if (databaseGroupUser == null)
                return null;

            if (InternalRemoveContributor(databaseGroupUser))
                return databaseGroupUser;

            return null;
        }

        public static string GetGroupDescription(int groupType)
        {
            switch (groupType)
            {
                case DatabaseValues.ADMINISTRATEUR: return "Tous les droits";
                case DatabaseValues.MODIFICATION: return "CRUD enregistrements et tables";
                case DatabaseValues.ECRITURE: return "CRUD enregistrements et tables";
                case DatabaseValues.LECTURE:
                default: return "SELECT uniquement";
            }
        }
        #endregion


        /*************************************************************************************************************************************/


        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
        }

        private bool InternalRemoveDatabase(DatabaseDb database)
        {
            string serverName = "Serveur non trouvé";
            try
            {
                // Obtention du serveur
                DatabaseServerName databaseServerName = db.DatabaseServerName.Find(database.ServerId);
                if (databaseServerName == null)
                    return false;

                // Obtention du serveur réel : MySQL, SQL Server, ... avec son adresse IP
                DatabaseManagement management = DatabaseManagement.CreateDatabaseManagement(databaseServerName.Code, databaseServerName.Iplocale, databaseServerName.PortLocal);
                if (management == null)
                    return false;

                // et suppression de la base de données sur le serveur de BD
                management.RemoveDatabase(database.NomBd);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(string.Format("Erreur dans la suppression de la base de données {0} sur le serveur '{1}'", database.ToString(), serverName), ex);
            }

            // Suppression des contributeurs associés
            db.DatabaseGroupUser.RemoveRange(database.Users);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new Exception(string.Format("Erreur dans la suppression des contributeurs de la base de données '{0}' dans le référentiel", database.ToString()));
            }

            // Suppression de la base de données
            db.DatabaseDb.Remove(database);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new Exception(string.Format("Erreur dans la suppression de la base de données '{0}' dans le référentiel", database.ToString()));
            }

            return true;
        }

        private bool InternalRemoveContributor(DatabaseGroupUser databaseGroupUser)
        {
            string serverName = "Serveur non trouvé";
            try
            {
                // Obtention du serveur
                DatabaseServerName databaseServerName = db.DatabaseServerName.Find(databaseGroupUser.Db.ServerId);
                if (databaseServerName == null)
                    return false;

                // Obtention du serveur réel : MySQL, SQL Server, ... avec son adresse IP
                DatabaseManagement management = DatabaseManagement.CreateDatabaseManagement(databaseServerName.Code, databaseServerName.Iplocale, databaseServerName.PortLocal);
                if (management == null)
                    return false;

                // et suppression de la base de données sur le serveur de BD
                management.RemoveContributorFromDatabase(databaseGroupUser.Db.NomBd, databaseGroupUser.SqlLogin);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new DatabaseException(string.Format("Erreur dans la suppression du contributeur '{0}' de la base de données' {1}' sur le serveur '{2}'", databaseGroupUser.SqlLogin, databaseGroupUser.Db.NomBd, serverName), ex);
            }

            // Suppression du contributeur
            db.DatabaseGroupUser.Remove(databaseGroupUser);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LogManager.GetLogger().Error(ex);
                throw new Exception(string.Format("Erreur dans la suppression du contributeur '{0}' de la base de données '{0}' dans le référentiel", databaseGroupUser.SqlLogin, databaseGroupUser.Db.NomBd));
            }

            return true;
        }

        private bool Exists(int id)
        {
            return db.DatabaseDb.Count(e => e.ServerId == id) > 0;
        }

    }
}

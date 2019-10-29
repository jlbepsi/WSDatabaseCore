using System;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using EpsiLibraryCore.Models;
using EpsiLibraryCore.BusinessLogic;
using EpsiLibraryCore.Utilitaires;
using Microsoft.AspNetCore.Authorization;

namespace WSDatabase.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabasesController : SecureApiController
    {
        private readonly DatabaseService _service = new DatabaseService();

       
        // GET: api/Database
        /// <summary>
        /// Retourne la liste des bases de données <code>DatabaseDb</code>
        /// </summary>
        /// <param name="serverId">L'identifiant du serveur de base de données</param>
        /// <returns>Une liste d'objets <code>DatabaseDb</code></returns>
        /// <example>
        /// http://serveur/api/databasess/3
        /// </example>
        [HttpGet]
        [Authorize]
        public ActionResult<List<DatabaseDb>> GetDatabases()
        {
            List<DatabaseDb> list = _service.GetDatabases();
            FillPermissions(list);
            return list;
        }

        // GET: api/Database/5
        /// <summary>
        /// Retourne la base de données <code>DatabaseDb</code> identifié par <paramref name="id"/>
        /// </summary>
        /// <param name="id">L'identifiant de la base de données</param>
        /// <returns>Un objet <code>DatabaseDb</code></returns>
        /// <example>
        /// http://serveur/api/databases/3
        /// </example>
        [HttpGet("{id}")]
        [Route("{id:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DatabaseDb> GetDatabase(int id)
        {
            DatabaseDb databaseDb = _service.GetDatabase(id);
            if (databaseDb == null)
            {
                return NotFound();
            }

            FillPermissions(databaseDb);
            return Ok(databaseDb);
        }
        
        // GET: api/Database
        /// <summary>
        /// Retourne la liste des bases de données <code>DatabaseDb</code> du serveur identifié par <paramref name="serverId"/>
        /// </summary>
        /// <param name="serverId">L'identifiant du serveur de base de données</param>
        /// <returns>Une liste d'objets <code>DatabaseDb</code></returns>
        /// <example>
        /// http://serveur/api/databases/ServerId/3
        /// </example>
        [HttpGet("{serverId}")]
        [Authorize]
        [Route("serverid/{serverId}")]
        public ActionResult<List<DatabaseDb>> GetDatabasesByServerId(int serverId)
        {
            List<DatabaseDb> list = _service.GetDatabasesByServerId(serverId);
            FillPermissions(list);
            return list;
        }

        // GET: api/Database
        /// <summary>
        /// Retourne la liste des bases de données <code>DatabaseDb</code> du serveur de type <paramref name="serverType"/>
        /// </summary>
        /// <param name="serverCode">L'identifiant du serveur de base de données</param>
        /// <returns>Une liste d'objets <code>DatabaseDb</code></returns>
        /// <example>
        /// http://serveur/api/databases/ServerCode/mysql
        /// </example>
        [HttpGet("{serverCode}")]
        [Authorize]
        [Route("servercode/{serverCode}")]
        public ActionResult<List<DatabaseDb>> GetDatabasesByServerType(string serverCode)
        {
            List<DatabaseDb> list = _service.GetDatabasesByServerCode(serverCode);
            FillPermissions(list);
            return list;
        }

        // GET: api/Database
        /// <summary>
        /// Retourne la liste des bases de données <code>DatabaseDb</code> de l'utilisateur <paramref name="userLogin"/>
        /// </summary>
        /// <param name="userLogin">L'identifiant de l'utilisateur</param>
        /// <returns>Une liste d'objets <code>DatabaseDb</code></returns>
        /// <example>
        /// http://serveur/api/databases/Login/test.v8/
        /// </example>
        [HttpGet("{userLogin}")]
        [Authorize]
        [Route("login/{userLogin}")]
        public ActionResult<List<DatabaseDb>> GetDatabasesByLogin(string userLogin)
        {
            List<DatabaseDb> list = _service.GetDatabasesByLogin(userLogin);
            FillPermissions(list);
            return list;
        }

        // POST: api/Database
        /// <summary>
        /// Ajoute la base de données, les éléments sont identifiés par <paramref name="databaseDb"/>
        /// </summary>
        /// <param name="database">L'objet DatabaseDb a ajouter</param>
        /// <returns>Retourne l'URL de l'objet créé si l'ajout est valide, le code statut HTTP BadRequest ou Conflict sinon</returns>
        /// <example>
        /// http://serveur/api/databases/
        /// L'enveloppe Body contient le JSON de le la base de données a ajouter :
        /// <code>{ "ServerId":0,"NomBD":"DBTest2","UserLogin":"test.v8","UserNom":"V8","UserPrenom":"Test","Commentaire":"Aucun" }</code>
        /// </example>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<DatabaseDb> PostDatabaseDb(DatabaseModel database)
        {
            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrOwner(database.UserLogin))
                return Forbid("Vous n'êtes pas administrateur de la base de données");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DatabaseDb databaseDb = _service.AddDatabase(database);
            if (databaseDb == null)
            {
                return Conflict();
            }

            FillPermissions(databaseDb);
            return CreatedAtRoute("DefaultApi", new { id = databaseDb.Id }, databaseDb);
        }

        // PUT: api/DatabaseDbs/5
        /// <summary>
        /// Modifie les informations (commentaire, ... <paramref name="database"/>) de la base de données identifiée par <paramref name="id"/>
        /// </summary>
        /// <param name="id">L'identifiant de la base de données</param>
        /// <param name="database">L'objet DatabaseModel contenant les données</param>
        /// <returns>Retourne le code statut HTTP NoContent si la modification a été faite, BadRequest sinon</returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PutDatabaseDb(int id, DatabaseModel database)
        {
            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrOwner(database.UserLogin))
                return Forbid("Vous n'êtes pas administrateur de la base de données");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != database.Id)
            {
                return BadRequest();
            }

            return _service.UpdateDatabase(id, database) ?
                StatusCode(StatusCodes.Status204NoContent) : 
                NotFound();
        }

        // DELETE: api/DatabaseDbs/5
        /// <summary>
        /// Supprime la base de données identifiée par <paramref name="id"/>
        /// </summary>
        /// <param name="id">L'identifiant de la base de données</param>
        /// <returns>Retourne le code statut HTTP Ok si la suppression a été faite, Forbidden ou NotFound sinon</returns>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteDatabaseDb(int id)
        {
            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrUser())
                return Forbid("Vous n'êtes pas administrateur de la base de données");

            // L'appelant doit être un administrateur de la base de données
            if (! _service.IsAdministrateur(this.GetJWTIdentity().Name, id))
                return Forbid("Vous n'êtes pas administrateur de la base de données");

            DatabaseDb databaseDb = _service.RemoveDatabase(id);
            if (databaseDb == null)
                return NotFound();

            return Ok(databaseDb);
        }


        /// <summary>
        /// Affecte les permissions pour chaque base de données de la liste
        /// en fonction de l'utilsateur connecté
        /// </summary>
        /// <param name="list"></param>
        /// <param name="jwtAuthenticationIdentity"></param>
        private void FillPermissions(List<DatabaseDb> list)
        {
            JWTAuthenticationIdentity jwtAuthenticationIdentity = GetJWTIdentity();
            foreach (DatabaseDb databaseDb in list)
            {
                FillPermissions(databaseDb, jwtAuthenticationIdentity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseDb"></param>
        /// <param name="jwtAuthenticationIdentity"></param>
        private void FillPermissions(DatabaseDb databaseDb)
        {
            FillPermissions(databaseDb, GetJWTIdentity());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseDb"></param>
        /// <param name="jwtAuthenticationIdentity"></param>
        private void FillPermissions(DatabaseDb databaseDb, JWTAuthenticationIdentity jwtAuthenticationIdentity)
        {
            if (jwtAuthenticationIdentity == null || string.IsNullOrEmpty(jwtAuthenticationIdentity.Name) )
            {
                databaseDb.CanBeDeleted = databaseDb.CanBeUpdated = databaseDb.CanAddGroupUser = false;
                if (databaseDb.Users != null)
                {
                    foreach (DatabaseGroupUser user in databaseDb.Users)
                    {
                        user.CanBeUpdated = user.CanBeDeleted = false;
                    }
                }
            }
            else
            {
                // Si l'utilisateur est administrateur il peut faire toutes les opérations
                int groupType = DatabaseGroupUserPermissions.GetGroupType(databaseDb.Users, jwtAuthenticationIdentity.Name);
                if (groupType == DatabaseGroupUserPermissions.ADMINISTRATEUR)
                {
                    databaseDb.CanBeDeleted = databaseDb.CanBeUpdated = databaseDb.CanAddGroupUser = true;
                    if (databaseDb.Users != null)
                    {
                        foreach (DatabaseGroupUser user in databaseDb.Users)
                        {
                            user.CanBeUpdated = user.CanBeDeleted = true;
                        }
                    }
                }
                else
                {
                    databaseDb.CanBeDeleted = databaseDb.CanBeUpdated = databaseDb.CanAddGroupUser = false;
                    if (databaseDb.Users != null)
                    {
                        foreach (DatabaseGroupUser user in databaseDb.Users)
                        {
                            // Si l'utilisateur connecté est l'utilisateur alors il peut faire les actions
                            if (!String.IsNullOrWhiteSpace(user.UserLogin) &&  user.UserLogin.Equals(jwtAuthenticationIdentity.Name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                user.CanBeUpdated = user.CanBeDeleted = true;
                            }
                            else
                            {
                                user.CanBeUpdated = user.CanBeDeleted = false;
                            }
                        }
                    }
                }
            }
        }

    }
}
using System.Collections.Generic;
using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using EpsiLibraryCore.BusinessLogic;
using EpsiLibraryCore.Models;
using Microsoft.AspNetCore.Authorization;


/*
 * IMPORTANT:
 * Pour les URL comportant un point (dans le login par exemple, 
 * il faut ajouter la ligne suivante dans le fichier web.config:
 *      <modules runAllManagedModulesForAllRequests="true">
 */

namespace WSDatabase.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : SecureApiController
    {
        private readonly ServerAccountService _service;

        public AccountsController(ServiceEpsiContext context)
        {
            _service = new ServerAccountService(context);
        } 

        // GET: api/Accounts
        /// <summary>
        /// Retourne la liste des comptes de tous les serveurs de BD
        /// </summary>
        /// <returns>Une liste d'objets <code>DatabaseServerUser</code></returns>
        /// <example>
        /// http://serveur/api/Accounts
        /// </example>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<DatabaseServerUser>> GetServerAccounts()
        {
            return _service.GetAccounts();
        }

        // GET: api/Accounts/5
        /// <summary>
        /// Retourne la liste des comptes <code>DatabaseServerUser</code> du serveur <paramref name="serverId"/>
        /// </summary>
        /// <param name="serverId">L'identifiant du serveur de base de données</param>
        /// <returns>Une liste d'objets <code>DatabaseServerUser</code></returns>
        /// <example>
        /// http://serveur/api/Accounts/byServerId/3
        /// </example>
        [HttpGet]
        [Route("serverid/{serverId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<DatabaseServerUser>> GetAccountsByServerId(int serverId)
        {
            return _service.GetAccountsByServerId(serverId);
        }

        // GET: api/Accounts/5
        /// <summary>
        /// Retourne la liste des logins des utilisateurs du serveur <paramref name="serverId"/>
        /// </summary>
        /// <param name="serverId">L'identifiant du serveur de base de données</param>
        /// <returns>Une liste de login Utilisateur</returns>
        /// <example>
        /// http://serveur/api/Accounts/byServerId/3
        /// </example>
        [HttpGet]
        [Route("loginserver/{serverId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<string>> GetLoginByServerId(int serverId)
        {
            List<DatabaseServerUser> accounts = _service.GetAccountsByServerId(serverId);
            List<string> logins = new List<string>(accounts.Count);
            foreach (DatabaseServerUser user in accounts)
            {
                logins.Add(user.UserLogin);
            }
            return logins;
        }

        // GET: api/Accounts/5
        /// <summary>
        /// Retourne la liste des comptes <code>DatabaseServerUser</code> identifié par <paramref name="userLogin"/>
        /// </summary>
        /// <param name="userLogin">Le login CetD de l'étudiant</param>
        /// <returns>Une liste d'objets <code>DatabaseServerUser</code></returns>
        /// <example>
        /// http://serveur/api/Accounts/UserLogin/test.v8
        /// </example>
        [HttpGet]
        [Route("userlogin/{userLogin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<ServerAccounUsertModel>> GetAccountsByUserLogin(string userLogin)
        {
            return _service.GetAccountsByUserLogin(userLogin);
        }

        // GET: api/Accounts/5
        /// <summary>
        /// Retourne la liste des comptes <code>DatabaseServerUser</code> identifié par <paramref name="sqlLogin"/>
        /// </summary>
        /// <param name="sqlLogin">Le login SQL de l'utilisateur</param>
        /// <returns>Une liste d'objets <code>DatabaseServerUser</code></returns>
        /// <example>
        /// http://serveur/api/Accounts/SqlLogin/test.v8/
        /// </example>
        [HttpGet]
        [Route("sqllogin/{sqlLogin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<ServerAccounUsertModel>> GetAccountsBySqlLogin(string sqlLogin)
        {
            return _service.GetAccountsBySqlLogin(sqlLogin);
        }

        // GET: api/Accounts/5/test.v5
        /// <summary>
        /// Retourne le compte <code>DatabaseServerUser</code> identifié par <paramref name="userLogin"/> du serveur <paramref name="serverId"/> 
        /// </summary>
        /// <param name="serverId">L'identifiant du serveur de base de données</param>
        /// <param name="userLogin">Le login CD de l'étudiant</param>
        /// <returns>Le compte utilisateur  <code>DatabaseServerUser</code></returns>
        /// <example>
        /// http://serveur/api/Accounts/ServerId/0/UserLogin/test.v8/
        /// </example>
        [HttpGet]
        [Route("serverid/{serverId}/userlogin/{userLogin}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DatabaseServerUser> GetAccounts(int serverId, string userLogin)
        {
            DatabaseServerUser databaseServerUser = _service.GetAccountByServerLogin(serverId, userLogin);
            if (databaseServerUser == null)
            {
                return NotFound();
            }

            return Ok(databaseServerUser);
        }

        // POST: api/DatabaseServerAccount
        /// <summary>
        /// Ajoute l'utilisateur sur le serveur, les éléments sont identifiés par <paramref name="serverAccount"/>
        /// </summary>
        /// <param name="serverAccount">L'objet ServerAccount a ajouter</param>
        /// <returns>Retourne l'URL de l'objet créé si l'ajout est valide, le code statut HTTP BadRequest ou Conflict sinon</returns>
        /// <example>
        /// http://serveur/api/Accounts/
        /// L'enveloppe Body contient le JSON de l'utilisateur a ajouter :
        /// <code>{ "ServerId"="1", "UserLogin"="test.v5", "Password"="123ABC" }</code>
        /// </example>
        
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<DatabaseServerUser> PostAccounts(ServerAccountModel serverAccount)
        {
            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrOwner(serverAccount.UserLogin))
                return Forbid();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DatabaseServerUser databaseServerUser = _service.AddAccount(serverAccount);
            if (databaseServerUser == null)
            {
                return Conflict();
            }

            return Ok(databaseServerUser);
            //return CreatedAtAction(nameof(GetAccountsByServerId ), new { id = databaseServerUser.ServerId }, databaseServerUser);
        }

        // PUT: api/Accounts/5
        /// <summary>
        /// Modifie le mot de passe de l'utilisateur identifié par <paramref name="serverAccount"/> sur le serveur <paramref name="id"/>
        /// </summary>
        /// <param name="id">L'identifiant du serveur de base de données</param>
        /// <param name="serverAccount">L'objet ServerAccount a modifier</param>
        /// <returns>Retourne le code statut HTTP NoContent si la modification est valide, le code statut HTTP BadRequest sinon</returns>
        /// <example>
        /// http://serveur/api/Accounts/3
        /// L'enveloppe Body contient le JSON de l'utilisateur a modifier :
        /// <code>{ "ServerId"="1", "UserLogin"="test.v5", "Password"="123ABC" }</code>
        /// </example>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Put(int id, ServerAccountModel serverAccount)
        {
            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrOwner(serverAccount.UserLogin))
                return Forbid();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != serverAccount.ServerId)
            {
                return BadRequest();
            }

            return _service.UpdateAccount(serverAccount) ?
                StatusCode(StatusCodes.Status204NoContent) :
                NotFound();
        }

        // DELETE: api/DatabaseServerAccount/5
        /// <summary>
        /// Supprime l'utilisateur identifié par <paramref name="serverAccount"/> sur le serveur <paramref name="id"/>
        /// </summary>
        /// <param name="id">L'identifiant du serveur de base de données</param>
        /// <param name="serverAccount">L'objet ServerAccount a supprimer</param>
        /// <returns>Retourne le code statut HTTP Ok et le JSON de l'objet supprimé si la suppression est valide, le code statut HTTP NotFound sinon</returns>
        /// <example>
        /// http://serveur/api/Accounts/3
        /// </example>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteAccounts(int id, ServerAccountModel serverAccount)
        {
            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrOwner(serverAccount.UserLogin))
                return Forbid();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id < 0 || string.IsNullOrWhiteSpace(serverAccount.UserLogin))
            {
                return BadRequest();
            }

            if (id != serverAccount.ServerId)
            {
                return BadRequest();
            }
            DatabaseServerUser databaseServerUser = _service.RemoveAccount(id, serverAccount.UserLogin);
            if (databaseServerUser == null)
            {
                return NotFound();
            }

            return Ok(databaseServerUser);
        }
    }
}
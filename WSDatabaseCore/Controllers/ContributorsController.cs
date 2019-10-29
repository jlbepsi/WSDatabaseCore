using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using EpsiLibraryCore.Models;
using EpsiLibraryCore.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using WSDatabaseCore.Filter;

namespace WSDatabase.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContributorsController : SecureApiController
    {
        private readonly DatabaseService _service = new DatabaseService();

        // GET: api/Contributors/5
        /// <summary>
        /// Retourne les groupes <code>DatabaseGroupUser</code> auquel appartient le contributeur identifié par <paramref name="id"/>
        /// </summary>
        /// <param name="id">L'identifiant du contributeur</param>
        /// <returns>Une lists d'objets <code>DatabaseGroupUser</code></returns>
        /// <example>
        /// http://serveur/api/Contributors/un.contributeur.ajoute
        /// </example>
        [HttpGet("{id}")]
        public ActionResult<List<DatabaseGroupUser>> Get(string id)
        {
            List<DatabaseGroupUser> list = _service.GetDatabaseGroupUserWithSqlLogin(id);
            return list;
        }

        // POST: api/Contributors
        /// <summary>
        /// Ajoute un contributeur pour une base de données, les éléments sont identifiés par <paramref name="groupUserModel"/>
        /// </summary>
        /// <param name="groupUserModel">L'objet contenant les informations du contributeur et de la base de données</param>
        /// <returns>Retourne le code statut HTTP Ok si l'ajout a été fait, BadRequest ou Conflict sinon
        /// </returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<DatabaseGroupUser> Post(GroupUserModel groupUserModel)
        {
            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrUser())
                return Forbid("Vous n'êtes pas autorisé");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // L'appelant doit être un administrateur de la base de données
            if (!_service.IsAdministrateur(GetJWTIdentity().Name, groupUserModel.DbId))
                return Forbid("Vous n'êtes pas administrateur de la base de données");
            
            DatabaseGroupUser databaseGroupUser = _service.AddContributor(GetJWTIdentity().Name, groupUserModel);
            if (databaseGroupUser == null)
            {
                return Conflict();
            }

            // L'utilisateur a tous les droits
            databaseGroupUser.CanBeDeleted = databaseGroupUser.CanBeUpdated = true;
            //return CreatedAtRoute("DefaultApi", new { id = databaseGroupUser. }, databaseGroupUser);
            return Ok(databaseGroupUser);
        }

        // PUT: api/Contributors/5
        /// <summary>
        /// Modifie le mot de passe et/ou letype de groupe
        /// </summary>
        /// <param name="id">L'identifiant du contributeur</param>
        /// <param name="groupUserModel">L'objet contenant les informations du contributeur et de la base de données</param>
        /// <returns>Retourne le code statut HTTP Ok si la modification a été faite, BadRequest ou Conflict sinon
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Put(String id, GroupUserModel groupUserModel)
        {
            if (!ModelState.IsValid || !id.Equals(groupUserModel.SqlLogin, StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest(ModelState);
            }

            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrUser())
                return Forbid("Vous n'êtes pas autorisé");

            // L'appelant doit être un administrateur de la base de données
            if (! _service.IsAdministrateur(GetJWTIdentity().Name, groupUserModel.DbId))
                return Forbid("Vous n'êtes pas administrateur de la base de données");

            if (_service.UpdateContributor(groupUserModel))
                return StatusCode(StatusCodes.Status204NoContent);

            return StatusCode(StatusCodes.Status404NotFound);
        }

        // DELETE: api/Contributors/5
        /// <summary>
        /// Supprime le contributeur "id" de la base de données 
        /// </summary>
        /// <param name="id">L'identifiant du contributeur</param>
        /// <param name="groupUserModel">L'objet contenant les informations du contributeur et de la base de données</param>
        /// <returns>Retourne le code statut HTTP Ok si la modification a été faite, BadRequest ou Conflict sinon
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Delete(String id, GroupUserModel groupUserModel)
        {
            string userLogin = GetJWTIdentity().Name;
            // Vérification de l'appelant
            if (! this.SecurityCheckRoleAdminOrUser())
                return Forbid("Vous n'êtes pas autorisé");

            DatabaseGroupUser databaseGroupUser = _service.GetDatabaseGroupUserWithSqlLogin(id, groupUserModel.DbId);
            if (databaseGroupUser == null)
            {
                return NotFound();
            }

            // L'utilisateur doit être un administrateur de la base de données
            if (! _service.IsAdministrateur(userLogin, groupUserModel.DbId))
                return Forbid("Vous n'êtes pas administrateur de la base de données");

            databaseGroupUser = _service.RemoveContributor(id, groupUserModel.DbId);

            return Ok(databaseGroupUser);
        }
    }

}

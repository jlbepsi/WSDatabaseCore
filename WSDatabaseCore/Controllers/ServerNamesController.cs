using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

using EpsiLibraryCore.BusinessLogic;
using EpsiLibraryCore.Models;

namespace WSDatabase.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerNamesController: ControllerBase
    {
        private ServerNameService _service;

        public ServerNamesController(ServiceEpsiContext context)
        {
            _service = new ServerNameService(context);
        } 
        
        [HttpGet]
        public IEnumerable<DatabaseServerName> GetDatabaseServerNames()
        {
            return _service.Get();
        }

        // GET: api/DatabaseServerNames/5
        [HttpGet("{id}")]
        public ActionResult<DatabaseServerName> GetDatabaseServerName(int id)
        {
            DatabaseServerName databaseServerName = _service.Get(id);
            if (databaseServerName == null)
            {
                return NotFound();
            }

            return Ok(databaseServerName);
        }
    }
}
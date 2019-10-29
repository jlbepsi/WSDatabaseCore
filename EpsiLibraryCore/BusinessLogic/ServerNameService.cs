using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using EpsiLibraryCore.Models;
using EpsiLibraryCore.Utilitaires;

namespace EpsiLibraryCore.BusinessLogic
{
    public class ServerNameService
    {
        private ServiceEpsiContext db;

        public ServerNameService()
        {
            this.db = new ServiceEpsiContext();
        }
        public ServerNameService(ServiceEpsiContext db)
        {
            this.db = db;
        }


        public List<DatabaseServerName> Get()
        {
            return db.DatabaseServerName.ToList();
        }

        public DatabaseServerName Get(int id)
        {
            return db.DatabaseServerName.Find(id);
        }

        public bool Add(DatabaseServerName databaseServerName)
        {

            db.DatabaseServerName.Add(databaseServerName);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!Exists(databaseServerName.Code))
                {
                    return false;
                }
                else
                {
                    LogManager.GetLogger().Error(ex);
                    throw new Exception("Erreur dans l'ajout du serveur");
                }
            }

            return true;
        }

        public bool Update(DatabaseServerName databaseServerName)
        {

            db.Entry(databaseServerName).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!Exists(databaseServerName.Code))
                {
                    return false;
                }
                else
                {
                    LogManager.GetLogger().Error(ex);
                    throw new Exception("Erreur dans la modification du serveur");
                }
            }

            return true;
        }
        public DatabaseServerName Remove(int id)
        {
            DatabaseServerName databaseServerName = db.DatabaseServerName.Find(id);
            if (databaseServerName == null)
            {
                return null;
            }

            db.DatabaseServerName.Remove(databaseServerName);
            db.SaveChanges();

            return databaseServerName;
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
        }

        private bool Exists(string code)
        {
            var databaseServerNames = db.DatabaseServerName.AsEnumerable();
            return databaseServerNames.Count(e => e.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)) > 0;
        }

    }
}

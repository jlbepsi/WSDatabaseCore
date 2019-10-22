using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EpsiLibraryCore.Models
{
    public partial class DatabaseDb
    {
        public DatabaseDb()
        {
            Users = new HashSet<DatabaseGroupUser>();
        }

        public int Id { get; set; }
        public int ServerId { get; set; }
        public string NomBd { get; set; }
        public DateTime? DateCreation { get; set; }
        public string Commentaire { get; set; }

        /*
         * Propriétés ajoutées
         */
        [NotMapped]
        public bool CanBeDeleted { get; set; }
        [NotMapped]
        public bool CanBeUpdated { get; set; }
        [NotMapped]
        public bool CanAddGroupUser { get; set; }
        
        public virtual DatabaseServerName Server { get; set; }
        public virtual ICollection<DatabaseGroupUser> Users { get; set; }
    }
}

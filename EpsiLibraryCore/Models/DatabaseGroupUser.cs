using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EpsiLibraryCore.Models
{
    public partial class DatabaseGroupUser
    {
        public int DbId { get; set; }
        public string SqlLogin { get; set; }
        public string UserLogin { get; set; }
        public string UserFullName { get; set; }
        public int GroupType { get; set; }
        public string AddedByUserLogin { get; set; }

        /*
         * Propriétés ajoutées
         */
        [NotMapped]
        public bool CanBeDeleted { get; set; }
        [NotMapped]
        public bool CanBeUpdated { get; set; }

        [JsonIgnore]
        public virtual DatabaseDb Db { get; set; }
    }
}

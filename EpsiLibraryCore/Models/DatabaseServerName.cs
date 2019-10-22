using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EpsiLibraryCore.Models
{
    public partial class DatabaseServerName
    {
        public DatabaseServerName()
        {
            DatabaseDb = new HashSet<DatabaseDb>();
            DatabaseServerUser = new HashSet<DatabaseServerUser>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Iplocale { get; set; }
        public string NomDns { get; set; }
        public string Description { get; set; }
        public int CanAddDatabase { get; set; }
        public int PortLocal { get; set; }
        public int PortExterne { get; set; }
        public string NomDnslocal { get; set; }

        [JsonIgnore]
        public virtual ICollection<DatabaseDb> DatabaseDb { get; set; }
        [JsonIgnore]
        public virtual ICollection<DatabaseServerUser> DatabaseServerUser { get; set; }
    }
}

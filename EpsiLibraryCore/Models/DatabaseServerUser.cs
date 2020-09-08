using System;
using System.Collections.Generic;

namespace EpsiLibraryCore.Models
{
    public partial class DatabaseServerUser
    {
        public int ServerId { get; set; }
        public string SqlLogin { get; set; }
        public string UserLogin { get; set; }

        public virtual DatabaseServerName Server { get; set; }
    }
}

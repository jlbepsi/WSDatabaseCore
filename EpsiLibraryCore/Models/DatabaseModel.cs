using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.Models
{
    public class DatabaseModel
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public string NomBD { get; set; }
        public string UserLogin { get; set; }
        public string UserFullName { get; set; }
        public string Commentaire { get; set; }

        public override string ToString()
        {
            return string.Format("ServerId={0}, NomBD={1}, UserLogin={2}", ServerId, NomBD, UserLogin);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.Models
{
    public class ServerAccountModel
    {
        public int ServerId { get; set; }
        public string UserLogin { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("ServerId={0}, UserLogin={1}", ServerId, UserLogin);
        }
    }
}

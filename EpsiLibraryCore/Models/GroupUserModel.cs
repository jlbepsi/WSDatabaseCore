using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.Models
{
    public class GroupUserModel
    {
        public int DbId { get; set; }
        public string SqlLogin { get; set; }
        public string UserLogin { get; set; }
        public string UserFullName { get; set; }
        public string Password { get; set; }
        public int GroupType { get; set; }

        public override string ToString()
        {
            return string.Format("DbId={0}, SqlLogin={1}, GroupType={2}", DbId, SqlLogin, GroupType);
        }
    }
}

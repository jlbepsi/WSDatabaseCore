using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace EpsiLibraryCore.Utilitaires
{
    public class JWTAuthenticationIdentity : GenericIdentity
    {

        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Mail { get; set; }
        public string Classe { get; set; }
        public string[] Roles { get; set; }

        public JWTAuthenticationIdentity(string login, string roles)
            : base(login)
        {
            Roles = roles.Split(new char[] { ',' });
        }

        public bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }

        public GenericPrincipal GetPrincipal()
        {
            return new GenericPrincipal(this, Roles);
        }

    }
}

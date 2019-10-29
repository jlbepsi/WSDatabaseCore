using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;

using EpsiLibraryCore.Utilitaires;
using Microsoft.AspNetCore.Http;

namespace WSDatabase.Controllers
{
    public abstract class SecureApiController : ControllerBase
    {
        protected bool SecurityCheckRoleAdminOrUser()
        {
            if (User?.Identity == null)
                return false;

            /* La demande de modification est valide ssi 
             *  - le rôle est ROLE_SUPER_ADMIN
             *  ou 
             *  - le rôle est ROLE_USER
             */
            
            return (this.User.IsInRole("ROLE_SUPER_ADMIN") || this.User.IsInRole("ROLE_USER"));
        }

        protected bool SecurityCheckRoleAdminOrOwner(string userLogin)
        {
            if (User?.Identity == null)
                return false;

            /* La demande de modification est valide ssi 
             *  - le rôle est ROLE_SUPER_ADMIN
             *  ou 
             *  - le rôle est ROLE_USER et l'utilisateur authentifié pat le token est l'utilisateur qui fait la modification
             */
            return (this.User.IsInRole("ROLE_SUPER_ADMIN") || 
                    (this.User.IsInRole("ROLE_USER") && this.User.Identity.Name.Equals(userLogin, System.StringComparison.InvariantCultureIgnoreCase)));
        }

        protected JWTAuthenticationIdentity GetJWTIdentity()
        {
            if (User?.Identity == null)
                return null;

            return AuthenticationModule.PopulateUser(User.Identity as ClaimsIdentity);
        }
    }
}
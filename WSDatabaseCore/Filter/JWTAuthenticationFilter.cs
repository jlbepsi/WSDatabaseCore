using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using EpsiLibraryCore.Utilitaires;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


// https://stackoverflow.com/questions/31464359/how-do-you-create-a-custom-authorizeattribute-in-asp-net-core
// https://docs.microsoft.com/fr-fr/aspnet/core/security/authorization/introduction?view=aspnetcore-3.0


namespace WSDatabaseCore.Filter
{
    public class JwtAuthenticationFilter : AuthorizeAttribute, IAuthorizationFilter
    {
        private string role;

        public JwtAuthenticationFilter()
        {
        }
        public JwtAuthenticationFilter(string role)
        {
            this.role = role;
        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (IsUserAuthorized(context))
            {
                return;
            }
            
            context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
        }
        /*public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
        }*/

        private bool IsUserAuthorized(AuthorizationFilterContext context)
        {
            try
            {
                string token = FetchFromHeader(context); // fetch authorization token from header


                if (token != null && !String.IsNullOrWhiteSpace(token))
                {
                    AuthenticationModule auth = new AuthenticationModule();
                    JwtSecurityToken userPayloadToken = auth.ValidateToken(token);

                    if (userPayloadToken != null)
                    {
                        JWTAuthenticationIdentity identity = AuthenticationModule.PopulateUserIdentity(userPayloadToken);

                        if (this.role == null || identity.Roles.Contains(this.role))
                        {
                            context.HttpContext.User = identity.GetPrincipal();
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex);
            }

            return false;
        }

        private string FetchFromHeader(AuthorizationFilterContext context)
        {
            string requestToken = null;
            var authRequest = context.HttpContext.Request.Headers["Authorization"];
            if (authRequest.Count == 1)
            {
                requestToken = authRequest[0];
            }

            return requestToken;
        }
    }
}
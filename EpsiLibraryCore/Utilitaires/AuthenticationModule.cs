﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;

namespace EpsiLibraryCore.Utilitaires
{
    public class AuthenticationModule
    {
        TokenValidationParameters validationParameters;

        public AuthenticationModule()
        {
            validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetPublicKey()
            };

        }

        public static X509SecurityKey GetPublicKey()
        {
            var path = ConfigurationManager.GetConfigurationManager().GetValue("security.keys.public");

            X509Certificate2 x509Certificate = LoadCertificateFile(path);
            X509SecurityKey publicKey = new X509SecurityKey(x509Certificate);

            LogManager.GetLogger().Info($"AuthenticationModule.GetPublicKey, Loaded");
            return publicKey;
        }

        private static JWTAuthenticationIdentity PopulateUserFromClaims(IEnumerable<Claim> claims)
        {
            // Pour les clés de Claims voir : https://mderriey.com/2019/06/23/where-are-my-jwt-claims/
            string login = claims.First(c => c.Type.Equals("sub")).Value;
            string roles = claims.First(c => c.Type.Equals("roles")).Value;

            return new JWTAuthenticationIdentity(login, roles)
            {
                Nom = claims.First(c => c.Type.Equals("nom")).Value,
                Prenom = claims.First(c => c.Type.Equals("prenom")).Value,
                Mail = claims.First(c => c.Type.Equals("mail")).Value,
                Classe = claims.First(c => c.Type.Equals("classe")).Value
            };
            
            /*string roles = "";
            foreach (Claim claim in claimsPrincipal.Claims)
            {
                roles += claim.Type + "=" + claim.Value + ",";
            }
            return new JWTAuthenticationIdentity("login", roles)
            {
                Nom = "nom",
                Prenom = "prenom",
                Mail = "mail",
                Classe = "classe"
            };*/
        }
        
        public static JWTAuthenticationIdentity PopulateUser(ClaimsIdentity claimsIdentity)
        {
            return PopulateUserFromClaims(claimsIdentity.Claims); 
        }

        /*
        public static JWTAuthenticationIdentity PopulateUserIdentity(JwtSecurityToken userPayloadToken)
        {
            return PopulateUserFromClaims(userPayloadToken.Claims);
        }
        public JwtSecurityToken ValidateToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanValidateToken)
                return null;

            ClaimsPrincipal result = null;
            JwtSecurityToken validatedToken = null;
            try
            {
                SecurityToken securityToken = null;
                result = handler.ValidateToken(accessToken, validationParameters, out securityToken);
                validatedToken = securityToken as JwtSecurityToken;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Debug.WriteLine(exception.Message);
            }

            return validatedToken;
        }
        */
 
        private static X509Certificate2 LoadCertificateFile(string filename)
        {
            X509Certificate2 x509 = null;
            using (FileStream fs = File.OpenRead(filename))
            {
                byte[] rawData = new byte[fs.Length];
                fs.Read(rawData, 0, rawData.Length);
                if (rawData != null)
                {
                    x509 = new X509Certificate2(rawData);
                }
            }
            return x509;
        }

    }
}

using System;
//using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
//using System.Linq;
//using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.HttpsPolicy;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.Swagger;


using EpsiLibraryCore.Utilitaires;

// JWT: https://docs.microsoft.com/fr-fr/aspnet/core/signalr/authn-and-authz?view=aspnetcore-3.0
//      https://blogs.infinitesquare.com/posts/web/mise-en-place-d-une-authentification-par-bearer-token-dans-une-application-aspnet-core

// JWT Correct :
//        https://jasonwatmore.com/post/2019/10/16/aspnet-core-3-role-based-authorization-tutorial-with-example-api

// Mise en place de Swagger
//        https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.0&tabs=visual-studio
//        https://code-maze.com/swagger-ui-asp-net-core-web-api/

namespace WSDatabaseCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            
            services
                .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

            // Réinitialise les clés pour les Claims:
            // https://mderriey.com/2019/06/23/where-are-my-jwt-claims/
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
                {
                    // Identity made Cookie authentication the default.
                    // However, we want JWT Bearer Auth to be the default.
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    // Configure JWT Bearer Auth to expect our security key
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = AuthenticationModule.GetPublicKey()
                        };
                });

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WSDatabaseCore API",
                    Version = "V1",
                    Description = "API de gestion des bases de données en ASP.Net Core Web API 3.0",
                    Contact = new OpenApiContact()
                    {
                        Name = "Jean-Luc Bompard",
                        Email = "jlb.epsi@gmail.com"
                    }
                });

                // Ajout des commentaires XML
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {   
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Fixe le fichier de configuration
            ConfigurationManager.MAIN_CONFIG = System.IO.Path.Combine(env.ContentRootPath, "epsiconfiguration.xml");

            app.UseRouting();
            // Enabling CORS
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint : v1 est le nom défini avec SwaggerDoc (voir ConfigureServices)
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "WSDatabaseCore API V1"); });
        }
    }
}
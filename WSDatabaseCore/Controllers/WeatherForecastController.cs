using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EpsiLibraryCore.Utilitaires;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WSDatabaseCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        //[Authorize]
        //[Authorize(Roles = "SUPER_ADMIN")]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)],
                })
                .ToArray();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SUPER_ADMIN")]
        public WeatherForecast Get(int id)
        {
            JWTAuthenticationIdentity jwtUser = AuthenticationModule.PopulateUser(HttpContext.User.Identity as ClaimsIdentity);
            var rng = new Random();
            return new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(2),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)],
                    Roles = String.Join(",", jwtUser.Roles),
                    Email = jwtUser.Mail,
                    Classe = jwtUser.Classe
                };
        }
    }
}
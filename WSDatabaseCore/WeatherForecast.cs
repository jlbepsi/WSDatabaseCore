using System;

namespace WSDatabaseCore
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);

        public string Summary { get; set; }
        public string Roles { get; set; }
        public string Email { get; set; }
        public string Classe { get; set; }
    }
}
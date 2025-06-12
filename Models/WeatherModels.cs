using System.ComponentModel.DataAnnotations;

namespace GloboClima.WebApp.Models
{
    public class WeatherResponseDto
    {
        public string? CityName { get; set; }
        public string? Country { get; set; }
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public double WindSpeed { get; set; }
        public int Pressure { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class WeatherByCityRequestDto
    {
        [Required(ErrorMessage = "Nome da cidade é obrigatório")]
        public string CityName { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
    }

    public class WeatherByCoordinatesRequestDto
    {
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
    }
}

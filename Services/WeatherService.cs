using GloboClima.WebApp.Models;
using Newtonsoft.Json;
using System.Text;

namespace GloboClima.WebApp.Services
{
    public interface IWeatherService
    {
        Task<WeatherResponseDto?> GetWeatherByCityAsync(string cityName, string? countryCode = null);
        Task<WeatherResponseDto?> GetWeatherByCoordinatesAsync(double latitude, double longitude);
        Task<bool> CityExistsAsync(string cityName, string? countryCode = null);
    }

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherResponseDto?> GetWeatherByCityAsync(string cityName, string? countryCode = null)
        {
            try
            {
                var url = $"prod/api/Weather/city/{Uri.EscapeDataString(cityName)}";
                if (!string.IsNullOrEmpty(countryCode))
                {
                    url += $"?countryCode={Uri.EscapeDataString(countryCode)}";
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WeatherResponseDto>(content);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting weather: {ex.Message}");
            }

            return null;
        }

        public async Task<WeatherResponseDto?> GetWeatherByCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var request = new WeatherByCoordinatesRequestDto
                {
                    Latitude = latitude,
                    Longitude = longitude
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("prod/api/Weather/coordinates", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WeatherResponseDto>(responseContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting weather by coordinates: {ex.Message}");
            }

            return null;
        }

        public async Task<bool> CityExistsAsync(string cityName, string? countryCode = null)
        {
            var weather = await GetWeatherByCityAsync(cityName, countryCode);
            return weather != null;
        }
    }
}

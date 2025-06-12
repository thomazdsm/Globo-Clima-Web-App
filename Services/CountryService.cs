using GloboClima.WebApp.Models;
using Newtonsoft.Json;

namespace GloboClima.WebApp.Services
{
    public interface ICountryService
    {
        Task<List<CountryResponseDto>> GetAllCountriesAsync();
        Task<CountryResponseDto?> GetCountryByNameAsync(string countryName);
        Task<List<CountryResponseDto>> SearchCountriesAsync(string searchTerm);
        Task<CountryResponseDto?> GetCountryByCodeAsync(string countryCode);
    }

    public class CountryService : ICountryService
    {
        private readonly HttpClient _httpClient;

        public CountryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CountryResponseDto>> GetAllCountriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Country");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<CountryResponseDto>>(content) ?? new List<CountryResponseDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting countries: {ex.Message}");
            }

            return new List<CountryResponseDto>();
        }

        public async Task<CountryResponseDto?> GetCountryByNameAsync(string countryName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Country/{Uri.EscapeDataString(countryName)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CountryResponseDto>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting country: {ex.Message}");
            }

            return null;
        }

        public async Task<List<CountryResponseDto>> SearchCountriesAsync(string searchTerm)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Country/search/{Uri.EscapeDataString(searchTerm)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<CountryResponseDto>>(content) ?? new List<CountryResponseDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching countries: {ex.Message}");
            }

            return new List<CountryResponseDto>();
        }

        public async Task<CountryResponseDto?> GetCountryByCodeAsync(string countryCode)
        {
            var countries = await SearchCountriesAsync(countryCode);
            return countries.FirstOrDefault(c => c.Code?.Equals(countryCode, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}

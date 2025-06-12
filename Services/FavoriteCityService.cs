using GloboClima.WebApp.Models;
using Newtonsoft.Json;
using System.Text;

namespace GloboClima.WebApp.Services
{
    public interface IFavoriteCityService
    {
        Task<FavoriteCityResponse?> CreateFavoriteCityAsync(CreateFavoriteCityRequest request);
        Task<List<FavoriteCityResponse>> GetFavoriteCitiesAsync();
        Task<bool> DeleteFavoriteCityAsync(string locationId);
        Task<bool> IsCityFavoriteAsync(string locationId);
        Task<List<FavoriteCityResponse>> GetFavoriteCitiesByCountryAsync(string countryCode);
    }

    public class FavoriteCityService : IFavoriteCityService
    {
        private readonly HttpClient _httpClient;

        public FavoriteCityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<FavoriteCityResponse?> CreateFavoriteCityAsync(CreateFavoriteCityRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/FavoriteCities", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FavoriteCityResponse>(responseContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating favorite city: {ex.Message}");
            }

            return null;
        }

        public async Task<List<FavoriteCityResponse>> GetFavoriteCitiesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/FavoriteCities");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<FavoriteCityResponse>>(content) ?? new List<FavoriteCityResponse>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting favorite cities: {ex.Message}");
            }

            return new List<FavoriteCityResponse>();
        }

        public async Task<bool> DeleteFavoriteCityAsync(string locationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/FavoriteCities/{Uri.EscapeDataString(locationId)}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting favorite city: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsCityFavoriteAsync(string locationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/FavoriteCities/check/{Uri.EscapeDataString(locationId)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<bool>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking favorite city: {ex.Message}");
            }

            return false;
        }

        public async Task<List<FavoriteCityResponse>> GetFavoriteCitiesByCountryAsync(string countryCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/FavoriteCities/country/{Uri.EscapeDataString(countryCode)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<FavoriteCityResponse>>(content) ?? new List<FavoriteCityResponse>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting favorite cities by country: {ex.Message}");
            }

            return new List<FavoriteCityResponse>();
        }
    }
}

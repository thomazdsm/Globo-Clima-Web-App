namespace GloboClima.WebApp.Models
{
    public class DashboardViewModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public int TotalFavoriteCities { get; set; }
        public int TotalCountries { get; set; }
        public List<FavoriteCityViewModel> RecentFavoriteCities { get; set; } = new();
        public double AverageTemperature { get; set; }
        public double HighestTemperature { get; set; }
        public double LowestTemperature { get; set; }
        public string HottestCity { get; set; } = string.Empty;
        public string ColdestCity { get; set; } = string.Empty;
        public string LastAddedCity { get; set; } = string.Empty;
        public DateTime? LastAddedDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class QuickWeatherRequest
    {
        public string CityName { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace GloboClima.WebApp.Models
{
    public class CreateFavoriteCityRequest
    {
        [Required(ErrorMessage = "Código do país é obrigatório")]
        [StringLength(3, MinimumLength = 2, ErrorMessage = "Código do país deve ter 2 ou 3 caracteres")]
        [Display(Name = "Código do País")]
        public string CountryCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome da cidade é obrigatório")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Nome da cidade deve ter entre 1 e 100 caracteres")]
        [Display(Name = "Nome da Cidade")]
        public string CityName { get; set; } = string.Empty;
    }

    public class FavoriteCityResponse
    {
        public string? LocationId { get; set; }
        public string? CountryCode { get; set; }
        public string? CityName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FavoriteCityViewModel
    {
        public FavoriteCityResponse FavoriteCity { get; set; } = new();
        public WeatherResponseDto? Weather { get; set; }
        public CountryResponseDto? Country { get; set; }
    }

    public class FavoriteCitiesPageViewModel
    {
        public CreateFavoriteCityRequest NewFavorite { get; set; } = new();
        public List<FavoriteCityViewModel> FavoriteCities { get; set; } = new();
        public bool IsLoading { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}

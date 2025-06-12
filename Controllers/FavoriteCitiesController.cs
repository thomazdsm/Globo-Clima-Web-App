using GloboClima.WebApp.Models;
using GloboClima.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GloboClima.WebApp.Controllers
{
    [Authorize]
    public class FavoriteCitiesController : Controller
    {
        private readonly IFavoriteCityService _favoriteCityService;
        private readonly IWeatherService _weatherService;
        private readonly ICountryService _countryService;

        public FavoriteCitiesController(
            IFavoriteCityService favoriteCityService,
            IWeatherService weatherService,
            ICountryService countryService)
        {
            _favoriteCityService = favoriteCityService;
            _weatherService = weatherService;
            _countryService = countryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new FavoriteCitiesPageViewModel();

            try
            {
                var favoriteCities = await _favoriteCityService.GetFavoriteCitiesAsync();

                foreach (var favorite in favoriteCities)
                {
                    var cityViewModel = new FavoriteCityViewModel
                    {
                        FavoriteCity = favorite
                    };

                    // Buscar informações do clima (opcional, pode ser carregado sob demanda)
                    if (!string.IsNullOrEmpty(favorite.CityName) && !string.IsNullOrEmpty(favorite.CountryCode))
                    {
                        cityViewModel.Weather = await _weatherService.GetWeatherByCityAsync(favorite.CityName, favorite.CountryCode);
                        cityViewModel.Country = await _countryService.GetCountryByCodeAsync(favorite.CountryCode);
                    }

                    viewModel.FavoriteCities.Add(cityViewModel);
                }
            }
            catch (Exception ex)
            {
                viewModel.ErrorMessage = "Erro ao carregar cidades favoritas: " + ex.Message;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFavorite(CreateFavoriteCityRequest model)
        {
            // Debug: Log dos dados recebidos
            Console.WriteLine($"=== AddFavorite Debug ===");
            Console.WriteLine($"CountryCode: '{model?.CountryCode ?? "NULL"}'");
            Console.WriteLine($"CityName: '{model?.CityName ?? "NULL"}'");
            Console.WriteLine($"Model is null: {model == null}");

            // Debug: Log de todos os dados do Request
            Console.WriteLine("=== Request.Form Debug ===");
            foreach (var item in Request.Form)
            {
                Console.WriteLine($"Key: {item.Key}, Value: {item.Value}");
            }

            // Se o model binding falhou, tentar extrair manualmente
            if (model == null || (string.IsNullOrEmpty(model.CountryCode) && string.IsNullOrEmpty(model.CityName)))
            {
                Console.WriteLine("Model binding failed, trying manual extraction...");

                var countryCode = Request.Form["CountryCode"].FirstOrDefault();
                var cityName = Request.Form["CityName"].FirstOrDefault();

                Console.WriteLine($"Manual extraction - CountryCode: '{countryCode}', CityName: '{cityName}'");

                if (!string.IsNullOrEmpty(countryCode) && !string.IsNullOrEmpty(cityName))
                {
                    model = new CreateFavoriteCityRequest
                    {
                        CountryCode = countryCode,
                        CityName = cityName
                    };

                    // Revalidar o modelo
                    ModelState.Clear();
                    TryValidateModel(model);
                }
            }

            // Debug: Log dos erros de validação
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== Validation Errors ===");
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });

                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {error.Field}, Errors: {string.Join(", ", error.Errors)}");
                }

                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ErrorMessage"] = "Erros de validação: " + string.Join("; ", errorMessages);
                return RedirectToAction("Index");
            }

            try
            {
                // Verificar se a cidade existe
                var cityExists = await _weatherService.CityExistsAsync(model.CityName, model.CountryCode);

                if (!cityExists)
                {
                    TempData["ErrorMessage"] = $"A cidade '{model.CityName}' não foi encontrada no país '{model.CountryCode}'.";
                    return RedirectToAction("Index");
                }

                // Adicionar aos favoritos
                var result = await _favoriteCityService.CreateFavoriteCityAsync(model);

                if (result != null)
                {
                    TempData["SuccessMessage"] = $"Cidade '{model.CityName}, {model.CountryCode}' adicionada aos favoritos com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao adicionar cidade aos favoritos. Talvez ela já esteja na sua lista.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddFavorite: {ex.Message}");
                TempData["ErrorMessage"] = "Erro ao processar solicitação: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Método alternativo para debug usando IFormCollection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFavoriteDebug(IFormCollection form)
        {
            Console.WriteLine("=== AddFavoriteDebug ===");
            foreach (var item in form)
            {
                Console.WriteLine($"Key: {item.Key}, Value: {item.Value}");
            }

            var countryCode = form["CountryCode"].FirstOrDefault();
            var cityName = form["CityName"].FirstOrDefault();

            if (string.IsNullOrEmpty(countryCode) || string.IsNullOrEmpty(cityName))
            {
                TempData["ErrorMessage"] = "Dados não recebidos corretamente.";
                return RedirectToAction("Index");
            }

            var model = new CreateFavoriteCityRequest
            {
                CountryCode = countryCode,
                CityName = cityName
            };

            // Redirecionar para o método principal
            return await AddFavorite(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFavorite(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
            {
                TempData["ErrorMessage"] = "ID da localização inválido.";
                return RedirectToAction("Index");
            }

            try
            {
                var success = await _favoriteCityService.DeleteFavoriteCityAsync(locationId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Cidade removida dos favoritos com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao remover cidade dos favoritos.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao processar solicitação: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
            {
                return NotFound();
            }

            try
            {
                var favoriteCities = await _favoriteCityService.GetFavoriteCitiesAsync();
                var favoriteCity = favoriteCities.FirstOrDefault(f => f.LocationId == locationId);

                if (favoriteCity == null)
                {
                    return NotFound();
                }

                var viewModel = new FavoriteCityViewModel
                {
                    FavoriteCity = favoriteCity
                };

                if (!string.IsNullOrEmpty(favoriteCity.CityName) && !string.IsNullOrEmpty(favoriteCity.CountryCode))
                {
                    viewModel.Weather = await _weatherService.GetWeatherByCityAsync(favoriteCity.CityName, favoriteCity.CountryCode);
                    viewModel.Country = await _countryService.GetCountryByCodeAsync(favoriteCity.CountryCode);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar detalhes: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckCityExists([FromBody] CreateFavoriteCityRequest model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.CityName) || string.IsNullOrEmpty(model.CountryCode))
                {
                    return Json(new { exists = false, message = "Dados inválidos" });
                }

                var exists = await _weatherService.CityExistsAsync(model.CityName, model.CountryCode);

                return Json(new
                {
                    exists = exists,
                    message = exists ? "Cidade encontrada!" : "Cidade não encontrada"
                });
            }
            catch (Exception ex)
            {
                return Json(new { exists = false, message = "Erro ao verificar cidade: " + ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GloboClima.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiTestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiTestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("test-auth")]
        public async Task<IActionResult> TestAuth()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                // O AuthTokenHandler automaticamente adicionará o Bearer token
                var response = await httpClient.GetAsync("https://httpbin.org/headers");
                var content = await response.Content.ReadAsStringAsync();

                return Ok(new
                {
                    success = true,
                    message = "Token enviado com sucesso",
                    headers = content,
                    userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                    hasAccessToken = User.FindFirst("AccessToken") != null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}

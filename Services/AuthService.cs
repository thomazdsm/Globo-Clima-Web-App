using GloboClima.WebApp.Models;
using Newtonsoft.Json;
using System.Text;

namespace GloboClima.WebApp.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request);
        Task<AuthResponse> ResendVerificationAsync(ResendVerificationRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var json = JsonConvert.SerializeObject(new
            {
                email = request.Email,
                password = request.Password,
                firstName = request.FirstName,
                lastName = request.LastName
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/Auth/register", content);

            return await ProcessResponse(response);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var json = JsonConvert.SerializeObject(new
            {
                email = request.Email,
                password = request.Password
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/Auth/login", content);

            return await ProcessResponse(response);
        }

        public async Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var json = JsonConvert.SerializeObject(new
            {
                email = request.Email,
                confirmationCode = request.ConfirmationCode
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/Auth/verify-email", content);

            return await ProcessResponse(response);
        }

        public async Task<AuthResponse> ResendVerificationAsync(ResendVerificationRequest request)
        {
            var json = JsonConvert.SerializeObject(new
            {
                email = request.Email
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/Auth/resend-verification", content);

            return await ProcessResponse(response);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var json = JsonConvert.SerializeObject(new
            {
                refreshToken = request.RefreshToken
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/Auth/refresh", content);

            return await ProcessResponse(response);
        }

        private async Task<AuthResponse> ProcessResponse(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                    response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        // Para endpoints que retornam tokens (login, refresh)
                        if (responseContent.Contains("access_token") || responseContent.Contains("accessToken"))
                        {
                            var tokenResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                            return new AuthResponse
                            {
                                Success = true,
                                Message = "Login realizado com sucesso",
                                AccessToken = tokenResponse?.access_token?.ToString() ?? tokenResponse?.accessToken?.ToString(),
                                RefreshToken = tokenResponse?.refresh_token?.ToString() ?? tokenResponse?.refreshToken?.ToString(),
                                ExpiresAt = tokenResponse?.expires_in != null ?
                                    DateTime.UtcNow.AddSeconds((int)tokenResponse.expires_in) :
                                    DateTime.UtcNow.AddHours(1)
                            };
                        }
                        else
                        {
                            // Para outros endpoints (register, verify-email, etc.)
                            return new AuthResponse
                            {
                                Success = true,
                                Message = "Operacao realizada com sucesso"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        return new AuthResponse
                        {
                            Success = true,
                            Message = "Operacao realizada com sucesso"
                        };
                    }
                }
            }

            // Tratamento de erros
            try
            {
                var error = JsonConvert.DeserializeObject<ApiError>(responseContent);
                return new AuthResponse
                {
                    Success = false,
                    Message = error?.Message ?? GetErrorMessageByStatusCode(response.StatusCode)
                };
            }
            catch
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = GetErrorMessageByStatusCode(response.StatusCode)
                };
            }
        }

        private string GetErrorMessageByStatusCode(System.Net.HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => "Dados inválidos fornecidos",
                System.Net.HttpStatusCode.Unauthorized => "Credenciais inválidas",
                System.Net.HttpStatusCode.Forbidden => "Acesso negado",
                System.Net.HttpStatusCode.NotFound => "Recurso nao encontrado",
                System.Net.HttpStatusCode.InternalServerError => "Erro interno do servidor",
                _ => "Erro na comunicacao com o servidor"
            };
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GloboClima.WebApp.Services
{
    public interface ITokenService
    {
        Task<bool> RefreshTokenIfNeededAsync();
        ClaimsPrincipal? GetPrincipalFromToken(string token);
        bool IsTokenExpired(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> RefreshTokenIfNeededAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return false;

            var accessToken = httpContext.User.FindFirst("AccessToken")?.Value;
            var refreshToken = httpContext.User.FindFirst("RefreshToken")?.Value;

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return false;

            if (!IsTokenExpired(accessToken))
                return true;

            try
            {
                var refreshRequest = new Models.RefreshTokenRequest { RefreshToken = refreshToken };
                var result = await _authService.RefreshTokenAsync(refreshRequest);

                if (result.Success && !string.IsNullOrEmpty(result.AccessToken))
                {
                    // Atualizar os claims do usuário
                    var identity = (ClaimsIdentity)httpContext.User.Identity;

                    // Remove old tokens
                    var oldAccessTokenClaim = identity.FindFirst("AccessToken");
                    var oldRefreshTokenClaim = identity.FindFirst("RefreshToken");

                    if (oldAccessTokenClaim != null)
                        identity.RemoveClaim(oldAccessTokenClaim);
                    if (oldRefreshTokenClaim != null)
                        identity.RemoveClaim(oldRefreshTokenClaim);

                    // Add new tokens
                    identity.AddClaim(new Claim("AccessToken", result.AccessToken));
                    if (!string.IsNullOrEmpty(result.RefreshToken))
                        identity.AddClaim(new Claim("RefreshToken", result.RefreshToken));

                    // Re-sign the user with updated claims
                    await httpContext.SignInAsync(
                        Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));

                    return true;
                }
            }
            catch
            {
                // Token refresh failed, user needs to login again
                await httpContext.SignOutAsync();
                return false;
            }

            return false;
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                var claims = jsonToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims, "jwt");

                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                return jsonToken.ValidTo <= DateTime.UtcNow.AddMinutes(-5); // 5 minute buffer
            }
            catch
            {
                return true;
            }
        }
    }
}

using GloboClima.WebApp.Models;
using GloboClima.WebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GloboClima.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.RegisterAsync(model);

            if (result.Success)
            {
                TempData["Email"] = model.Email;
                TempData["SuccessMessage"] = "Cadastro realizado com sucesso! Verifique seu email para confirmar a conta.";
                return RedirectToAction("VerifyEmail");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.LoginAsync(model);

            if (result.Success)
            {
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    // Login com token JWT
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, model.Email),
                        new Claim("AccessToken", result.AccessToken)
                    };

                    if (!string.IsNullOrEmpty(result.RefreshToken))
                    {
                        claims.Add(new Claim("RefreshToken", result.RefreshToken));
                    }

                    // Extrair claims do JWT se possível
                    try
                    {
                        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jsonToken = tokenHandler.ReadJwtToken(result.AccessToken);

                        // Adicionar claims do JWT
                        foreach (var claim in jsonToken.Claims)
                        {
                            if (claim.Type != "exp" && claim.Type != "iat" && claim.Type != "nbf")
                            {
                                claims.Add(new Claim(claim.Type, claim.Value));
                            }
                        }
                    }
                    catch
                    {
                        // Se não conseguir ler o JWT, continua com os claims básicos
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = result.ExpiresAt ?? DateTimeOffset.UtcNow.AddHours(1)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    // Login sem token (apenas confirmação)
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("VerifyEmail", new { email = model.Email });
                }
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            var email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Register");

            var model = new VerifyEmailRequest { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.VerifyEmailAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Email verificado com sucesso! Você já pode fazer login.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(ResendVerificationRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Email inválido" });

            var result = await _authService.ResendVerificationAsync(model);

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}

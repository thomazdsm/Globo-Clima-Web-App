using System.ComponentModel.DataAnnotations;

namespace GloboClima.WebApp.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sobrenome é obrigatório")]
        [MaxLength(50, ErrorMessage = "Sobrenome deve ter no máximo 50 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmacao de senha é obrigatória")]
        [Compare("Password", ErrorMessage = "Senhas nao coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Código de confirmacao é obrigatório")]
        public string ConfirmationCode { get; set; } = string.Empty;
    }

    public class ResendVerificationRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Token de atualizacao é obrigatório")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class ApiError
    {
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}

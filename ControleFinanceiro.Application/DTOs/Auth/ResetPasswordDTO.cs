using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Application.DTOs.Auth
{
    /// <summary>
    /// DTO para redefinição de senha
    /// </summary>
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Token é obrigatório")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "As senhas não conferem")]
        public string ConfirmPassword { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Application.DTOs.Auth
{
    /// <summary>
    /// DTO para solicitação de recuperação de senha
    /// </summary>
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }
    }
}

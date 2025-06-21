using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Application.DTOs.Auth
{
    /// <summary>
    /// DTO para login de usuário
    /// </summary>
    public class LoginDTO
    {
        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Password { get; set; }
    }
}

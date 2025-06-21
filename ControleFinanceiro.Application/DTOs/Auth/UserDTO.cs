using System;

namespace ControleFinanceiro.Application.DTOs.Auth
{
    /// <summary>
    /// DTO para retorno de dados do usuário
    /// </summary>
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime? DataAlteracao { get; set; }
    }
}

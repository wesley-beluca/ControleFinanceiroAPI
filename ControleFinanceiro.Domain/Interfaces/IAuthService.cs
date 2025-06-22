using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Notifications;
using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Interfaces
{
    /// <summary>
    /// Interface para o serviço de autenticação
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica um usuário e retorna um token JWT
        /// </summary>
        Task<(string token, Usuario usuario)> AuthenticateAsync(string username, string password);
        
        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        /// <returns>O usuário criado ou null em caso de erro</returns>
        Task<Usuario> RegisterAsync(string username, string email, string password);
        
        /// <summary>
        /// Solicita um token para redefinição de senha
        /// </summary>
        /// <returns>Tupla contendo o usuário encontrado e o token gerado, ou null em caso de erro</returns>
        Task<(Usuario usuario, string token)> SolicitarResetSenhaAsync(string email);
        
        /// <summary>
        /// Redefine a senha usando um token
        /// </summary>
        /// <returns>True se a senha foi redefinida com sucesso, False caso contrário</returns>
        Task<bool> ResetSenhaAsync(string token, string novaSenha);
        

    }
}

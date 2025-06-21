using ControleFinanceiro.Domain.Entities;
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
        Task<(bool sucesso, string mensagem, Usuario usuario)> RegisterAsync(string username, string email, string password);
        
        /// <summary>
        /// Solicita um token para redefinição de senha
        /// </summary>
        /// <returns>Tupla contendo: sucesso da operação, mensagem, usuário encontrado e token gerado</returns>
        Task<(bool sucesso, string mensagem, Usuario usuario, string token)> SolicitarResetSenhaAsync(string email);
        
        /// <summary>
        /// Redefine a senha usando um token
        /// </summary>
        Task<(bool sucesso, string mensagem)> ResetSenhaAsync(string token, string novaSenha);
        

    }
}

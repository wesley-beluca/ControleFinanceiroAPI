using ControleFinanceiro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Interfaces
{
    /// <summary>
    /// Interface para o repositório de usuários
    /// </summary>
    public interface IUsuarioRepository
    {
        /// <summary>
        /// Obtém um usuário pelo seu ID
        /// </summary>
        Task<Usuario> ObterPorIdAsync(Guid id);
        
        /// <summary>
        /// Obtém um usuário pelo seu nome de usuário
        /// </summary>
        Task<Usuario> ObterPorUsernameAsync(string username);
        
        /// <summary>
        /// Obtém um usuário pelo seu email
        /// </summary>
        Task<Usuario> ObterPorEmailAsync(string email);
        
        /// <summary>
        /// Obtém um usuário pelo token de reset de senha
        /// </summary>
        Task<Usuario> ObterPorResetTokenAsync(string token);
        
        /// <summary>
        /// Obtém todos os usuários
        /// </summary>
        Task<IEnumerable<Usuario>> ObterTodosAsync();
        
        /// <summary>
        /// Adiciona um novo usuário
        /// </summary>
        Task<Usuario> AdicionarAsync(Usuario usuario);
        
        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        Task<Usuario> AtualizarAsync(Usuario usuario);
        
        /// <summary>
        /// Remove um usuário (soft delete)
        /// </summary>
        Task<bool> RemoverAsync(Guid id);
        
        /// <summary>
        /// Verifica se existe um usuário com o username especificado
        /// </summary>
        Task<bool> ExisteUsernameAsync(string username);
        
        /// <summary>
        /// Verifica se existe um usuário com o email especificado
        /// </summary>
        Task<bool> ExisteEmailAsync(string email);
    }
}

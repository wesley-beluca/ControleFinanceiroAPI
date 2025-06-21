using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Interfaces
{
    /// <summary>
    /// Interface para o serviço de cálculo de saldo
    /// </summary>
    public interface ISaldoService
    {
        /// <summary>
        /// Calcula o saldo atual para um usuário específico
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Saldo atual do usuário</returns>
        Task<decimal> CalcularSaldoUsuarioAsync(Guid usuarioId);
        
        /// <summary>
        /// Calcula o saldo para todos os usuários
        /// </summary>
        /// <returns>Dicionário com o ID do usuário e seu saldo</returns>
        Task<Dictionary<Guid, decimal>> CalcularSaldoTodosUsuariosAsync();
    }
}

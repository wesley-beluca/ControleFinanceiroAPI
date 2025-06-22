using ControleFinanceiro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Interfaces
{
    /// <summary>
    /// Interface para o serviço de transações
    /// </summary>
    public interface ITransacaoService
    {
        /// <summary>
        /// Obtém uma transação pelo ID
        /// </summary>
        Task<TransacaoDTO> GetByIdAsync(Guid id, Guid? usuarioId = null);
        
        /// <summary>
        /// Obtém todas as transações
        /// </summary>
        Task<IEnumerable<TransacaoDTO>> GetAllAsync(Guid? usuarioId = null);
        
        /// <summary>
        /// Obtém transações por período
        /// </summary>
        Task<IEnumerable<TransacaoDTO>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null);
        
        /// <summary>
        /// Obtém transações por tipo
        /// </summary>
        Task<IEnumerable<TransacaoDTO>> GetByTipoAsync(int tipo, Guid? usuarioId = null);
        
        /// <summary>
        /// Adiciona uma nova transação
        /// </summary>
        Task<Guid> AddAsync(CreateTransacaoDTO transacaoDto, Guid? usuarioId = null);
        
        /// <summary>
        /// Atualiza uma transação existente
        /// </summary>
        Task<bool> UpdateAsync(Guid id, UpdateTransacaoDTO transacaoDto, Guid? usuarioId = null);
        
        /// <summary>
        /// Exclui uma transação
        /// </summary>
        Task<bool> DeleteAsync(Guid id, Guid? usuarioId = null);
    }
}
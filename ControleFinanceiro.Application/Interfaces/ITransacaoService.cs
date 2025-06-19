using ControleFinanceiro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Interfaces
{
    public interface ITransacaoService
    {
        Task<TransacaoDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<TransacaoDTO>> GetAllAsync();
        Task<IEnumerable<TransacaoDTO>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<TransacaoDTO>> GetByTipoAsync(string tipo);
        Task<Guid> AddAsync(TransacaoDTO transacaoDto);
        Task UpdateAsync(TransacaoDTO transacaoDto);
        Task DeleteAsync(Guid id);
    }
} 
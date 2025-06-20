using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Interfaces
{
    public interface ITransacaoService
    {
        Task<Result<TransacaoDTO>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<TransacaoDTO>>> GetAllAsync();
        Task<Result<IEnumerable<TransacaoDTO>>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<Result<IEnumerable<TransacaoDTO>>> GetByTipoAsync(int tipo);
        Task<Result<Guid>> AddAsync(TransacaoDTO transacaoDto);
        Task<Result<Guid>> AddAsync(CreateTransacaoDTO transacaoDto);
        Task<Result<bool>> UpdateAsync(TransacaoDTO transacaoDto);
        Task<Result<bool>> DeleteAsync(Guid id);
    }
} 
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Interfaces
{
    public interface ITransacaoService
    {
        Task<Result<TransacaoDTO>> GetByIdAsync(Guid id, Guid? usuarioId = null);
        Task<Result<IEnumerable<TransacaoDTO>>> GetAllAsync(Guid? usuarioId = null);
        Task<Result<IEnumerable<TransacaoDTO>>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null);
        Task<Result<IEnumerable<TransacaoDTO>>> GetByTipoAsync(int tipo, Guid? usuarioId = null);
        Task<Result<Guid>> AddAsync(CreateTransacaoDTO transacaoDto, Guid? usuarioId = null);
        Task<Result<bool>> UpdateAsync(Guid id, UpdateTransacaoDTO transacaoDto, Guid? usuarioId = null);
        Task<Result<bool>> DeleteAsync(Guid id);
    }
}
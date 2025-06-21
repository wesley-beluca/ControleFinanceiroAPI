using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Interfaces
{
    public interface IResumoFinanceiroService
    {
        Task<Result<ResumoFinanceiroDTO>> GerarResumoFinanceiroAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null);
    }
}
using ControleFinanceiro.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Interfaces
{
    public interface IResumoFinanceiroService
    {
        Task<ResumoFinanceiroDTO> GerarResumoFinanceiroAsync(DateTime dataInicio, DateTime dataFim);
    }
} 
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Interfaces
{
    public interface IResumoFinanceiroService
    {
        /// <summary>
        /// Gera um resumo financeiro para o período especificado
        /// </summary>
        /// <param name="dataInicio">Data de início do período</param>
        /// <param name="dataFim">Data de fim do período</param>
        /// <param name="usuarioId">ID do usuário (opcional)</param>
        /// <returns>Resumo financeiro ou null em caso de erro</returns>
        Task<ResumoFinanceiroDTO> GerarResumoFinanceiroAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null);
    }
}
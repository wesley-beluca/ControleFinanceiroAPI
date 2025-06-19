using ControleFinanceiro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Interfaces.Repositories
{
    public interface ITransacaoRepository
    {
        Task<Transacao> GetByIdAsync(Guid id);
        Task<IEnumerable<Transacao>> GetAllAsync();
        Task<IEnumerable<Transacao>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<Transacao>> GetByTipoAsync(TipoTransacao tipo);
        Task<Guid> AddAsync(Transacao transacao);
        Task UpdateAsync(Transacao transacao);
        Task DeleteAsync(Guid id);
    }
} 
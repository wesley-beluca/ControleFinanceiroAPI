using ControleFinanceiro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Interfaces.Repositories
{
    public interface ITransacaoRepository : IBaseRepository<Transacao>
    {
        Task<IEnumerable<Transacao>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<Transacao>> GetByTipoAsync(TipoTransacao tipo);
    }
} 
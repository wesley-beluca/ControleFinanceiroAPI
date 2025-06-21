using ControleFinanceiro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Interfaces.Repositories
{
    public interface ITransacaoRepository : IBaseRepository<Transacao>
    {
        Task<IEnumerable<Transacao>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null);
        Task<IEnumerable<Transacao>> GetByTipoAsync(TipoTransacao tipo, Guid? usuarioId = null);
        Task<IEnumerable<Transacao>> GetAllByUsuarioAsync(Guid usuarioId);
        Task<Transacao> GetByIdAndUsuarioAsync(Guid id, Guid usuarioId);
    }
}
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Repositories
{
    public class TransacaoRepository : BaseRepository<Transacao>, ITransacaoRepository
    {
        public TransacaoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transacao>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _dbSet
                .Where(t => t.Data >= dataInicio && t.Data <= dataFim)
                .OrderBy(t => t.Data)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transacao>> GetByTipoAsync(TipoTransacao tipo)
        {
            return await _dbSet
                .Where(t => t.Tipo == tipo)
                .OrderBy(t => t.Data)
                .ToListAsync();
        }
    }
} 
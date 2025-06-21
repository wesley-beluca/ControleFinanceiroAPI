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

        public async Task<IEnumerable<Transacao>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null)
        {
            var query = _dbSet
                .Where(t => t.Data.Date >= dataInicio.Date && t.Data.Date <= dataFim.Date && !t.Excluido);
                
            if (usuarioId.HasValue)
            {
                query = query.Where(t => t.UsuarioId == usuarioId);
            }
            
            return await query.OrderBy(t => t.Data).ToListAsync();
        }

        public async Task<IEnumerable<Transacao>> GetByTipoAsync(TipoTransacao tipo, Guid? usuarioId = null)
        {
            var query = _dbSet
                .Where(t => t.Tipo == tipo && !t.Excluido);
                
            if (usuarioId.HasValue)
            {
                query = query.Where(t => t.UsuarioId == usuarioId);
            }
            
            return await query.OrderBy(t => t.Data).ToListAsync();
        }
        
        public async Task<IEnumerable<Transacao>> GetAllByUsuarioAsync(Guid usuarioId)
        {
            return await _dbSet
                .Where(t => t.UsuarioId == usuarioId && !t.Excluido)
                .OrderByDescending(t => t.Data)
                .ToListAsync();
        }
        
        public async Task<Transacao> GetByIdAndUsuarioAsync(Guid id, Guid usuarioId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId && !t.Excluido);
        }
        
        public override async Task<IEnumerable<Transacao>> GetAllAsync()
        {
            return await _dbSet
                .Where(t => !t.Excluido)
                .OrderByDescending(t => t.Data)
                .ToListAsync();
        }
    }
}
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
    public class BaseRepository<T> : IBaseRepository<T> where T : Entity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.Excluido);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet
                .Where(e => !e.Excluido)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllNoTrackingAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(e => !e.Excluido)
                .ToListAsync();
        }

        public virtual async Task<Guid> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entity.Id;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            // Atualiza a entidade sem alterar Id e DataInclusao
            _context.Entry(entity).State = EntityState.Modified;
            _context.Entry(entity).Property(x => x.Id).IsModified = false;
            _context.Entry(entity).Property(x => x.DataInclusao).IsModified = false;
            
            await SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                entity.MarcarComoExcluido();
                await UpdateAsync(entity);
            }
        }
        
        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id && !e.Excluido);
        }

        public virtual async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
} 
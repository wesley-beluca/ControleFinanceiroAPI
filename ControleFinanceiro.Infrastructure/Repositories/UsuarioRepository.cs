using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario> ObterPorIdAsync(Guid id)
        {
            return await _context.Set<Usuario>().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario> ObterPorUsernameAsync(string username)
        {
            return await _context.Set<Usuario>().FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<Usuario> ObterPorEmailAsync(string email)
        {
            return await _context.Set<Usuario>().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario> ObterPorResetTokenAsync(string token)
        {
            return await _context.Set<Usuario>().FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
        }

        public async Task<IEnumerable<Usuario>> ObterTodosAsync()
        {
            return await _context.Set<Usuario>().ToListAsync();
        }

        public async Task<Usuario> AdicionarAsync(Usuario usuario)
        {
            await _context.Set<Usuario>().AddAsync(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> AtualizarAsync(Usuario usuario)
        {
            _context.Set<Usuario>().Update(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var usuario = await ObterPorIdAsync(id);
            if (usuario == null)
                return false;

            usuario.MarcarComoExcluido();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteUsernameAsync(string username)
        {
            return await _context.Set<Usuario>().AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _context.Set<Usuario>().AnyAsync(u => u.Email == email);
        }
    }
}

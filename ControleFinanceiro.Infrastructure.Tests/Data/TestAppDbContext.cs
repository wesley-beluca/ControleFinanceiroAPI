using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Data;
using ControleFinanceiro.Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ControleFinanceiro.Infrastructure.Tests.Data
{
    /// <summary>
    /// Versão do AppDbContext específica para testes, que inclui a entidade TestEntity
    /// </summary>
    public class TestAppDbContext : AppDbContext
    {
        public TestAppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração específica para a entidade de teste
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                
                // Filtro global para excluir registros marcados como excluídos
                entity.HasQueryFilter(t => !t.Excluido);
            });
        }
    }
} 
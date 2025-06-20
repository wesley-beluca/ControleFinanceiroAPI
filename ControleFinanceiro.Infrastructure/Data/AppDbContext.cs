using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace ControleFinanceiro.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Transacao> Transacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Descricao).IsRequired().HasMaxLength(Transacao.DESCRICAO_MAX_LENGTH);
                entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.Tipo).IsRequired();
                entity.Property(e => e.DataCriacao).IsRequired();
                entity.Property(e => e.DataAtualizacao).IsRequired(false);
            });

        }
    }
} 
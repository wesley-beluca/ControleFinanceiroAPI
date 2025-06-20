using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            // Aplicando configurações comuns a todas as entidades derivadas de Entity
            var entityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(t => typeof(Entity).IsAssignableFrom(t.ClrType));

            foreach (var entityType in entityTypes)
            {
                var entityTypeBuilder = modelBuilder.Entity(entityType.ClrType);
                
                entityTypeBuilder.Property("Id").ValueGeneratedNever();
                entityTypeBuilder.Property("DataInclusao").IsRequired();
                entityTypeBuilder.Property("DataAlteracao").IsRequired(false);
                entityTypeBuilder.Property("Excluido").IsRequired().HasDefaultValue(false);
            }

            modelBuilder.Entity<Transacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descricao).IsRequired().HasMaxLength(Transacao.DESCRICAO_MAX_LENGTH);
                entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.Tipo).IsRequired();
                
                // Filtro global para excluir registros marcados como excluídos
                entity.HasQueryFilter(t => !t.Excluido);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateSoftDeleteStatus();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateSoftDeleteStatus()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Entity entityEntry)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entityEntry.DefinirDataInclusao(DateTime.Now);
                            entityEntry.DefinirExcluido(false);
                            break;
                        case EntityState.Modified:
                            entityEntry.DefinirDataAlteracao(DateTime.Now);
                            break;
                    }
                }
            }
        }
    }
} 
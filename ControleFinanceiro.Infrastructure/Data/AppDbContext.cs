using ControleFinanceiro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar o nome das tabelas do Identity para evitar conflitos
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

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
                entity.Property(e => e.UsuarioId).IsRequired(false);
                
                // Relacionamento com Usuario (opcional)
                entity.HasOne(t => t.Usuario)
                      .WithMany()
                      .HasForeignKey(t => t.UsuarioId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Filtro global para excluir registros marcados como excluídos
                entity.HasQueryFilter(t => !t.Excluido);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(Usuario.USERNAME_MAX_LENGTH);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(Usuario.EMAIL_MAX_LENGTH);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.ResetPasswordToken).IsRequired(false);
                entity.Property(e => e.ResetPasswordTokenExpiration).IsRequired(false);
                
                // Índices para busca rápida
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.ResetPasswordToken);
                
                // Filtro global para excluir registros marcados como excluídos
                entity.HasQueryFilter(u => !u.Excluido);
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
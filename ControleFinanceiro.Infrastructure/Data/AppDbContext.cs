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
                entity.Property(e => e.Descricao).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Valor).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.Tipo).IsRequired();
            });

            // Dados iniciais para demonstração com GUIDs estáticos
            modelBuilder.Entity<Transacao>().HasData(
                new Transacao { 
                    Id = new Guid("c1c6a98a-5ff2-4d1e-a158-be2861fde84b"), 
                    Tipo = TipoTransacao.Despesa, 
                    Data = new DateTime(2022, 8, 29), 
                    Descricao = "Cartão de Crédito", 
                    Valor = 825.82m 
                },
                new Transacao { 
                    Id = new Guid("c9b5d3c3-6e1f-4f3e-9d5b-f9d5c5d5c5d5"), 
                    Tipo = TipoTransacao.Despesa, 
                    Data = new DateTime(2022, 8, 29), 
                    Descricao = "Curso C#", 
                    Valor = 200.00m 
                },
                new Transacao { 
                    Id = new Guid("d8b5d3c3-7e1f-4f3e-9d5b-f9d5c5d5c5d6"), 
                    Tipo = TipoTransacao.Receita, 
                    Data = new DateTime(2022, 8, 31), 
                    Descricao = "Salário", 
                    Valor = 7000.00m 
                },
                new Transacao { 
                    Id = new Guid("e8b5d3c3-8e1f-4f3e-9d5b-f9d5c5d5c5d7"), 
                    Tipo = TipoTransacao.Despesa, 
                    Data = new DateTime(2022, 9, 1), 
                    Descricao = "Mercado", 
                    Valor = 3000.00m 
                },
                new Transacao { 
                    Id = new Guid("f8b5d3c3-9e1f-4f3e-9d5b-f9d5c5d5c5d8"), 
                    Tipo = TipoTransacao.Despesa, 
                    Data = new DateTime(2022, 9, 1), 
                    Descricao = "Farmácia", 
                    Valor = 300.00m 
                },
                new Transacao { 
                    Id = new Guid("08b5d3c3-ae1f-4f3e-9d5b-f9d5c5d5c5d9"), 
                    Tipo = TipoTransacao.Despesa, 
                    Data = new DateTime(2022, 9, 1), 
                    Descricao = "Combustível", 
                    Valor = 800.25m 
                },
                new Transacao { 
                    Id = new Guid("18b5d3c3-be1f-4f3e-9d5b-f9d5c5d5c5da"), 
                    Tipo = TipoTransacao.Despesa, 
                    Data = new DateTime(2022, 9, 15), 
                    Descricao = "Financiamento Carro", 
                    Valor = 900.00m 
                },
                new Transacao { 
                    Id = new Guid("28b5d3c3-ce1f-4f3e-9d5b-f9d5c5d5c5db"), 
                    Tipo = TipoTransacao.Despesa, 
                    Data = new DateTime(2022, 9, 22), 
                    Descricao = "Financiamento Casa", 
                    Valor = 1200.00m 
                },
                new Transacao { 
                    Id = new Guid("38b5d3c3-de1f-4f3e-9d5b-f9d5c5d5c5dc"), 
                    Tipo = TipoTransacao.Receita, 
                    Data = new DateTime(2022, 9, 25), 
                    Descricao = "Freelance Projeto XPTO", 
                    Valor = 2500.00m 
                }
            );
        }
    }
} 
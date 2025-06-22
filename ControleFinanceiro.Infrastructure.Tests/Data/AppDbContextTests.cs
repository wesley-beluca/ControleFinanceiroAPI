using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ControleFinanceiro.Infrastructure.Tests.Data
{
    public class AppDbContextTests
    {
        [Fact]
        public async Task SaveChangesAsync_DeveAtualizarDataInclusao_AoInserirEntidade()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"InMemoryAppDbContextTest1{Guid.NewGuid()}")
                .Options;

            var dataAntes = DateTime.Now.AddDays(-1);
            
            // Act
            Guid transacaoId;
            using (var context = new AppDbContext(options))
            {
                var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
                await context.Transacoes.AddAsync(transacao);
                await context.SaveChangesAsync();
                transacaoId = transacao.Id;
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var transacao = await context.Transacoes.FindAsync(transacaoId);
                transacao.Should().NotBeNull();
                transacao.DataInclusao.Should().BeAfter(dataAntes);
                transacao.Excluido.Should().BeFalse();
            }
        }

        [Fact]
        public async Task SaveChangesAsync_DeveAtualizarDataAlteracao_AoModificarEntidade()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"InMemoryAppDbContextTest2{Guid.NewGuid()}")
                .Options;

            Guid transacaoId;
            using (var context = new AppDbContext(options))
            {
                var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste Original", 100m);
                await context.Transacoes.AddAsync(transacao);
                await context.SaveChangesAsync();
                transacaoId = transacao.Id;
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var transacao = await context.Transacoes.FindAsync(transacaoId);
                transacao.SetDescricao("Teste Modificado");
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var transacao = await context.Transacoes.FindAsync(transacaoId);
                transacao.Should().NotBeNull();
                transacao.Descricao.Should().Be("Teste Modificado");
                transacao.DataAlteracao.Should().NotBeNull();
                transacao.DataAlteracao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
            }
        }

        [Fact]
        public async Task QueryFilter_DeveIgnorarEntidadesExcluidas_AoConsultarDados()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"InMemoryAppDbContextTest3{Guid.NewGuid()}")
                .Options;

            using (var context = new AppDbContext(options))
            {
                var transacao1 = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste 1", 100m);
                var transacao2 = new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-2), "Teste 2", 200m);
                await context.Transacoes.AddRangeAsync(transacao1, transacao2);
                await context.SaveChangesAsync();
                
                // Marcar uma transação como excluída
                transacao1.MarcarComoExcluido();
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var transacoes = await context.Transacoes.ToListAsync();
                
                // Assert
                transacoes.Should().HaveCount(1);
                transacoes.First().Descricao.Should().Be("Teste 2");
            }
        }
    }
} 
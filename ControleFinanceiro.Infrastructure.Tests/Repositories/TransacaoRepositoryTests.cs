using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Data;
using ControleFinanceiro.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace ControleFinanceiro.Infrastructure.Tests.Repositories
{
    public class TransacaoRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly TransacaoRepository _repository;

        public TransacaoRepositoryTests()
        {
            // Configurar o DbContext para usar banco de dados em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"ControleFinanceiroDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _repository = new TransacaoRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task AddAsync_DeveAdicionarTransacao()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);

            // Act
            var id = await _repository.AddAsync(transacao);

            // Assert
            id.Should().NotBe(Guid.Empty);
            var transacaoSalva = await _context.Transacoes.FindAsync(id);
            transacaoSalva.Should().NotBeNull();
            transacaoSalva.Descricao.Should().Be("Teste");
            transacaoSalva.Valor.Should().Be(100m);
            transacaoSalva.Tipo.Should().Be(TipoTransacao.Receita);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoTransacaoExiste_DeveRetornarTransacao()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            await _context.Transacoes.AddAsync(transacao);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByIdAsync(transacao.Id);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(transacao.Id);
            resultado.Descricao.Should().Be("Teste");
            resultado.Valor.Should().Be(100m);
            resultado.Tipo.Should().Be(TipoTransacao.Receita);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoTransacaoNaoExiste_DeveRetornarNull()
        {
            // Act
            var resultado = await _repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_QuandoTransacaoExcluidaSoftDelete_DeveRetornarNull()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            await _context.Transacoes.AddAsync(transacao);
            await _context.SaveChangesAsync();

            // Marcar como excluído
            transacao.MarcarComoExcluido();
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByIdAsync(transacao.Id);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_DeveRetornarApenasTransacoesNaoExcluidas()
        {
            // Arrange
            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Receita 1", 100m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-2), "Despesa 1", 50m),
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-3), "Receita 2", 200m)
            };

            await _context.Transacoes.AddRangeAsync(transacoes);
            await _context.SaveChangesAsync();

            // Marcar uma transação como excluída
            var transacaoExcluida = transacoes[1];
            transacaoExcluida.MarcarComoExcluido();
            await _context.SaveChangesAsync();

            // Act
            var resultados = await _repository.GetAllAsync();

            // Assert
            resultados.Should().HaveCount(2); // Apenas as duas não excluídas
            resultados.Any(t => t.Id == transacaoExcluida.Id).Should().BeFalse();
        }

        [Fact]
        public async Task GetByPeriodoAsync_DeveRetornarTransacoesDoPeriodo()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now.AddDays(-2); // Usando data no passado para evitar erro de data futura

            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, dataInicio.AddDays(2), "Dentro do período 1", 100m),
                new Transacao(TipoTransacao.Despesa, dataInicio.AddDays(5), "Dentro do período 2", 50m),
                new Transacao(TipoTransacao.Receita, dataInicio.AddDays(-2), "Antes do período", 200m), // Antes
                new Transacao(TipoTransacao.Despesa, dataFim.AddDays(1), "Depois do período", 75m)  // Depois
            };

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"InMemoryTransacaoRepositoryTest2{Guid.NewGuid()}")
                .Options;

            using (var context = new AppDbContext(options))
            {
                foreach (var transacao in transacoes)
                {
                    await context.Transacoes.AddAsync(transacao);
                }
                await context.SaveChangesAsync();
            }

            // Act
            List<Transacao> resultados;
            using (var context = new AppDbContext(options))
            {
                var repository = new TransacaoRepository(context);
                resultados = (await repository.GetByPeriodoAsync(dataInicio, dataFim)).ToList();
            }

            // Assert
            resultados.Should().NotBeNull();
            resultados.Should().HaveCount(2);
            resultados.Should().Contain(t => t.Descricao == "Dentro do período 1");
            resultados.Should().Contain(t => t.Descricao == "Dentro do período 2");
            resultados.Should().NotContain(t => t.Descricao == "Antes do período");
            resultados.Should().NotContain(t => t.Descricao == "Depois do período");
        }

        [Fact]
        public async Task GetByTipoAsync_DeveRetornarTransacoesPorTipo()
        {
            // Arrange
            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Receita 1", 100m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-2), "Despesa 1", 50m),
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-3), "Receita 2", 200m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-4), "Despesa 2", 75m)
            };

            await _context.Transacoes.AddRangeAsync(transacoes);
            await _context.SaveChangesAsync();

            // Act
            var resultadosReceitas = await _repository.GetByTipoAsync(TipoTransacao.Receita);
            var resultadosDespesas = await _repository.GetByTipoAsync(TipoTransacao.Despesa);

            // Assert
            resultadosReceitas.Should().HaveCount(2);
            resultadosReceitas.All(t => t.Tipo == TipoTransacao.Receita).Should().BeTrue();

            resultadosDespesas.Should().HaveCount(2);
            resultadosDespesas.All(t => t.Tipo == TipoTransacao.Despesa).Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_DeveAtualizarTransacao()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Original", 100m);
            await _context.Transacoes.AddAsync(transacao);
            await _context.SaveChangesAsync();

            // Modificar
            transacao.SetDescricao("Atualizado");
            transacao.SetValor(200m);
            transacao.SetTipo(TipoTransacao.Despesa);

            // Act
            await _repository.UpdateAsync(transacao);

            // Assert
            var transacaoAtualizada = await _context.Transacoes.FindAsync(transacao.Id);
            transacaoAtualizada.Should().NotBeNull();
            transacaoAtualizada.Descricao.Should().Be("Atualizado");
            transacaoAtualizada.Valor.Should().Be(200m);
            transacaoAtualizada.Tipo.Should().Be(TipoTransacao.Despesa);
            transacaoAtualizada.DataAlteracao.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteAsync_DeveFazerSoftDelete()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            await _context.Transacoes.AddAsync(transacao);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(transacao.Id);

            // Assert
            var transacaoExcluida = await _context.Transacoes.FindAsync(transacao.Id);
            transacaoExcluida.Should().NotBeNull(); // Ainda existe fisicamente
            transacaoExcluida.Excluido.Should().BeTrue(); // Mas marcada como excluída
            transacaoExcluida.DataAlteracao.Should().NotBeNull();

            // Verificar que GetById não a encontra mais
            var resultado = await _repository.GetByIdAsync(transacao.Id);
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_QuandoTransacaoExiste_DeveRetornarTrue()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            await _context.Transacoes.AddAsync(transacao);
            await _context.SaveChangesAsync();

            // Act
            var existe = await _repository.ExistsAsync(transacao.Id);

            // Assert
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_QuandoTransacaoNaoExiste_DeveRetornarFalse()
        {
            // Act
            var existe = await _repository.ExistsAsync(Guid.NewGuid());

            // Assert
            existe.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_QuandoTransacaoExcluidaSoftDelete_DeveRetornarFalse()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            await _context.Transacoes.AddAsync(transacao);
            await _context.SaveChangesAsync();

            // Marcar como excluído
            transacao.MarcarComoExcluido();
            await _context.SaveChangesAsync();

            // Act
            var existe = await _repository.ExistsAsync(transacao.Id);

            // Assert
            existe.Should().BeFalse();
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace ControleFinanceiro.Application.Tests.Services
{
    public class ResumoFinanceiroServiceTests
    {
        private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
        private readonly ResumoFinanceiroService _service;

        public ResumoFinanceiroServiceTests()
        {
            _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
            _service = new ResumoFinanceiroService(_transacaoRepositoryMock.Object);
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoDataInicioMaiorQueDataFim_DeveRetornarFalha()
        {
            // Arrange
            var dataInicio = DateTime.Now;
            var dataFim = DateTime.Now.AddDays(-1);

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("data inicial não pode ser maior que a data final");
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoPeriodoMuitoLongo_DeveRetornarFalha()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-400);
            var dataFim = DateTime.Now;

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("não pode ser maior que 1 ano");
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoNaoExistemTransacoes_DeveRetornarResumoVazio()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim))
                                   .ReturnsAsync(new List<Transacao>());

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.TotalReceitas.Should().Be(0);
            result.Data.TotalDespesas.Should().Be(0);
            result.Data.SaldoFinal.Should().Be(0);
            result.Data.Periodo.Should().Be($"{dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}");
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoExistemTransacoes_DeveCalcularCorretamente()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            
            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-5), "Salário", 3000m),
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-3), "Freelance", 500m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-8), "Aluguel", 1200m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-2), "Supermercado", 400m)
            };
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim))
                                   .ReturnsAsync(transacoes);

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.TotalReceitas.Should().Be(3500m); // 3000 + 500
            result.Data.TotalDespesas.Should().Be(1600m); // 1200 + 400
            result.Data.SaldoFinal.Should().Be(1900m); // 3500 - 1600
            result.Data.Periodo.Should().Be($"{dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}");
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoDespesasMaioresQueReceitas_DeveTerSaldoNegativo()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            
            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-5), "Salário", 1000m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-8), "Aluguel", 1500m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-2), "Supermercado", 500m)
            };
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim))
                                   .ReturnsAsync(transacoes);

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.TotalReceitas.Should().Be(1000m);
            result.Data.TotalDespesas.Should().Be(2000m); // 1500 + 500
            result.Data.SaldoFinal.Should().Be(-1000m); // 1000 - 2000
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_DeveIgnorarTransacoesExcluidas()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            
            // Criando transações
            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-5), "Salário", 2000m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-4), "Aluguel", 800m)
            };
            
            // Marcando a transação de despesa como excluída
            transacoes[1].MarcarComoExcluido();
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim))
                                   .ReturnsAsync(transacoes);

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.TotalReceitas.Should().Be(2000m);
            result.Data.TotalDespesas.Should().Be(0m); // A despesa foi excluída
            result.Data.SaldoFinal.Should().Be(2000m); // 2000 - 0
        }
    }
} 
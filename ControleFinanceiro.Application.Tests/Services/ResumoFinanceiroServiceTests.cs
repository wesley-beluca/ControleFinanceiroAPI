using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Domain.Constants;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using ControleFinanceiro.Domain.Notifications;
using Moq;
using Xunit;

namespace ControleFinanceiro.Application.Tests.Services
{
    public class ResumoFinanceiroServiceTests
    {
        private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly ResumoFinanceiroService _service;

        public ResumoFinanceiroServiceTests()
        {
            _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            
            // Configuração padrão para o mock do INotificationService
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem>());
            _notificationServiceMock.Setup(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            _notificationServiceMock.Setup(n => n.Clear()).Verifiable();
            
            _service = new ResumoFinanceiroService(
                _transacaoRepositoryMock.Object,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoDataInicioMaiorQueDataFim_DeveAdicionarNotificacao()
        {
            // Arrange
            var dataInicio = DateTime.Now;
            var dataFim = DateTime.Now.AddDays(-1);
            
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            Assert.Null(result);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(
                ChavesNotificacao.DataInicio, 
                MensagensErro.DataInicioMaiorQueFinal), 
                Times.Once);
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoPeriodoMuitoLongo_DeveAdicionarNotificacao()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-400);
            var dataFim = DateTime.Now;
            
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            Assert.Null(result);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(
                ChavesNotificacao.Periodo, 
                MensagensErro.PeriodoInvalido), 
                Times.Once);
        }

        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoNaoExistemTransacoes_DeveRetornarResumoVazio()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
                                   .ReturnsAsync(new List<Transacao>());
                                   
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(DateTime.MinValue, dataInicio.AddDays(-1), It.IsAny<Guid?>()))
                                   .ReturnsAsync(new List<Transacao>());

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalReceitas);
            Assert.Equal(0, result.TotalDespesas);
            Assert.Equal(0, result.SaldoFinal);
            Assert.Equal(dataInicio, result.DataInicio);
            Assert.Equal(dataFim, result.DataFim);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
                                   .ReturnsAsync(transacoes);
                                   
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(DateTime.MinValue, dataInicio.AddDays(-1), It.IsAny<Guid?>()))
                                   .ReturnsAsync(new List<Transacao>());

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3500m, result.TotalReceitas); // 3000 + 500
            Assert.Equal(1600m, result.TotalDespesas); // 1200 + 400
            Assert.Equal(1900m, result.SaldoFinal); // 3500 - 1600
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
                                   .ReturnsAsync(transacoes);
                                   
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(DateTime.MinValue, dataInicio.AddDays(-1), It.IsAny<Guid?>()))
                                   .ReturnsAsync(new List<Transacao>());

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000m, result.TotalReceitas);
            Assert.Equal(2000m, result.TotalDespesas); // 1500 + 500
            Assert.Equal(-1000m, result.SaldoFinal); // 1000 - 2000
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
                                   .ReturnsAsync(transacoes);
                                   
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(DateTime.MinValue, dataInicio.AddDays(-1), It.IsAny<Guid?>()))
                                   .ReturnsAsync(new List<Transacao>());

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2000m, result.TotalReceitas);
            Assert.Equal(0m, result.TotalDespesas); // A despesa foi excluída
            Assert.Equal(2000m, result.SaldoFinal); // 2000 - 0
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async Task GerarResumoFinanceiroAsync_QuandoOcorreExcecao_DeveAdicionarNotificacao()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            
            _transacaoRepositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
                                   .ThrowsAsync(new Exception("Erro simulado"));
                                   
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);

            // Act
            var result = await _service.GerarResumoFinanceiroAsync(dataInicio, dataFim);

            // Assert
            Assert.Null(result);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(
                ChavesNotificacao.Erro, 
                It.Is<string>(s => s.Contains("Erro ao gerar resumo financeiro"))), 
                Times.Once);
        }
    }
}

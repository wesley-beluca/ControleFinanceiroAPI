using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Constants;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using ControleFinanceiro.Domain.Notifications;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace ControleFinanceiro.Application.Tests.Services
{
    public class TransacaoServiceTests
    {
        private readonly Mock<ITransacaoRepository> _repositoryMock;
        private readonly TransacaoDTOValidator _transacaoValidator;
        private readonly CreateTransacaoDTOValidator _createTransacaoValidator;
        private readonly UpdateTransacaoDTOValidator _updateTransacaoValidator;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TransacaoService _service;

        public TransacaoServiceTests()
        {
            _repositoryMock = new Mock<ITransacaoRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            
            // Usando validadores de teste que herdam dos validadores reais
            _transacaoValidator = new TransacaoDTOValidator();
            _createTransacaoValidator = new Mock<CreateTransacaoDTOValidator>().Object;
            _updateTransacaoValidator = new Mock<UpdateTransacaoDTOValidator>().Object;
            _notificationServiceMock = new Mock<INotificationService>();
            _mapperMock = new Mock<IMapper>();
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem>());
            _notificationServiceMock.Setup(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            _notificationServiceMock.Setup(n => n.Clear()).Verifiable();
            
            _service = new TransacaoService(
                _repositoryMock.Object,
                _transacaoValidator,
                _createTransacaoValidator,
                _updateTransacaoValidator,
                _notificationServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_QuandoTransacaoExiste_DeveRetornarTransacaoDTO()
        {
            // Arrange
            var id = Guid.NewGuid();
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            
            // Configurando o Id usando reflection (já que Id é propriedade protegida)
            typeof(Entity).GetProperty("Id").SetValue(transacao, id);

            _repositoryMock.Setup(r => r.GetByIdAsync(id))
                           .ReturnsAsync(transacao);

            // Act
            var result = await _service.GetByIdAsync(id, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal((int)TipoTransacao.Receita, result.Tipo);
            Assert.Equal("Teste", result.Descricao);
            Assert.Equal(100m, result.Valor);
            Assert.False(_notificationServiceMock.Object.HasNotifications);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoTransacaoNaoExiste_DeveAdicionarNotificacao()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _repositoryMock.Setup(r => r.GetByIdAsync(id))
                           .ReturnsAsync((Transacao)null);
                           
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);

            // Act
            var result = await _service.GetByIdAsync(id, null);

            // Assert
            Assert.Null(result);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(
                ChavesNotificacao.Transacao, 
                It.Is<string>(s => s.Contains("não encontrada"))), 
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_QuandoExistemTransacoes_DeveRetornarListaDeTransacoesDTO()
        {
            // Arrange
            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Receita 1", 100m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-2), "Despesa 1", 50m),
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-3), "Receita 2", 200m)
            };
            
            // Configurando os Ids usando reflection
            typeof(Entity).GetProperty("Id").SetValue(transacoes[0], Guid.NewGuid());
            typeof(Entity).GetProperty("Id").SetValue(transacoes[1], Guid.NewGuid());
            typeof(Entity).GetProperty("Id").SetValue(transacoes[2], Guid.NewGuid());

            _repositoryMock.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(transacoes);

            // Act
            var result = await _service.GetAllAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_QuandoDTOInvalido_DeveAdicionarNotificacoes()
        {
            // Arrange
            var dto = new CreateTransacaoDTO
            {
                Tipo = 999, // Tipo inválido
                Data = DateTime.Now,
                Descricao = "", // Descrição inválida
                Valor = -100 // Valor inválido
            };
            
            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Tipo", "Tipo inválido"),
                new ValidationFailure("Descricao", "Descrição é obrigatória"),
                new ValidationFailure("Valor", "Valor deve ser maior que zero")
            };
            
            var mockValidator = Mock.Get(_createTransacaoValidator);
            mockValidator.Setup(v => v.Validate(It.IsAny<ValidationContext<CreateTransacaoDTO>>()))
                .Returns(new ValidationResult(validationErrors));
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.Equal(Guid.Empty, result);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(
                It.IsAny<string>(), 
                It.IsAny<string>()), 
                Times.Exactly(3));
            
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_QuandoDadosValidos_DeveCriarTransacao()
        {
            // Arrange
            var dto = new CreateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now,
                Descricao = "Teste",
                Valor = 100m
            };
            
            var validationResult = new ValidationResult(); // Validação passa
            var transacaoId = Guid.NewGuid();
            
            var mockValidator = Mock.Get(_createTransacaoValidator);
            mockValidator.Setup(v => v.Validate(It.IsAny<ValidationContext<CreateTransacaoDTO>>()))
                .Returns(validationResult);
            
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                          .Callback<Transacao>(t => typeof(Entity).GetProperty("Id").SetValue(t, transacaoId))
                          .ReturnsAsync(transacaoId);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.Equal(transacaoId, result);
            Assert.False(_notificationServiceMock.Object.HasNotifications);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(It.Is<Transacao>(t =>
                t.Tipo == TipoTransacao.Receita &&
                t.Descricao == dto.Descricao &&
                t.Valor == dto.Valor
            )), Times.Once);
        }
        
        [Fact]
        public async Task AddAsync_QuandoDadosValidosComUsuario_DeveCriarTransacaoComUsuario()
        {
            // Arrange
            var dto = new CreateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now,
                Descricao = "Teste",
                Valor = 100m
            };
            
            var usuarioId = Guid.NewGuid();
            var validationResult = new ValidationResult(); // Validação passa
            var transacaoId = Guid.NewGuid();
            
            var mockValidator = Mock.Get(_createTransacaoValidator);
            mockValidator.Setup(v => v.Validate(It.IsAny<ValidationContext<CreateTransacaoDTO>>()))
                .Returns(validationResult);
            
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                          .Callback<Transacao>(t => typeof(Entity).GetProperty("Id").SetValue(t, transacaoId))
                          .ReturnsAsync(transacaoId);

            // Act
            var result = await _service.AddAsync(dto, usuarioId);

            // Assert
            Assert.Equal(transacaoId, result);
            Assert.False(_notificationServiceMock.Object.HasNotifications);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(It.Is<Transacao>(t =>
                t.Tipo == TipoTransacao.Receita &&
                t.Descricao == dto.Descricao &&
                t.Valor == dto.Valor &&
                t.UsuarioId == usuarioId
            )), Times.Once);
        }
        
        [Fact]
        public async Task UpdateAsync_QuandoDadosValidosComUsuario_DeveAtualizarTransacaoComUsuario()
        {
            // Arrange
            var id = Guid.NewGuid();
            var usuarioId = Guid.NewGuid();
            var updateDto = new UpdateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Despesa,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Teste Atualizado",
                Valor = 200m
            };
            
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-2), "Teste Original", 100m);
            
            // Configurando o Id usando reflection (já que Id é propriedade protegida)
            typeof(Entity).GetProperty("Id").SetValue(transacao, id);
            
            // Configurando o validador de teste
            var mockValidator = Mock.Get(_updateTransacaoValidator);
            mockValidator.Setup(v => v.Validate(It.IsAny<ValidationContext<UpdateTransacaoDTO>>()))
                .Returns(new ValidationResult());
            
            _repositoryMock.Setup(r => r.ExistsAsync(id))
                          .ReturnsAsync(true);
                          
            _repositoryMock.Setup(r => r.GetByIdAsync(id))
                          .ReturnsAsync(transacao);
                          
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Transacao>()))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(id, updateDto, usuarioId);

            // Assert
            Assert.True(result);
            Assert.False(_notificationServiceMock.Object.HasNotifications);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Transacao>(t =>
                t.Id == id &&
                t.Tipo == TipoTransacao.Despesa &&
                t.Descricao == "Teste Atualizado" &&
                t.Valor == 200m &&
                t.UsuarioId == usuarioId
            )), Times.Once);
        }
        
        [Fact]
        public async Task DeleteAsync_QuandoTransacaoNaoExiste_DeveAdicionarNotificacao()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _repositoryMock.Setup(r => r.ExistsAsync(id))
                          .ReturnsAsync(false);
                          
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);

            // Act
            var result = await _service.DeleteAsync(id, null);

            // Assert
            Assert.False(result);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _notificationServiceMock.Verify(n => n.AddNotification(
                ChavesNotificacao.Transacao, 
                It.Is<string>(s => s.Contains("não encontrada"))), 
                Times.Once);
            
            _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_QuandoTransacaoExiste_DeveExcluirTransacao()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _repositoryMock.Setup(r => r.ExistsAsync(id))
                          .ReturnsAsync(true);
            
            _repositoryMock.Setup(r => r.DeleteAsync(id))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(id, null);

            // Assert
            Assert.True(result);
            Assert.False(_notificationServiceMock.Object.HasNotifications);
            
            _notificationServiceMock.Verify(n => n.Clear(), Times.Once);
            _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }
    }
}

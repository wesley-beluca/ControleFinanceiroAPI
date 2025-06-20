using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Application.Tests.TestHelpers;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace ControleFinanceiro.Application.Tests.Services
{
    public class TransacaoServiceTests
    {
        private readonly Mock<ITransacaoRepository> _repositoryMock;
        private readonly TestTransacaoDTOValidator _transacaoValidator;
        private readonly TestCreateTransacaoDTOValidator _createTransacaoValidator;
        private readonly TestUpdateTransacaoDTOValidator _updateTransacaoValidator;
        private readonly TransacaoService _service;

        public TransacaoServiceTests()
        {
            _repositoryMock = new Mock<ITransacaoRepository>();
            
            // Usando validadores de teste que herdam dos validadores reais
            _transacaoValidator = new TestTransacaoDTOValidator();
            _createTransacaoValidator = new TestCreateTransacaoDTOValidator();
            _updateTransacaoValidator = new TestUpdateTransacaoDTOValidator();
            
            _service = new TransacaoService(
                _repositoryMock.Object,
                _transacaoValidator,
                _createTransacaoValidator,
                _updateTransacaoValidator
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
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(id);
            result.Data.Tipo.Should().Be((int)TipoTransacao.Receita);
            result.Data.Descricao.Should().Be("Teste");
            result.Data.Valor.Should().Be(100m);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoTransacaoNaoExiste_DeveRetornarFalha()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _repositoryMock.Setup(r => r.GetByIdAsync(id))
                           .ReturnsAsync((Transacao)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("não encontrada");
            result.Data.Should().BeNull();
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
            
            _repositoryMock.Setup(r => r.GetAllAsync())
                          .ReturnsAsync(transacoes);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(3);
            result.Data.First().Descricao.Should().Be("Receita 1");
            result.Data.ElementAt(1).Descricao.Should().Be("Despesa 1");
            result.Data.ElementAt(2).Descricao.Should().Be("Receita 2");
        }

        [Fact]
        public async Task GetByPeriodoAsync_QuandoDataInicioMaiorQueDataFim_DeveRetornarFalha()
        {
            // Arrange
            var dataInicio = DateTime.Now;
            var dataFim = DateTime.Now.AddDays(-1);

            // Act
            var result = await _service.GetByPeriodoAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("data inicial não pode ser maior que a data final");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetByPeriodoAsync_QuandoPeriodoMuitoLongo_DeveRetornarFalha()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-400);
            var dataFim = DateTime.Now;

            // Act
            var result = await _service.GetByPeriodoAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("não pode ser maior que 1 ano");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetByPeriodoAsync_QuandoPeriodoValido_DeveRetornarTransacoesDoPeriodo()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            
            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-5), "Receita", 100m),
                new Transacao(TipoTransacao.Despesa, DateTime.Now.AddDays(-8), "Despesa", 50m)
            };
            
            _repositoryMock.Setup(r => r.GetByPeriodoAsync(dataInicio, dataFim))
                          .ReturnsAsync(transacoes);

            // Act
            var result = await _service.GetByPeriodoAsync(dataInicio, dataFim);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByTipoAsync_QuandoTipoInvalido_DeveRetornarFalha()
        {
            // Arrange
            var tipoInvalido = 999;

            // Act
            var result = await _service.GetByTipoAsync(tipoInvalido);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Tipo de transação inválido");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetByTipoAsync_QuandoTipoValido_DeveRetornarTransacoesDoTipo()
        {
            // Arrange
            var tipo = (int)TipoTransacao.Receita;
            
            var transacoes = new List<Transacao>
            {
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Receita 1", 100m),
                new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-3), "Receita 2", 200m)
            };
            
            _repositoryMock.Setup(r => r.GetByTipoAsync(TipoTransacao.Receita))
                          .ReturnsAsync(transacoes);

            // Act
            var result = await _service.GetByTipoAsync(tipo);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.All(t => t.Tipo == tipo).Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_QuandoDTOInvalido_DeveRetornarFalha()
        {
            // Arrange
            var dto = new CreateTransacaoDTO
            {
                Tipo = 0,
                Data = DateTime.Now,
                Descricao = "",  // Inválido, pois a descrição é obrigatória
                Valor = 100m
            };
            
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Descricao", "A descrição da transação é obrigatória")
            });
            
            // Configurando o validador de teste
            _createTransacaoValidator.SetValidationResult(validationResult);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("A descrição da transação é obrigatória");
        }

        [Fact]
        public async Task AddAsync_QuandoTipoInvalido_DeveRetornarFalha()
        {
            // Arrange
            var dto = new CreateTransacaoDTO
            {
                Tipo = 999,  // Tipo inválido
                Data = DateTime.Now,
                Descricao = "Teste",
                Valor = 100m
            };
            
            var validationResult = new ValidationResult();  // Validação passa
            
            // Configurando o validador de teste
            _createTransacaoValidator.SetValidationResult(validationResult);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Tipo de transação inválido");
        }

        [Fact]
        public async Task AddAsync_QuandoDadosValidos_DeveCriarTransacao()
        {
            // Arrange
            var dto = new CreateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Teste",
                Valor = 100m
            };
            
            var validationResult = new ValidationResult();  // Validação passa
            var transacaoId = Guid.NewGuid();
            
            // Configurando o validador de teste
            _createTransacaoValidator.SetValidationResult(validationResult);
            
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                          .ReturnsAsync(transacaoId);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Be(transacaoId);
            result.Message.Should().Contain("sucesso");
            
            _repositoryMock.Verify(r => r.AddAsync(It.Is<Transacao>(t =>
                t.Tipo == TipoTransacao.Receita &&
                t.Descricao == dto.Descricao &&
                t.Valor == dto.Valor
            )), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_QuandoTransacaoNaoExiste_DeveRetornarFalha()
        {
            // Arrange
            var id = Guid.NewGuid();
            
            _repositoryMock.Setup(r => r.ExistsAsync(id))
                          .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("não encontrada");
            
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
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("sucesso");
            
            _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }
    }
} 
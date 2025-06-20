using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleFinanceiro.API.Controllers;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ControleFinanceiro.API.Tests.Controllers
{
    public class TransacoesControllerTests
    {
        private readonly Mock<ITransacaoService> _transacaoServiceMock;
        private readonly TransacoesController _controller;

        public TransacoesControllerTests()
        {
            _transacaoServiceMock = new Mock<ITransacaoService>();
            _controller = new TransacoesController(_transacaoServiceMock.Object);
        }

        [Fact]
        public async Task GetAll_QuandoHaTransacoes_DeveRetornarOkComLista()
        {
            // Arrange
            var transacoes = new List<TransacaoDTO>
            {
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Transacao 1", Valor = 100m },
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Transacao 2", Valor = 200m }
            };

            _transacaoServiceMock.Setup(s => s.GetAllAsync())
                                .ReturnsAsync(Result<IEnumerable<TransacaoDTO>>.Ok(transacoes));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<IEnumerable<TransacaoDTO>>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_QuandoOcorreErro_DeveRetornarBadRequest()
        {
            // Arrange
            _transacaoServiceMock.Setup(s => s.GetAllAsync())
                                .ReturnsAsync(Result<IEnumerable<TransacaoDTO>>.Fail("Erro ao buscar transações"));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<IEnumerable<TransacaoDTO>>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("Erro ao buscar transações");
        }

        [Fact]
        public async Task GetById_QuandoTransacaoExiste_DeveRetornarOkComTransacao()
        {
            // Arrange
            var id = Guid.NewGuid();
            var transacao = new TransacaoDTO
            {
                Id = id,
                Descricao = "Transacao Teste",
                Valor = 150m
            };

            _transacaoServiceMock.Setup(s => s.GetByIdAsync(id))
                                .ReturnsAsync(Result<TransacaoDTO>.Ok(transacao));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<TransacaoDTO>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.Id.Should().Be(id);
            returnedResult.Data.Descricao.Should().Be("Transacao Teste");
            returnedResult.Data.Valor.Should().Be(150m);
        }

        [Fact]
        public async Task GetById_QuandoTransacaoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.GetByIdAsync(id))
                                .ReturnsAsync(Result<TransacaoDTO>.Fail("Transação não encontrada"));

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<TransacaoDTO>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("Transação não encontrada");
        }

        [Fact]
        public async Task GetByPeriodo_QuandoHaTransacoes_DeveRetornarOkComLista()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            var transacoes = new List<TransacaoDTO>
            {
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Transacao 1", Valor = 100m },
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Transacao 2", Valor = 200m }
            };

            _transacaoServiceMock.Setup(s => s.GetByPeriodoAsync(dataInicio, dataFim))
                                .ReturnsAsync(Result<IEnumerable<TransacaoDTO>>.Ok(transacoes));

            // Act
            var result = await _controller.GetByPeriodo(dataInicio, dataFim);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<IEnumerable<TransacaoDTO>>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByPeriodo_QuandoOcorreErro_DeveRetornarBadRequest()
        {
            // Arrange
            var dataInicio = DateTime.Now;
            var dataFim = DateTime.Now.AddDays(-1); // Data inválida (fim antes do início)

            _transacaoServiceMock.Setup(s => s.GetByPeriodoAsync(dataInicio, dataFim))
                                .ReturnsAsync(Result<IEnumerable<TransacaoDTO>>.Fail("A data inicial não pode ser maior que a data final"));

            // Act
            var result = await _controller.GetByPeriodo(dataInicio, dataFim);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<IEnumerable<TransacaoDTO>>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("A data inicial não pode ser maior que a data final");
        }

        [Fact]
        public async Task GetByTipo_QuandoHaTransacoes_DeveRetornarOkComLista()
        {
            // Arrange
            var tipo = 1; // Receita
            var transacoes = new List<TransacaoDTO>
            {
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Receita 1", Valor = 100m, Tipo = 1 },
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Receita 2", Valor = 200m, Tipo = 1 }
            };

            _transacaoServiceMock.Setup(s => s.GetByTipoAsync(tipo))
                                .ReturnsAsync(Result<IEnumerable<TransacaoDTO>>.Ok(transacoes));

            // Act
            var result = await _controller.GetByTipo(tipo);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<IEnumerable<TransacaoDTO>>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByTipo_QuandoTipoInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var tipoInvalido = 999;

            _transacaoServiceMock.Setup(s => s.GetByTipoAsync(tipoInvalido))
                                .ReturnsAsync(Result<IEnumerable<TransacaoDTO>>.Fail("Tipo de transação inválido"));

            // Act
            var result = await _controller.GetByTipo(tipoInvalido);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<IEnumerable<TransacaoDTO>>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("Tipo de transação inválido");
        }

        [Fact]
        public async Task Create_QuandoDadosValidos_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateTransacaoDTO
            {
                Descricao = "Nova Transacao",
                Valor = 300m,
                Tipo = 0,
                Data = DateTime.Now.AddDays(-1)
            };
            
            var novoId = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.AddAsync(createDto))
                                .ReturnsAsync(Result<Guid>.Ok(novoId));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(TransacoesController.GetById));
            createdAtActionResult.RouteValues["id"].Should().Be(novoId);
            var returnedResult = createdAtActionResult.Value.Should().BeAssignableTo<Result<Guid>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.Should().Be(novoId);
        }

        [Fact]
        public async Task Create_QuandoDadosInvalidos_DeveRetornarBadRequest()
        {
            // Arrange
            var createDto = new CreateTransacaoDTO
            {
                Descricao = "",  // Inválido
                Valor = 0,       // Inválido
                Tipo = 0,
                Data = DateTime.Now.AddDays(-1)
            };
            
            var errors = new List<string>
            {
                "A descrição da transação é obrigatória",
                "O valor da transação deve ser maior que zero"
            };

            _transacaoServiceMock.Setup(s => s.AddAsync(createDto))
                                .ReturnsAsync(Result<Guid>.Fail(errors));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().NotBeNull();
            var returnedResult = badRequestResult.Value.Should().BeAssignableTo<Result<Guid>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task Update_QuandoDadosValidos_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateTransacaoDTO
            {
                Descricao = "Transacao Atualizada",
                Valor = 150m,
                Tipo = 1,
                Data = DateTime.Now.AddDays(-1)
            };

            _transacaoServiceMock.Setup(s => s.UpdateAsync(id, updateDto))
                                .ReturnsAsync(Result<bool>.Ok(true));

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<bool>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Update_QuandoTransacaoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateTransacaoDTO
            {
                Descricao = "Transacao Atualizada",
                Valor = 150m,
                Tipo = 1,
                Data = DateTime.Now.AddDays(-1)
            };

            _transacaoServiceMock.Setup(s => s.UpdateAsync(id, updateDto))
                                .ReturnsAsync(Result<bool>.Fail("Transação não encontrada"));

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<bool>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("Transação não encontrada");
        }

        [Fact]
        public async Task Update_QuandoDadosInvalidos_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var updateDto = new UpdateTransacaoDTO
            {
                Descricao = "",  // Inválido
                Valor = 0,       // Inválido
                Tipo = 0,
                Data = DateTime.Now.AddDays(-1)
            };

            var errors = new List<string>
            {
                "A descrição da transação é obrigatória",
                "O valor da transação deve ser maior que zero"
            };

            _transacaoServiceMock.Setup(s => s.UpdateAsync(id, updateDto))
                                .ReturnsAsync(Result<bool>.Fail(errors));

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<bool>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task Delete_QuandoTransacaoExiste_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.DeleteAsync(id))
                                .ReturnsAsync(Result<bool>.Ok(true));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<bool>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_QuandoTransacaoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.DeleteAsync(id))
                                .ReturnsAsync(Result<bool>.Fail("Transação não encontrada"));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<bool>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("Transação não encontrada");
        }
    }
} 
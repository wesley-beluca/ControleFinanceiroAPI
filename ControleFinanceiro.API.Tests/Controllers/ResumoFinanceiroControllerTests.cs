using System;
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
    public class ResumoFinanceiroControllerTests
    {
        private readonly Mock<IResumoFinanceiroService> _resumoFinanceiroServiceMock;
        private readonly ResumoFinanceiroController _controller;

        public ResumoFinanceiroControllerTests()
        {
            _resumoFinanceiroServiceMock = new Mock<IResumoFinanceiroService>();
            _controller = new ResumoFinanceiroController(_resumoFinanceiroServiceMock.Object);
        }

        [Fact]
        public async Task GetResumoFinanceiro_QuandoPeriodoValido_DeveRetornarOkComResumo()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-30);
            var dataFim = DateTime.Now;
            var resumo = new ResumoFinanceiroDTO
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalReceitas = 5000m,
                TotalDespesas = 3000m,
                SaldoFinal = 2000m
            };

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim))
                                       .ReturnsAsync(Result<ResumoFinanceiroDTO>.Ok(resumo));

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<ResumoFinanceiroDTO>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.TotalReceitas.Should().Be(5000m);
            returnedResult.Data.TotalDespesas.Should().Be(3000m);
            returnedResult.Data.SaldoFinal.Should().Be(2000m);
            returnedResult.Data.Periodo.Should().Be($"{dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}");
        }

        [Fact]
        public async Task GetResumoFinanceiro_QuandoDataInicioMaiorQueDataFim_DeveRetornarBadRequest()
        {
            // Arrange
            var dataInicio = DateTime.Now;
            var dataFim = DateTime.Now.AddDays(-1); // Data inválida (fim antes do início)

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim))
                                       .ReturnsAsync(Result<ResumoFinanceiroDTO>.Fail("A data inicial não pode ser maior que a data final"));

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<ResumoFinanceiroDTO>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("A data inicial não pode ser maior que a data final");
        }

        [Fact]
        public async Task GetResumoFinanceiro_QuandoPeriodoMuitoLongo_DeveRetornarBadRequest()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-400); // Mais de 1 ano
            var dataFim = DateTime.Now;

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim))
                                       .ReturnsAsync(Result<ResumoFinanceiroDTO>.Fail("O período não pode ser maior que 1 ano"));

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<ResumoFinanceiroDTO>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("O período não pode ser maior que 1 ano");
        }

        [Fact]
        public async Task GetResumoFinanceiro_QuandoNaoHaTransacoes_DeveRetornarOkComResumoZerado()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-30);
            var dataFim = DateTime.Now;
            var resumo = new ResumoFinanceiroDTO
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalReceitas = 0m,
                TotalDespesas = 0m,
                SaldoFinal = 0m
            };

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim))
                                       .ReturnsAsync(Result<ResumoFinanceiroDTO>.Ok(resumo));

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<ResumoFinanceiroDTO>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.TotalReceitas.Should().Be(0m);
            returnedResult.Data.TotalDespesas.Should().Be(0m);
            returnedResult.Data.SaldoFinal.Should().Be(0m);
        }

        [Fact]
        public async Task GetResumoFinanceiro_QuandoDespesasMaioresQueReceitas_DeveRetornarSaldoNegativo()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-30);
            var dataFim = DateTime.Now;
            var resumo = new ResumoFinanceiroDTO
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalReceitas = 1000m,
                TotalDespesas = 3000m,
                SaldoFinal = -2000m // Saldo negativo
            };

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim))
                                       .ReturnsAsync(Result<ResumoFinanceiroDTO>.Ok(resumo));

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<ResumoFinanceiroDTO>>().Subject;
            returnedResult.Success.Should().BeTrue();
            returnedResult.Data.TotalReceitas.Should().Be(1000m);
            returnedResult.Data.TotalDespesas.Should().Be(3000m);
            returnedResult.Data.SaldoFinal.Should().Be(-2000m);
        }

        [Fact]
        public async Task GetResumoFinanceiro_QuandoOcorreExcecao_DeveRetornarBadRequest()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-30);
            var dataFim = DateTime.Now;

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim))
                                       .ReturnsAsync(Result<ResumoFinanceiroDTO>.Fail("Erro ao processar o resumo financeiro"));

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedResult = okResult.Value.Should().BeAssignableTo<Result<ResumoFinanceiroDTO>>().Subject;
            returnedResult.Success.Should().BeFalse();
            returnedResult.Message.Should().Be("Erro ao processar o resumo financeiro");
        }
    }
} 
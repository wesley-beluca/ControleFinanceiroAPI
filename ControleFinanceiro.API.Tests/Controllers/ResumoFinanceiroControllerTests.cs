using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ControleFinanceiro.API.Controllers;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Notifications;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ControleFinanceiro.API.Tests.Controllers
{
    public class ResumoFinanceiroControllerTests
    {
        private readonly Mock<IResumoFinanceiroService> _resumoFinanceiroServiceMock;
        private readonly Mock<UserManager<Usuario>> _userManagerMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly ResumoFinanceiroController _controller;

        public ResumoFinanceiroControllerTests()
        {
            _resumoFinanceiroServiceMock = new Mock<IResumoFinanceiroService>();
            _userManagerMock = MockUserManager<Usuario>();
            _notificationServiceMock = new Mock<INotificationService>();
            
            // Configuração padrão para o mock do INotificationService
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem>().AsReadOnly());
            
            _controller = new ResumoFinanceiroController(_resumoFinanceiroServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            
            // Setup user authentication
            var usuarioId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext()
            {
                User = principal
            };
            
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var usuario = new Usuario { Id = usuarioId };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(usuario);
        }
        

        private Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            options.Setup(o => o.Value).Returns(idOptions);

            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);

            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());
            
            var userManager = new Mock<UserManager<TUser>>(store.Object, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);
            return userManager;
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
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
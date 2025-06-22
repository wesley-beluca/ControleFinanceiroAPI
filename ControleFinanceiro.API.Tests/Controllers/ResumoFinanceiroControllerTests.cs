using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ControleFinanceiro.API.Controllers;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Constants;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Notifications;
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
        private readonly Guid _usuarioId;

        public ResumoFinanceiroControllerTests()
        {
            _resumoFinanceiroServiceMock = new Mock<IResumoFinanceiroService>();
            _userManagerMock = MockUserManager<Usuario>();
            _notificationServiceMock = new Mock<INotificationService>();
            
            // Configuração padrão para o mock do INotificationService
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem>());
            
            _controller = new ResumoFinanceiroController(
                _resumoFinanceiroServiceMock.Object, 
                _userManagerMock.Object, 
                _notificationServiceMock.Object);
            
            // Setup user authentication
            _usuarioId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _usuarioId.ToString())
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
            
            var usuario = new Usuario { Id = _usuarioId };
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, _usuarioId))
                .ReturnsAsync(resumo);

            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            
            dynamic response = okResult.Value!;
            Assert.True((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            dynamic dados = response.GetType().GetProperty("dados").GetValue(response)!;
            Assert.Equal(5000m, (decimal)dados.GetType().GetProperty("totalReceitas").GetValue(dados));
            Assert.Equal(3000m, (decimal)dados.GetType().GetProperty("totalDespesas").GetValue(dados));
            Assert.Equal(2000m, (decimal)dados.GetType().GetProperty("saldoFinal").GetValue(dados));
        }

        [Fact]
        public async Task GetResumoFinanceiro_QuandoDataInicioMaiorQueDataFim_DeveRetornarBadRequest()
        {
            // Arrange
            var dataInicio = DateTime.Now;
            var dataFim = DateTime.Now.AddDays(-30); // Data início maior que data fim

            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem>
            {
                new NotificationItem(ChavesNotificacao.DataInicio, MensagensErro.DataInicioMaiorQueFinal)
            });

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            
            dynamic response = badRequestResult.Value!;
            Assert.False((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            // Verifica que o serviço não foi chamado
            _resumoFinanceiroServiceMock.Verify(
                s => s.GerarResumoFinanceiroAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>()), 
                Times.Never);
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, _usuarioId))
                .ReturnsAsync(resumo);

            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            
            dynamic response = okResult.Value!;
            Assert.True((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            dynamic dados = response.GetType().GetProperty("dados").GetValue(response)!;
            Assert.Equal(0m, (decimal)dados.GetType().GetProperty("totalReceitas").GetValue(dados));
            Assert.Equal(0m, (decimal)dados.GetType().GetProperty("totalDespesas").GetValue(dados));
            Assert.Equal(0m, (decimal)dados.GetType().GetProperty("saldoFinal").GetValue(dados));
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

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, _usuarioId))
                .ReturnsAsync(resumo);

            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            
            dynamic response = okResult.Value!;
            Assert.True((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            dynamic dados = response.GetType().GetProperty("dados").GetValue(response)!;
            Assert.Equal(1000m, (decimal)dados.GetType().GetProperty("totalReceitas").GetValue(dados));
            Assert.Equal(3000m, (decimal)dados.GetType().GetProperty("totalDespesas").GetValue(dados));
            Assert.Equal(-2000m, (decimal)dados.GetType().GetProperty("saldoFinal").GetValue(dados));
        }

        [Fact]
        public async Task GetResumoFinanceiro_QuandoOcorreExcecao_DeveRetornarBadRequest()
        {
            // Arrange
            var dataInicio = DateTime.Now.AddDays(-30);
            var dataFim = DateTime.Now;

            _resumoFinanceiroServiceMock.Setup(s => s.GerarResumoFinanceiroAsync(dataInicio, dataFim, _usuarioId))
                .ReturnsAsync(() => null);

            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem>
            {
                new NotificationItem(ChavesNotificacao.Erro, "Erro ao processar o resumo financeiro")
            });

            // Act
            var result = await _controller.GetResumoFinanceiro(dataInicio, dataFim);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            
            dynamic response = badRequestResult.Value!;
            Assert.False((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            var erros = response.GetType().GetProperty("erros").GetValue(response) as IEnumerable<object>;
            Assert.NotEmpty(erros!);
        }
    }
}

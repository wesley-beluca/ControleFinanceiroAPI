using ControleFinanceiro.API.Controllers;
using ControleFinanceiro.Application.DTOs.Auth;
using ControleFinanceiro.Domain.Constants;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ControleFinanceiro.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<UserManager<Usuario>> _mockUserManager;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockNotificationService = new Mock<INotificationService>();
            
            // Configuração padrão para o mock do INotificationService
            _mockNotificationService.Setup(n => n.HasNotifications).Returns(false);
            _mockNotificationService.Setup(n => n.Notifications).Returns(new List<NotificationItem>());
            _mockNotificationService.Setup(n => n.Clear()).Verifiable();
            _mockNotificationService.Setup(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            
            // Configuração do mock do UserManager
            var store = new Mock<IUserStore<Usuario>>();
            _mockUserManager = new Mock<UserManager<Usuario>>(store.Object, null, null, null, null, null, null, null, null);
            
            _controller = new AuthController(_mockAuthService.Object, _mockEmailService.Object, _mockNotificationService.Object, _mockUserManager.Object);
        }

        [Fact]
        public async Task ForgotPassword_ComEmailValido_EnviaEmailERetornaOk()
        {
            // Arrange
            var model = new ForgotPasswordDTO { Email = "usuario@example.com" };
            var usuario = new Usuario("testuser", model.Email, "Password123!");
            var token = "token_reset_senha_123";

            _mockAuthService.Setup(x => x.SolicitarResetSenhaAsync(model.Email))
                .ReturnsAsync((usuario, token));

            _mockEmailService.Setup(x => x.EnviarEmailResetSenhaAsync(model.Email, token, usuario.UserName))
                .ReturnsAsync(true);

            _mockNotificationService.Setup(n => n.HasNotifications).Returns(false);

            // Act
            var resultado = await _controller.ForgotPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            
            dynamic response = okResult.Value!;
            Assert.True((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            dynamic dados = response.GetType().GetProperty("dados").GetValue(response)!;
            Assert.Equal("Instruções para redefinição de senha foram enviadas para seu email", dados.GetType().GetProperty("mensagem").GetValue(dados));
            
            _mockAuthService.Verify(x => x.SolicitarResetSenhaAsync(model.Email), Times.Once);
            _mockEmailService.Verify(x => x.EnviarEmailResetSenhaAsync(model.Email, token, usuario.UserName), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ComEmailInvalido_RetornaBadRequest()
        {
            // Arrange
            var model = new ForgotPasswordDTO { Email = "usuario_inexistente@example.com" };

            _mockAuthService.Setup(x => x.SolicitarResetSenhaAsync(model.Email))
                .ReturnsAsync((null, null));
                
            // Simular que o serviço adicionou notificações

            _mockNotificationService.Setup(n => n.HasNotifications).Returns(true);
            _mockNotificationService.Setup(n => n.Notifications).Returns(new List<NotificationItem> 
            { 
                new NotificationItem(ChavesNotificacao.Usuario, MensagensErro.UsuarioNaoEncontrado) 
            });

            // Act
            var resultado = await _controller.ForgotPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            
            dynamic response = badRequestResult.Value!;
            Assert.False((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            _mockAuthService.Verify(x => x.SolicitarResetSenhaAsync(model.Email), Times.Once);
            _mockEmailService.Verify(x => x.EnviarEmailResetSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPassword_QuandoEmailNaoEnvia_RetornaOkComMensagemGenerica()
        {
            // Arrange
            var model = new ForgotPasswordDTO { Email = "usuario@example.com" };
            var usuario = new Usuario("testuser", model.Email, "Password123!");
            var token = "token_reset_senha_123";

            _mockAuthService.Setup(x => x.SolicitarResetSenhaAsync(model.Email))
                .ReturnsAsync((usuario, token));

            _mockEmailService.Setup(x => x.EnviarEmailResetSenhaAsync(model.Email, token, usuario.UserName))
                .ReturnsAsync(false);
                
            // Simular que o serviço de email adicionou notificação
            _mockNotificationService.Setup(n => n.AddNotification(ChavesNotificacao.Email, MensagensErro.ErroEnvioEmail)).Verifiable();

            _mockNotificationService.Setup(n => n.HasNotifications).Returns(false);

            // Act
            var resultado = await _controller.ForgotPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            
            dynamic response = okResult.Value!;
            Assert.True((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            dynamic dados = response.GetType().GetProperty("dados").GetValue(response)!;
            Assert.Equal("Se o email existir em nossa base de dados, você receberá instruções para redefinição de senha.", dados.GetType().GetProperty("mensagem").GetValue(dados));
            
            _mockAuthService.Verify(x => x.SolicitarResetSenhaAsync(model.Email), Times.Once);
            _mockEmailService.Verify(x => x.EnviarEmailResetSenhaAsync(model.Email, token, usuario.UserName), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ComModelStateInvalido_RetornaBadRequest()
        {
            // Arrange
            var model = new ForgotPasswordDTO { Email = "" };
            _controller.ModelState.AddModelError("Email", "O campo Email é obrigatório");

            // Act
            var resultado = await _controller.ForgotPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            
            _mockAuthService.Verify(x => x.SolicitarResetSenhaAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPassword_ComTokenValido_RetornaOk()
        {
            // Arrange
            var model = new ResetPasswordDTO 
            { 
                Token = "token_valido_123", 
                Password = "NovaSenha123!", 
                ConfirmPassword = "NovaSenha123!" 
            };

            _mockAuthService.Setup(x => x.ResetSenhaAsync(model.Token, model.Password))
                .ReturnsAsync(true);

            _mockNotificationService.Setup(n => n.HasNotifications).Returns(false);

            // Act
            var resultado = await _controller.ResetPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            
            dynamic response = okResult.Value!;
            Assert.True((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            dynamic dados = response.GetType().GetProperty("dados").GetValue(response)!;
            Assert.Equal("Senha redefinida com sucesso", dados.GetType().GetProperty("mensagem").GetValue(dados));
            
            _mockAuthService.Verify(x => x.ResetSenhaAsync(model.Token, model.Password), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_ComTokenInvalido_RetornaBadRequest()
        {
            // Arrange
            var model = new ResetPasswordDTO 
            { 
                Token = "token_invalido_123", 
                Password = "NovaSenha123!", 
                ConfirmPassword = "NovaSenha123!" 
            };

            _mockAuthService.Setup(x => x.ResetSenhaAsync(model.Token, model.Password))
                .ReturnsAsync(false);

            _mockNotificationService.Setup(n => n.HasNotifications).Returns(true);
            _mockNotificationService.Setup(n => n.Notifications).Returns(new List<NotificationItem> 
            { 
                new NotificationItem(ChavesNotificacao.Token, MensagensErro.TokenInvalido) 
            });

            // Act
            var resultado = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            
            dynamic response = badRequestResult.Value!;
            Assert.False((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            _mockAuthService.Verify(x => x.ResetSenhaAsync(model.Token, model.Password), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_ComSenhasNaoCorrespondentes_RetornaBadRequest()
        {
            // Arrange
            var model = new ResetPasswordDTO 
            { 
                Token = "token_valido_123", 
                Password = "NovaSenha123!", 
                ConfirmPassword = "SenhaDiferente456!" 
            };

            // Act
            var resultado = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            
            dynamic response = badRequestResult.Value!;
            Assert.False((bool)response.GetType().GetProperty("sucesso").GetValue(response)!);
            
            _mockAuthService.Verify(x => x.ResetSenhaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPassword_ComModelStateInvalido_RetornaBadRequest()
        {
            // Arrange
            var model = new ResetPasswordDTO 
            { 
                Token = "", 
                Password = "", 
                ConfirmPassword = "" 
            };
            _controller.ModelState.AddModelError("Token", "O campo Token é obrigatório");
            
            // Verificar que o ModelState inválido adiciona notificações via ValidarModelState

            // Act
            var resultado = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            
            _mockAuthService.Verify(x => x.ResetSenhaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}

using ControleFinanceiro.API.Controllers;
using ControleFinanceiro.Application.DTOs.Auth;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Notifications;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ControleFinanceiro.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockNotificationService = new Mock<INotificationService>();
            
            // Configuração padrão para o mock do INotificationService
            _mockNotificationService.Setup(n => n.HasNotifications).Returns(false);
            _mockNotificationService.Setup(n => n.Notifications).Returns(new List<NotificationItem>().AsReadOnly());
            
            _controller = new AuthController(_mockAuthService.Object, _mockEmailService.Object, _mockNotificationService.Object);
        }

        [Fact]
        public async Task ForgotPassword_ComEmailValido_EnviaEmailERetornaOk()
        {
            // Arrange
            var model = new ForgotPasswordDTO { Email = "usuario@example.com" };
            var usuario = new Usuario("testuser", model.Email, "Password123!");
            var token = "token_reset_senha_123";

            _mockAuthService.Setup(x => x.SolicitarResetSenhaAsync(model.Email))
                .ReturnsAsync((true, "Token de redefinição de senha gerado com sucesso", usuario, token));

            _mockEmailService.Setup(x => x.EnviarEmailResetSenhaAsync(model.Email, token, usuario.UserName))
                .ReturnsAsync(true);

            // Act
            var resultado = await _controller.ForgotPassword(model) as OkObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(200, resultado.StatusCode);
            
            var json = JsonSerializer.Serialize(resultado.Value);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(json);
            var message = responseObj.GetProperty("message").GetString();
            Assert.Equal("Instruções para redefinição de senha foram enviadas para seu email", message);
            
            _mockAuthService.Verify(x => x.SolicitarResetSenhaAsync(model.Email), Times.Once);
            _mockEmailService.Verify(x => x.EnviarEmailResetSenhaAsync(model.Email, token, usuario.UserName), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ComEmailInvalido_RetornaBadRequest()
        {
            // Arrange
            var model = new ForgotPasswordDTO { Email = "usuario_inexistente@example.com" };

            _mockAuthService.Setup(x => x.SolicitarResetSenhaAsync(model.Email))
                .ReturnsAsync((false, "Email não encontrado", null, null));

            // Act
            var resultado = await _controller.ForgotPassword(model) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(400, resultado.StatusCode);
            
            var json = JsonSerializer.Serialize(resultado.Value);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(json);
            var message = responseObj.GetProperty("message").GetString();
            Assert.Equal("Email não encontrado", message);
            
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
                .ReturnsAsync((true, "Token de redefinição de senha gerado com sucesso", usuario, token));

            _mockEmailService.Setup(x => x.EnviarEmailResetSenhaAsync(model.Email, token, usuario.UserName))
                .ReturnsAsync(false);

            // Act
            var resultado = await _controller.ForgotPassword(model) as OkObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(200, resultado.StatusCode);
            
            var json = JsonSerializer.Serialize(resultado.Value);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(json);
            var message = responseObj.GetProperty("message").GetString();
            Assert.Equal("Se o email existir em nossa base de dados, você receberá instruções para redefinição de senha.", message);
            
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
            var resultado = await _controller.ForgotPassword(model) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(400, resultado.StatusCode);
            
            _mockAuthService.Verify(x => x.SolicitarResetSenhaAsync(It.IsAny<string>()), Times.Never);
            _mockEmailService.Verify(x => x.EnviarEmailResetSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
                .ReturnsAsync((true, "Senha redefinida com sucesso"));

            // Act
            var resultado = await _controller.ResetPassword(model) as OkObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(200, resultado.StatusCode);
            
            var json = JsonSerializer.Serialize(resultado.Value);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(json);
            var message = responseObj.GetProperty("message").GetString();
            Assert.Equal("Senha redefinida com sucesso", message);
            
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
                .ReturnsAsync((false, "Token inválido"));

            // Act
            var resultado = await _controller.ResetPassword(model) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(400, resultado.StatusCode);
            
            var json = JsonSerializer.Serialize(resultado.Value);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(json);
            var message = responseObj.GetProperty("message").GetString();
            Assert.Equal("Token inválido", message);
            
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
            var resultado = await _controller.ResetPassword(model) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(400, resultado.StatusCode);
            
            var json = JsonSerializer.Serialize(resultado.Value);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(json);
            var message = responseObj.GetProperty("message").GetString();
            Assert.Equal("As senhas não conferem", message);
            
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

            // Act
            var resultado = await _controller.ResetPassword(model) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(400, resultado.StatusCode);
            
            _mockAuthService.Verify(x => x.ResetSenhaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}

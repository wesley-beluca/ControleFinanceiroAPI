using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Notifications;
using ControleFinanceiro.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
#nullable enable

namespace ControleFinanceiro.Infrastructure.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<UserManager<Usuario>> _mockUserManager;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUsuarioRepository = new Mock<IUsuarioRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockNotificationService = new Mock<INotificationService>();
            
            // Setup for UserManager mock
            var userStoreMock = new Mock<IUserStore<Usuario>>();
            _mockUserManager = new Mock<UserManager<Usuario>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("chave_secreta_para_testes_unitarios_1234567890");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("teste_issuer");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("teste_audience");
            
            // Configuração padrão para o mock do INotificationService
            _mockNotificationService.Setup(n => n.HasNotifications).Returns(false);
            _mockNotificationService.Setup(n => n.Notifications).Returns(new List<NotificationItem>().AsReadOnly());
            _mockNotificationService.Setup(n => n.AddNotification(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            _mockNotificationService.Setup(n => n.Clear()).Verifiable();
            
            _authService = new AuthService(_mockUsuarioRepository.Object, _mockConfiguration.Object, _mockUserManager.Object, _mockNotificationService.Object);
        }

        [Fact]
        public async Task SolicitarResetSenhaAsync_ComEmailValido_RetornaUsuarioEToken()
        {
            // Arrange
            var email = "usuario@example.com";
            var usuario = new Usuario("testuser", email, "Password123!");
            
            _mockUsuarioRepository.Setup(x => x.ObterPorEmailAsync(email))
                .ReturnsAsync(usuario);
            
            _mockUsuarioRepository.Setup(x => x.AtualizarAsync(It.IsAny<Usuario>()))
                .ReturnsAsync(usuario);
            
            // Act
            var resultado = await _authService.SolicitarResetSenhaAsync(email);
            
            // Assert
            Assert.True(resultado.sucesso);
            Assert.Equal("Token de redefinição de senha gerado com sucesso", resultado.mensagem);
            Assert.NotNull(resultado.usuario);
            Assert.Equal(email, resultado.usuario.Email);
            Assert.NotNull(resultado.token);
            _mockUsuarioRepository.Verify(x => x.AtualizarAsync(It.IsAny<Usuario>()), Times.Once);
        }
        
        [Fact]
        public async Task SolicitarResetSenhaAsync_ComEmailInvalido_RetornaFalha()
        {
            // Arrange
            var email = "usuario_inexistente@example.com";
            
            _mockUsuarioRepository.Setup(x => x.ObterPorEmailAsync(email))
                .ReturnsAsync((Usuario?)null);
            
            // Act
            var resultado = await _authService.SolicitarResetSenhaAsync(email);
            
            // Assert
            Assert.False(resultado.sucesso);
            Assert.Equal("Email não encontrado", resultado.mensagem);
            Assert.Null(resultado.usuario);
            Assert.Null(resultado.token);
            _mockUsuarioRepository.Verify(x => x.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
        }
        
        [Fact]
        public async Task ResetSenhaAsync_ComTokenValido_RetornaSucesso()
        {
            // Arrange
            var token = "token_valido_123";
            var novaSenha = "NovaSenha123!";
            
            // Criar um usuário real com token válido
            var usuario = new Usuario("testuser", "usuario@example.com", "Password123!");
            
            // Usar reflection para definir as propriedades privadas
            typeof(Usuario).GetProperty("ResetPasswordToken")
                .SetValue(usuario, token);
                
            typeof(Usuario).GetProperty("ResetPasswordTokenExpiration")
                .SetValue(usuario, DateTime.Now.AddHours(1)); // Token válido (expira em 1 hora)
            
            _mockUsuarioRepository.Setup(x => x.ObterPorResetTokenAsync(token))
                .ReturnsAsync(usuario);
                
            _mockUsuarioRepository.Setup(x => x.AtualizarAsync(It.IsAny<Usuario>()))
                .ReturnsAsync(usuario);
            
            // Act
            var resultado = await _authService.ResetSenhaAsync(token, novaSenha);
            
            // Assert
            Assert.True(resultado.sucesso);
            Assert.Equal("Senha redefinida com sucesso", resultado.mensagem);
            _mockUsuarioRepository.Verify(x => x.AtualizarAsync(It.IsAny<Usuario>()), Times.Once);
        }
        
        [Fact]
        public async Task ResetSenhaAsync_ComTokenInvalido_RetornaFalha()
        {
            // Arrange
            var token = "token_invalido_123";
            var novaSenha = "NovaSenha123!";
            
            _mockUsuarioRepository.Setup(x => x.ObterPorResetTokenAsync(token))
                .ReturnsAsync((Usuario?)null);
            
            // Act
            var resultado = await _authService.ResetSenhaAsync(token, novaSenha);
            
            // Assert
            Assert.False(resultado.sucesso);
            Assert.Equal("Token inválido", resultado.mensagem);
            _mockUsuarioRepository.Verify(x => x.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
        }
        
        [Fact]
        public async Task ResetSenhaAsync_ComTokenExpirado_RetornaFalha()
        {
            // Arrange
            var token = "token_expirado_123";
            var novaSenha = "NovaSenha123!";
            
            // Criar um usuário real com token expirado
            var usuario = new Usuario("testuser", "usuario@example.com", "Password123!");
            
            // Usar reflection para definir as propriedades privadas
            typeof(Usuario).GetProperty("ResetPasswordToken")
                .SetValue(usuario, token);
                
            var expirationProperty = typeof(Usuario).GetProperty("ResetPasswordTokenExpiration");
            if (expirationProperty != null)
                expirationProperty.SetValue(usuario, DateTime.Now.AddHours(-1)); // Token expirado (expirou há 1 hora)
            
            _mockUsuarioRepository.Setup(x => x.ObterPorResetTokenAsync(token))
                .ReturnsAsync(usuario);
            
            // Act
            var resultado = await _authService.ResetSenhaAsync(token, novaSenha);
            
            // Assert
            Assert.False(resultado.sucesso);
            Assert.Equal("Token expirado", resultado.mensagem);
            _mockUsuarioRepository.Verify(x => x.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
        }
    }
}

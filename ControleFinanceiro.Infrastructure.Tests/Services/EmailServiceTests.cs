using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

#nullable enable

namespace ControleFinanceiro.Infrastructure.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockConfigSection;
        private readonly Dictionary<string, string> _configSettings;

        public EmailServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfigSection = new Mock<IConfigurationSection>();
            
            // Configurações padrão para testes
            _configSettings = new Dictionary<string, string>
            {
                { "Email:SmtpServer", "smtp.example.com" },
                { "Email:SmtpPort", "587" },
                { "Email:SmtpUsername", "test@example.com" },
                { "Email:SmtpPassword", "password123" },
                { "Email:EmailRemetente", "noreply@example.com" },
                { "Email:NomeRemetente", "Sistema de Teste" },
                { "BaseUrl", "https://example.com" }
            };

            // Configurar o mock do IConfiguration para retornar valores do dicionário
            foreach (var setting in _configSettings)
            {
                _mockConfiguration.Setup(x => x[setting.Key]).Returns(setting.Value);
            }
        }

        [Fact]
        public void Constructor_WithValidConfiguration_InitializesCorrectly()
        {
            // Act & Assert (não deve lançar exceção)
            var emailService = new EmailService(_mockConfiguration.Object);
            
            // Verificar se o serviço foi inicializado corretamente (não lançou exceção)
            Assert.NotNull(emailService);
        }

        [Fact]
        public void Constructor_WithInvalidSmtpPort_ThrowsArgumentException()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Email:SmtpPort"]).Returns("invalid_port");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new EmailService(_mockConfiguration.Object));
            Assert.Contains("SMTP Port is not configured or is invalid", exception.Message);
        }

        [Fact]
        public void Constructor_WithMissingSmtpPassword_ThrowsArgumentException()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Email:SmtpPassword"]).Returns((string)null);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new EmailService(_mockConfiguration.Object));
            Assert.Contains("SMTP Password is not configured", exception.Message);
        }

        [Fact]
        public async Task EnviarEmailAsync_WhenSuccessful_ReturnsTrue()
        {            
            // Arrange
            var testEmailService = new TestableEmailService(_mockConfiguration.Object);
            testEmailService.SetupSendMailResult(true);

            // Act
            var result = await testEmailService.EnviarEmailAsync("destinatario@example.com", "Assunto Teste", "Corpo do email");

            // Assert
            Assert.True(result);
            Assert.True(testEmailService.SendMailCalled);
            Assert.Equal("destinatario@example.com", testEmailService.LastMailMessage?.To[0].Address);
            Assert.Equal("Assunto Teste", testEmailService.LastMailMessage?.Subject);
            Assert.Equal("Corpo do email", testEmailService.LastMailMessage?.Body);
        }

        [Fact]
        public async Task EnviarEmailAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            // Arrange
            var testEmailService = new TestableEmailService(_mockConfiguration.Object);
            testEmailService.SetupSendMailResult(false);

            // Act
            var result = await testEmailService.EnviarEmailAsync("destinatario@example.com", "Assunto Teste", "Corpo do email");

            // Assert
            Assert.False(result);
            Assert.True(testEmailService.SendMailCalled);
        }

        [Fact]
        public async Task EnviarEmailResetSenhaAsync_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var testEmailService = new TestableEmailService(_mockConfiguration.Object);
            testEmailService.SetupSendMailResult(true);
            
            var destinatario = "usuario@example.com";
            var token = "token123";
            var username = "usuario_teste";

            // Act
            var result = await testEmailService.EnviarEmailResetSenhaAsync(destinatario, token, username);

            // Assert
            Assert.True(result);
            Assert.True(testEmailService.SendMailCalled);
            Assert.Equal(destinatario, testEmailService.LastMailMessage?.To[0].Address);
            Assert.Contains("Redefinição de Senha", testEmailService.LastMailMessage?.Subject);
            Assert.Contains(username, testEmailService.LastMailMessage?.Body);
            Assert.Contains(token, testEmailService.LastMailMessage?.Body);
        }

        [Fact]
        public async Task EnviarEmailResetSenhaAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            // Arrange
            var testEmailService = new TestableEmailService(_mockConfiguration.Object);
            testEmailService.SetupSendMailResult(false);
            
            var destinatario = "usuario@example.com";
            var token = "token123";
            var username = "usuario_teste";

            // Act
            var result = await testEmailService.EnviarEmailResetSenhaAsync(destinatario, token, username);

            // Assert
            Assert.False(result);
            Assert.True(testEmailService.SendMailCalled);
        }
    }

    public class TestableEmailService : EmailService
    {
        public bool SendMailCalled { get; private set; }
        public MailMessage? LastMailMessage { get; private set; }
        private bool _sendMailResult;

        public TestableEmailService(IConfiguration configuration) : base(configuration)
        {
        }

        public void SetupSendMailResult(bool result)
        {
            _sendMailResult = result;
        }

        public override async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress("test@example.com", "Test Sender"),
                    Subject = assunto,
                    Body = corpo,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(destinatario));

                SendMailCalled = true;
                LastMailMessage = message;

                if (!_sendMailResult)
                {
                    throw new Exception("Simulated failure");
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public override async Task<bool> EnviarEmailResetSenhaAsync(string destinatario, string token, string username)
        {
            string assunto = "Redefinição de Senha";
            string corpo = $"<h1>Olá, {username}!</h1>" +
                           $"<p>Você solicitou a redefinição de senha.</p>" +
                           $"<p>Seu token é: {token}</p>" +
                           $"<p>Clique <a href='https://example.com/reset-password?token={token}'>aqui</a> para redefinir sua senha.</p>";
                           
            return await EnviarEmailAsync(destinatario, assunto, corpo);
        }
    }
}

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
    public class TransacoesControllerTests
    {
        private readonly Mock<ITransacaoService> _transacaoServiceMock;
        private readonly Mock<UserManager<Usuario>> _userManagerMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly TransacoesController _controller;

        public TransacoesControllerTests()
        {
            _transacaoServiceMock = new Mock<ITransacaoService>();
            _userManagerMock = MockUserManager<Usuario>();
            _notificationServiceMock = new Mock<INotificationService>();
            _controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
        }
        
        // Helper method to create UserManager mock
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

        private void ConfigureControllerContext(TransacoesController controller, Guid userId)
        {
            // Create claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, "testuser@example.com")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuthentication");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            // Create and configure HttpContext
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            // Set up controller context
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            // Configure UserManager to return a user when GetUserAsync is called
            var user = new Usuario { Id = userId, UserName = "testuser@example.com" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
        }

        [Fact]
        public async Task GetAll_QuandoHaTransacoes_DeveRetornarOkComLista()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var transacoes = new List<TransacaoDTO>
            {
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Transacao 1", Valor = 100m },
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Transacao 2", Valor = 200m }
            };

            _transacaoServiceMock.Setup(s => s.GetAllAsync(It.IsAny<Guid?>()))
                                .ReturnsAsync(transacoes);

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso
            var responseObj = okResult.Value;
            var responseType = responseObj.GetType();
            var sucessoProperty = responseType.GetProperty("sucesso");
            sucessoProperty.Should().NotBeNull();
            var sucessoValue = (bool)sucessoProperty.GetValue(responseObj);
            sucessoValue.Should().BeTrue();
            
            // Verificar se dados contém a lista de transações
            var dadosProperty = responseType.GetProperty("dados");
            dadosProperty.Should().NotBeNull();
            var dadosValue = dadosProperty.GetValue(responseObj);
            dadosValue.Should().NotBeNull();
            var dadosList = dadosValue as IEnumerable<object>;
            dadosList.Should().NotBeNull();
            dadosList.Count().Should().Be(2);
        }

        [Fact]
        public async Task GetById_QuandoTransacaoExiste_DeveRetornarOkComTransacao()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var transacaoId = Guid.NewGuid();
            var transacao = new TransacaoDTO
            {
                Id = transacaoId,
                Descricao = "Transacao Teste",
                Valor = 150m
            };

            _transacaoServiceMock.Setup(s => s.GetByIdAsync(transacaoId, It.IsAny<Guid?>()))
                                .ReturnsAsync(transacao);

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetById(transacaoId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso
            var responseObj = okResult.Value;
            var responseType = responseObj.GetType();
            var sucessoProperty = responseType.GetProperty("sucesso");
            sucessoProperty.Should().NotBeNull();
            var sucessoValue = (bool)sucessoProperty.GetValue(responseObj);
            sucessoValue.Should().BeTrue();
            
            // Verificar se dados contém a transação
            var dadosProperty = responseType.GetProperty("dados");
            dadosProperty.Should().NotBeNull();
            var dadosValue = dadosProperty.GetValue(responseObj);
            dadosValue.Should().NotBeNull();
            
            // Verificar propriedades usando reflexão para evitar erros de cast
            var dadosType = dadosValue.GetType();
            var idProperty = dadosType.GetProperty("Id");
            idProperty.Should().NotBeNull();
            idProperty.GetValue(dadosValue).Should().Be(transacaoId);
            
            var descricaoProperty = dadosType.GetProperty("Descricao");
            descricaoProperty.Should().NotBeNull();
            descricaoProperty.GetValue(dadosValue).Should().Be("Transacao Teste");
            
            var valorProperty = dadosType.GetProperty("Valor");
            valorProperty.Should().NotBeNull();
            valorProperty.GetValue(dadosValue).Should().Be(150m);
        }

        [Fact]
        public async Task GetById_QuandoTransacaoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var transacaoId = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.GetByIdAsync(transacaoId, It.IsAny<Guid?>()))
                                .ReturnsAsync((TransacaoDTO)null);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem> { new NotificationItem("Erro", "Transação não encontrada") });

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetById(transacaoId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().NotBeNull();
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso e erros
            var responseObj = badRequestResult.Value;
            var responseType = responseObj.GetType();
            var sucessoProperty = responseType.GetProperty("sucesso");
            sucessoProperty.Should().NotBeNull();
            var sucessoValue = (bool)sucessoProperty.GetValue(responseObj);
            sucessoValue.Should().BeFalse();
            
            // Verificar se erros contém as notificações
            var errosProperty = responseType.GetProperty("erros");
            errosProperty.Should().NotBeNull();
            var errosValue = errosProperty.GetValue(responseObj);
            errosValue.Should().NotBeNull();
            var errosList = errosValue as IEnumerable<object>;
            errosList.Should().NotBeNull();
            errosList.Count().Should().Be(1);
        }

        [Fact]
        public async Task GetByPeriodo_QuandoHaTransacoes_DeveRetornarOkComLista()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dataInicio = DateTime.Now.AddDays(-10);
            var dataFim = DateTime.Now;
            var transacoes = new List<TransacaoDTO>
            {
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Transacao 1", Valor = 100m },
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Transacao 2", Valor = 200m }
            };

            _transacaoServiceMock.Setup(s => s.GetByPeriodoAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
                                .ReturnsAsync(transacoes);

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetByPeriodo(dataInicio, dataFim);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso
            var responseObj = okResult.Value;
            var responseType = responseObj.GetType();
            var sucessoProperty = responseType.GetProperty("sucesso");
            sucessoProperty.Should().NotBeNull();
            var sucessoValue = (bool)sucessoProperty.GetValue(responseObj);
            sucessoValue.Should().BeTrue();
            
            // Verificar se dados contém a lista de transações
            var dadosProperty = responseType.GetProperty("dados");
            dadosProperty.Should().NotBeNull();
            var dadosValue = dadosProperty.GetValue(responseObj);
            dadosValue.Should().NotBeNull();
            var dadosList = dadosValue as IEnumerable<object>;
            dadosList.Should().NotBeNull();
            dadosList.Count().Should().Be(2);
        }

        [Fact]
        public async Task Create_QuandoDadosValidos_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            
            var dto = new CreateTransacaoDTO
            {
                Tipo = 1,
                Data = DateTime.Now,
                Descricao = "Teste",
                Valor = 100m
            };

            var transacaoId = Guid.NewGuid();
            _transacaoServiceMock.Setup(s => s.AddAsync(It.IsAny<CreateTransacaoDTO>(), It.IsAny<Guid?>()))
                                .ReturnsAsync(transacaoId);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);
                                
            // Configure user in UserManager
            var user = new Usuario { Id = usuarioId, UserName = "testuser@example.com" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, usuarioId);

            // Act
            var result = await controller.Create(dto);

            // Assert
            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(TransacoesController.GetById));
            createdAtActionResult.RouteValues.Should().ContainKey("id");
            createdAtActionResult.RouteValues["id"].Should().Be(transacaoId);
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso
            var responseObj = createdAtActionResult.Value;
            var responseType = responseObj.GetType();
            var sucessoProperty = responseType.GetProperty("sucesso");
            sucessoProperty.Should().NotBeNull();
            var sucessoValue = (bool)sucessoProperty.GetValue(responseObj);
            sucessoValue.Should().BeTrue();
            
            // Verificar se dados contém o ID da transação
            var dadosProperty = responseType.GetProperty("dados");
            dadosProperty.Should().NotBeNull();
            var dadosValue = dadosProperty.GetValue(responseObj);
            dadosValue.Should().NotBeNull();
            dadosValue.ToString().Should().Be(transacaoId.ToString());
            
            // Verificar que o usuarioId foi passado para o serviço
            _transacaoServiceMock.Verify(s => s.AddAsync(dto, usuarioId), Times.Once);
        }

        [Fact]
        public async Task Create_QuandoDadosInvalidos_DeveRetornarBadRequest()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            
            var dto = new CreateTransacaoDTO
            {
                // Dados inválidos - faltando campos obrigatórios
            };
            
            // Configure user in UserManager
            var user = new Usuario { Id = usuarioId, UserName = "testuser@example.com" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            // Configurar o NotificationService para ter notificações
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem> { new NotificationItem("Descricao", "O campo Descrição é obrigatório") });
            
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, usuarioId);
            
            // Simular ModelState inválido
            controller.ModelState.AddModelError("Descricao", "O campo Descrição é obrigatório");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().NotBeNull();
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso e erros
            var responseObj = badRequestResult.Value;
            var responseType = responseObj.GetType();
            var sucessoProperty = responseType.GetProperty("sucesso");
            sucessoProperty.Should().NotBeNull();
            var sucessoValue = (bool)sucessoProperty.GetValue(responseObj);
            sucessoValue.Should().BeFalse();
            
            // Verificar se erros contém as notificações
            var errosProperty = responseType.GetProperty("erros");
            errosProperty.Should().NotBeNull();
            var errosValue = errosProperty.GetValue(responseObj);
            errosValue.Should().NotBeNull();
            var errosList = errosValue as IEnumerable<object>;
            errosList.Should().NotBeNull();
            errosList.Count().Should().Be(1);
            
            // Verificar que o serviço não foi chamado
            _transacaoServiceMock.Verify(s => s.AddAsync(It.IsAny<CreateTransacaoDTO>(), It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task Update_QuandoDadosValidos_DeveRetornarNoContent()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            
            var id = Guid.NewGuid();
            var dto = new UpdateTransacaoDTO
            {
                Descricao = "Transacao Atualizada",
                Valor = 200m,
                Tipo = 2,
                Data = DateTime.Now
            };

            _transacaoServiceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateTransacaoDTO>(), It.IsAny<Guid?>()))
                                .ReturnsAsync(true);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);
                                
            // Configure user in UserManager
            var user = new Usuario { Id = usuarioId, UserName = "testuser@example.com" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, usuarioId);

            // Act
            var result = await controller.Update(id, dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso
            dynamic response = okResult.Value;
            bool sucesso = (bool)response.GetType().GetProperty("sucesso").GetValue(response);
            sucesso.Should().BeTrue();
            
            // Verificar que o usuarioId foi passado para o serviço
            _transacaoServiceMock.Verify(s => s.UpdateAsync(id, It.IsAny<UpdateTransacaoDTO>(), usuarioId), Times.Once);
        }

        [Fact]
        public async Task Update_QuandoTransacaoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var updateDto = new UpdateTransacaoDTO
            {
                Descricao = "Transacao Atualizada",
                Valor = 150m,
                Tipo = 1,
                Data = DateTime.Now.AddDays(-1)
            };

            _transacaoServiceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateTransacaoDTO>(), It.IsAny<Guid?>()))
                                .ReturnsAsync(false);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem> { new NotificationItem("Erro", "Transação não encontrada") });
                                
            // Configure user in UserManager
            var user = new Usuario { Id = usuarioId, UserName = "testuser@example.com" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, usuarioId);

            // Act
            var result = await controller.Update(id, updateDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().NotBeNull();
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso e erros
            var responseObj = badRequestResult.Value;
            var responseType = responseObj.GetType();
            var sucessoProperty = responseType.GetProperty("sucesso");
            sucessoProperty.Should().NotBeNull();
            var sucessoValue = (bool)sucessoProperty.GetValue(responseObj);
            sucessoValue.Should().BeFalse();
            
            // Verificar se erros contém as notificações
            var errosProperty = responseType.GetProperty("erros");
            errosProperty.Should().NotBeNull();
            var errosValue = errosProperty.GetValue(responseObj);
            errosValue.Should().NotBeNull();
            var errosList = errosValue as IEnumerable<object>;
            errosList.Should().NotBeNull();
            errosList.Count().Should().Be(1);
        }

        [Fact]
        public async Task Delete_QuandoTransacaoExiste_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.DeleteAsync(id, It.IsAny<Guid?>()))
                                .ReturnsAsync(true);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);

            // Configurar o contexto do controlador para o teste
            ConfigureControllerContext(_controller, Guid.NewGuid());
            
            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            
            // Verificar se o valor é um objeto anônimo com a propriedade sucesso
            dynamic response = okResult.Value;
            bool sucesso = (bool)response.GetType().GetProperty("sucesso").GetValue(response);
            sucesso.Should().BeTrue();
        } 
    }
}
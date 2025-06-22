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
            okResult.Value.Should().BeAssignableTo<IEnumerable<TransacaoDTO>>();
            ((IEnumerable<TransacaoDTO>)okResult.Value).Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_QuandoOcorreErro_DeveRetornarBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _transacaoServiceMock.Setup(s => s.GetAllAsync(It.IsAny<Guid?>()))
                                .ReturnsAsync((IEnumerable<TransacaoDTO>)null);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem> { new NotificationItem("Erro", "Erro ao buscar transações") });

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetAll();

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<List<NotificationItem>>();
            ((List<NotificationItem>)badRequestResult.Value).Should().HaveCount(1);
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
            okResult.Value.Should().BeAssignableTo<TransacaoDTO>();
            ((TransacaoDTO)okResult.Value).Id.Should().Be(transacaoId);
            ((TransacaoDTO)okResult.Value).Descricao.Should().Be("Transacao Teste");
            ((TransacaoDTO)okResult.Value).Valor.Should().Be(150m);
        }

        [Fact]
        public async Task GetById_QuandoTransacaoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var transacaoId = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.GetByIdAsync(transacaoId, It.IsAny<Guid?>()))
                                .ReturnsAsync((TransacaoDTO?)null);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem> { new NotificationItem("Erro", "Transação não encontrada") });

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetById(transacaoId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<List<NotificationItem>>();
            ((List<NotificationItem>)badRequestResult.Value).Should().HaveCount(1);
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
            okResult.Value.Should().BeAssignableTo<IEnumerable<TransacaoDTO>>();
            ((IEnumerable<TransacaoDTO>)okResult.Value).Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByPeriodo_QuandoOcorreErro_DeveRetornarBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dataInicio = DateTime.Now;
            var dataFim = DateTime.Now.AddDays(-1); // Data inválida (fim antes do início)

            _transacaoServiceMock.Setup(s => s.GetByPeriodoAsync(dataInicio, dataFim, It.IsAny<Guid?>()))
                .ReturnsAsync((IEnumerable<TransacaoDTO>?)null);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem> { new NotificationItem("Erro", "A data inicial não pode ser maior que a data final") });

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetByPeriodo(dataInicio, dataFim);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<List<NotificationItem>>();
            ((List<NotificationItem>)badRequestResult.Value).Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByTipo_QuandoHaTransacoes_DeveRetornarOkComLista()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tipo = 1; // Receita
            var transacoes = new List<TransacaoDTO>
            {
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Receita 1", Valor = 100m, Tipo = 1 },
                new TransacaoDTO { Id = Guid.NewGuid(), Descricao = "Receita 2", Valor = 200m, Tipo = 1 }
            };

            _transacaoServiceMock.Setup(s => s.GetByTipoAsync(tipo, It.IsAny<Guid?>()))
                                .ReturnsAsync(transacoes);

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetByTipo(tipo);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeAssignableTo<IEnumerable<TransacaoDTO>>();
            ((IEnumerable<TransacaoDTO>)okResult.Value).Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByTipo_QuandoTipoInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tipoInvalido = 999;

            _transacaoServiceMock.Setup(s => s.GetByTipoAsync(tipoInvalido, It.IsAny<Guid?>()))
                                .ReturnsAsync((IEnumerable<TransacaoDTO>?)null);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem> { new NotificationItem("Erro", "Tipo de transação inválido") });

            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, userId);

            // Act
            var result = await controller.GetByTipo(tipoInvalido);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<List<NotificationItem>>();
            ((List<NotificationItem>)badRequestResult.Value).Should().HaveCount(1);
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
            createdAtActionResult.RouteValues["id"].Should().Be(transacaoId);
            
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
                
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object);
            ConfigureControllerContext(controller, usuarioId);
            
            // Simular ModelState inválido
            controller.ModelState.AddModelError("Descricao", "O campo Descrição é obrigatório");

            // Act
            var result = await controller.Create(dto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<List<NotificationItem>>();
            ((List<NotificationItem>)badRequestResult.Value).Should().HaveCount(1);
            
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
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            
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
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeAssignableTo<List<NotificationItem>>();
            ((List<NotificationItem>)notFoundResult.Value).Should().HaveCount(1);
        }

        [Fact]
        public async Task Update_QuandoDadosInvalidos_DeveRetornarBadRequest()
        {
            // Arrange
            // Configurar usuário autenticado
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
            
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object)
            {
                ControllerContext = controllerContext
            };
            
            var id = Guid.NewGuid();
            var dto = new UpdateTransacaoDTO
            {
                // Dados inválidos - faltando campos obrigatórios
            };

            // Simular ModelState inválido
            controller.ModelState.AddModelError("Descricao", "O campo Descrição é obrigatório");

            // Act
            var result = await controller.Update(id, dto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeAssignableTo<List<NotificationItem>>();
            ((List<NotificationItem>)badRequestResult.Value).Should().HaveCount(1);
            
            // Verificar que o serviço não foi chamado
            _transacaoServiceMock.Verify(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateTransacaoDTO>(), It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task Delete_QuandoTransacaoExiste_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.DeleteAsync(id, It.IsAny<Guid?>()))
                                .ReturnsAsync(true);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        }

        [Fact]
        public async Task Delete_QuandoTransacaoNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _transacaoServiceMock.Setup(s => s.DeleteAsync(id, It.IsAny<Guid?>()))
                                .ReturnsAsync(false);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(true);
            _notificationServiceMock.Setup(n => n.Notifications).Returns(new List<NotificationItem> { new NotificationItem("Erro", "Transação não encontrada") });

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeAssignableTo<List<NotificationItem>>();
            ((List<NotificationItem>)notFoundResult.Value).Should().HaveCount(1);
        }

        [Fact]
        public void ObterUsuarioIdLogado_QuandoUsuarioAutenticado_DeveRetornarUsuarioId()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Name, "usuario@teste.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            
            // Configurando o HttpContext com o usuário autenticado
            var httpContext = new DefaultHttpContext()
            {
                User = principal
            };
            
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object)
            {
                ControllerContext = controllerContext
            };

            // Act
            var method = typeof(TransacoesController).GetMethod("ObterUsuarioIdLogado", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(controller, null) as Guid?;

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().Be(usuarioId);
        }
        
        [Fact]
        public void ObterUsuarioIdLogado_QuandoUsuarioNaoAutenticado_DeveRetornarNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext(); // Usuário não autenticado
            
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object)
            {
                ControllerContext = controllerContext
            };

            // Act
            var method = typeof(TransacoesController).GetMethod("ObterUsuarioIdLogado", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            method.Should().NotBeNull("ObterUsuarioIdLogado method should exist");
            
            var result = method?.Invoke(controller, null) as Guid?;

            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task Create_QuandoUsuarioAutenticado_DevePassarUsuarioIdParaServico()
        {
            // Arrange
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
            
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object)
            {
                ControllerContext = controllerContext
            };
            
            var dto = new CreateTransacaoDTO
            {
                Tipo = 1,
                Data = DateTime.Now,
                Descricao = "Teste com usuário",
                Valor = 100m
            };
            
            var transacaoId = Guid.NewGuid();
            _transacaoServiceMock.Setup(s => s.AddAsync(It.IsAny<CreateTransacaoDTO>(), It.IsAny<Guid?>()))
                                .ReturnsAsync(transacaoId);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);

            // Act
            await controller.Create(dto);

            // Assert
            _transacaoServiceMock.Verify(s => s.AddAsync(dto, usuarioId), Times.Once);
        }
        
        [Fact]
        public async Task Update_QuandoUsuarioAutenticado_DevePassarUsuarioIdParaServico()
        {
            // Arrange
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
            
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            
            var controller = new TransacoesController(_transacaoServiceMock.Object, _userManagerMock.Object, _notificationServiceMock.Object)
            {
                ControllerContext = controllerContext
            };
            
            var id = Guid.NewGuid();
            var dto = new UpdateTransacaoDTO
            {
                Tipo = 1,
                Data = DateTime.Now,
                Descricao = "Teste com usuário",
                Valor = 100m
            };
            
            _transacaoServiceMock.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateTransacaoDTO>(), It.IsAny<Guid?>()))
                                .ReturnsAsync(true);
            _notificationServiceMock.Setup(n => n.HasNotifications).Returns(false);

            // Act
            await controller.Update(id, dto);

            // Assert
            _transacaoServiceMock.Verify(s => s.UpdateAsync(id, dto, usuarioId), Times.Once);
        }
    }
}
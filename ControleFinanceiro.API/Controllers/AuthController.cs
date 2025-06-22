using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.DTOs.Auth;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Constants;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(
            IAuthService authService, 
            IEmailService emailService, 
            INotificationService notificationService,
            UserManager<Usuario> userManager = null) : base(notificationService, userManager)
        {
            _authService = authService;
            _emailService = emailService;
        }

        /// <summary>
        /// Endpoint para login de usuário
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ValidarModelState())
            {
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }

            var (token, usuario) = await _authService.AuthenticateAsync(model.Username, model.Password);

            if (token == null)
            {
                if (!_notificationService.HasNotifications)
                {
                    _notificationService.AddNotification(ChavesNotificacao.Autenticacao, MensagensErro.CredenciaisInvalidas);
                }
                return RespostaPersonalizada(null, StatusCodes.Status401Unauthorized);
            }

            var userDto = new UserDTO
            {
                Id = usuario.Id,
                Username = usuario.UserName,
                Email = usuario.Email,
                Role = usuario.Role,
                DataInclusao = usuario.DataInclusao,
                DataAlteracao = usuario.DataAlteracao
            };

            return RespostaPersonalizada(new { token, user = userDto });
        }

        /// <summary>
        /// Endpoint para registro de novo usuário
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ValidarModelState())
            {
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }

            if (model.Password != model.ConfirmPassword)
            {
                _notificationService.AddNotification(ChavesNotificacao.Senha, MensagensErro.SenhasNaoConferem);
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }

            var usuario = await _authService.RegisterAsync(model.Username, model.Email, model.Password);

            if (usuario == null)
            {
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }

            var userDto = new UserDTO
            {
                Id = usuario.Id,
                Username = usuario.UserName,
                Email = usuario.Email,
                Role = usuario.Role,
                DataInclusao = usuario.DataInclusao,
                DataAlteracao = usuario.DataAlteracao
            };

            return RespostaPersonalizada(new { mensagem = "Usuário registrado com sucesso", usuario = userDto });
        }

        /// <summary>
        /// Endpoint para solicitar redefinição de senha
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (!ValidarModelState())
            {
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }

            var (usuario, token) = await _authService.SolicitarResetSenhaAsync(model.Email);

            if (_notificationService.HasNotifications)
            {
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }

            if (usuario != null && !string.IsNullOrEmpty(token))
            {
                var emailEnviado = await _emailService.EnviarEmailResetSenhaAsync(model.Email, token, usuario.UserName);
                
                if (!emailEnviado)
                {
                    _notificationService.AddNotification(ChavesNotificacao.Email, MensagensErro.ErroEnvioEmail);
                    return RespostaPersonalizada(new { mensagem = "Se o email existir em nossa base de dados, você receberá instruções para redefinição de senha." });
                }
            }

            return RespostaPersonalizada(new { mensagem = "Instruções para redefinição de senha foram enviadas para seu email" });
        }

        /// <summary>
        /// Endpoint para redefinir senha
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (!ValidarModelState())
            {
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }

            if (model.Password != model.ConfirmPassword)
            {
                _notificationService.AddNotification(ChavesNotificacao.Senha, MensagensErro.SenhasNaoConferem);
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }

            var sucesso = await _authService.ResetSenhaAsync(model.Token, model.Password);

            if (!sucesso || _notificationService.HasNotifications)
            {
                return RespostaPersonalizada(null, StatusCodes.Status400BadRequest);
            }
        
            return RespostaPersonalizada(new { mensagem = "Senha redefinida com sucesso" });
        }
    }
}

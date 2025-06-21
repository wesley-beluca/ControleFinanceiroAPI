using ControleFinanceiro.Application.DTOs.Auth;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (token, usuario) = await _authService.AuthenticateAsync(model.Username, model.Password);

            if (token == null)
                return Unauthorized(new { message = "Nome de usuário ou senha inválidos" });

            var userDto = new UserDTO
            {
                Id = usuario.Id,
                Username = usuario.Username,
                Email = usuario.Email,
                Role = usuario.Role,
                DataInclusao = usuario.DataInclusao,
                DataAlteracao = usuario.DataAlteracao
            };

            return Ok(new { token, user = userDto });
        }

        /// <summary>
        /// Endpoint para registro de novo usuário
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { message = "As senhas não conferem" });

            var (sucesso, mensagem, usuario) = await _authService.RegisterAsync(model.Username, model.Email, model.Password);

            if (!sucesso)
                return BadRequest(new { message = mensagem });

            var userDto = new UserDTO
            {
                Id = usuario.Id,
                Username = usuario.Username,
                Email = usuario.Email,
                Role = usuario.Role,
                DataInclusao = usuario.DataInclusao,
                DataAlteracao = usuario.DataAlteracao
            };

            return Ok(new { message = mensagem, user = userDto });
        }

        /// <summary>
        /// Endpoint para solicitar redefinição de senha
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (sucesso, mensagem, usuario, token) = await _authService.SolicitarResetSenhaAsync(model.Email);

            if (!sucesso)
                return BadRequest(new { message = mensagem });

            if (usuario != null && !string.IsNullOrEmpty(token))
            {
                var emailEnviado = await _emailService.EnviarEmailResetSenhaAsync(model.Email, token, usuario.Username);
                
                if (!emailEnviado)
                {
                    return Ok(new { message = "Se o email existir em nossa base de dados, você receberá instruções para redefinição de senha." });
                }
            }

            return Ok(new { message = "Instruções para redefinição de senha foram enviadas para seu email" });
        }

        /// <summary>
        /// Endpoint para redefinir senha
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Password != model.ConfirmPassword)
                return BadRequest(new { message = "As senhas não conferem" });

            var (sucesso, mensagem) = await _authService.ResetSenhaAsync(model.Token, model.Password);

            if (!sucesso)
                return BadRequest(new { message = mensagem });

            return Ok(new { message = mensagem });
        }


    }
}

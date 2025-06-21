using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Usuario> _userManager;
        private readonly INotificationService _notificationService;

        public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration, UserManager<Usuario> userManager, INotificationService notificationService)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<(string token, Usuario usuario)> AuthenticateAsync(string username, string password)
        {
            var usuario = await _usuarioRepository.ObterPorUsernameAsync(username);

            if (usuario == null || !usuario.VerificarSenha(password))
                return (null, null);
                
            var token = GerarJwtToken(usuario);
            return (token, usuario);
        }

        public async Task<(bool sucesso, string mensagem, Usuario usuario)> RegisterAsync(string username, string email, string password)
        {
            // Limpar notificações anteriores
            _notificationService.Clear();
            
            // Verificar se o username já existe
            if (await _usuarioRepository.ExisteUsernameAsync(username))
            {
                _notificationService.AddNotification("Username", "Nome de usuário já está em uso");
            }

            // Verificar se o email já existe
            if (await _usuarioRepository.ExisteEmailAsync(email))
            {
                _notificationService.AddNotification("Email", "Email já está em uso");
            }
            
            // Se já existem notificações, retorna erro
            if (_notificationService.HasNotifications)
            {
                return (false, string.Join(", ", _notificationService.Notifications.Select(n => n.Message)), null);
            }
            
            try
            {
                // Criar uma notificação para coletar erros de validação
                var notification = new Notification();
                var novoUsuario = new Usuario(username, email, password, "User", notification);
                
                // Se houver erros de validação, adiciona ao serviço de notificação
                if (!notification.IsValid)
                {
                    _notificationService.AddNotifications(notification);
                    return (false, string.Join(", ", _notificationService.Notifications.Select(n => n.Message)), null);
                }
                
                await _usuarioRepository.AdicionarAsync(novoUsuario);
                return (true, "Usuário registrado com sucesso", novoUsuario);
            }
            catch (ArgumentException ex)
            {
                _notificationService.AddNotification("Erro", ex.Message);
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool sucesso, string mensagem, Usuario usuario, string token)> SolicitarResetSenhaAsync(string email)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return (false, "Email não encontrado", null, null);

            var token = usuario.GerarTokenResetSenha();
            usuario.ResetPasswordToken = token;
            usuario.ResetPasswordTokenExpiration = DateTime.Now.AddHours(2);
            await _usuarioRepository.AtualizarAsync(usuario);

            return (true, "Token de redefinição de senha gerado com sucesso", usuario, token);
        }

        public async Task<(bool sucesso, string mensagem)> ResetSenhaAsync(string token, string novaSenha)
        {
            // Limpar notificações anteriores
            _notificationService.Clear();
            
            // Primeiro tentamos obter o usuário pelo token armazenado na entidade
            var usuario = await _usuarioRepository.ObterPorResetTokenAsync(token);
            if (usuario == null)
            {
                _notificationService.AddNotification("Token", "Token inválido");
                return (false, "Token inválido");
            }

            // Verificamos se o token está expirado
            if (usuario.ResetPasswordTokenExpiration < DateTime.Now)
            {
                _notificationService.AddNotification("Token", "Token expirado");
                return (false, "Token expirado");
            }

            // Validar a nova senha usando o Notification Pattern
            var notification = new Notification();
            var senhaRedefinida = usuario.RedefinirSenha(token, novaSenha);
            
            if (!senhaRedefinida)
            {
                _notificationService.AddNotification("Token", "Token expirado ou inválido");
                return (false, "Token expirado ou inválido");
            }

            await _usuarioRepository.AtualizarAsync(usuario);
            return (true, "Senha redefinida com sucesso");
        }



        private string GerarJwtToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.UserName),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

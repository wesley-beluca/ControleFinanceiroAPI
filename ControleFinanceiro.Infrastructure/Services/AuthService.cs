using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
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
            // Verificar se o username já existe
            if (await _usuarioRepository.ExisteUsernameAsync(username))
                return (false, "Nome de usuário já está em uso", null);

            // Verificar se o email já existe
            if (await _usuarioRepository.ExisteEmailAsync(email))
                return (false, "Email já está em uso", null);

            try
            {
                var novoUsuario = new Usuario(username, email, password);
                await _usuarioRepository.AdicionarAsync(novoUsuario);
                return (true, "Usuário registrado com sucesso", novoUsuario);
            }
            catch (ArgumentException ex)
            {
                return (false, ex.Message, null);
            }
            catch (Exception)
            {
                return (false, "Erro ao registrar usuário", null);
            }
        }

        public async Task<(bool sucesso, string mensagem, Usuario usuario, string token)> SolicitarResetSenhaAsync(string email)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario == null)
                return (false, "Email não encontrado", null, null);

            // Gerar token para reset de senha
            var token = usuario.GerarTokenResetSenha();
            await _usuarioRepository.AtualizarAsync(usuario);

            return (true, "Token de redefinição de senha gerado com sucesso", usuario, token);
        }

        public async Task<(bool sucesso, string mensagem)> ResetSenhaAsync(string token, string novaSenha)
        {
            var usuario = await _usuarioRepository.ObterPorResetTokenAsync(token);
            if (usuario == null)
                return (false, "Token inválido");

            if (!usuario.RedefinirSenha(token, novaSenha))
                return (false, "Token expirado ou inválido");

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
                new Claim(ClaimTypes.Name, usuario.Username),
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

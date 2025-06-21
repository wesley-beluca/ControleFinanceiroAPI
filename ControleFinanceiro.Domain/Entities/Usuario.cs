using System;
using System.Security.Cryptography;
using System.Text;

namespace ControleFinanceiro.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um usuário do sistema
    /// </summary>
    public class Usuario : Entity
    {
        public const int USERNAME_MIN_LENGTH = 3;
        public const int USERNAME_MAX_LENGTH = 50;
        public const int PASSWORD_MIN_LENGTH = 6;
        public const int EMAIL_MAX_LENGTH = 100;

        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string Role { get; private set; }
        public string ResetPasswordToken { get; private set; }
        public DateTime? ResetPasswordTokenExpiration { get; private set; }

        // Construtor protegido para EF Core
        protected Usuario() : base() { }

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        public Usuario(string username, string email, string password, string role = "User") : base()
        {
            ValidarUsername(username);
            ValidarEmail(email);
            ValidarPassword(password);

            Username = username;
            Email = email;
            PasswordHash = GerarHash(password);
            Role = role;
        }

        /// <summary>
        /// Atualiza os dados do usuário
        /// </summary>
        public void AtualizarDados(string username, string email)
        {
            ValidarUsername(username);
            ValidarEmail(email);

            Username = username;
            Email = email;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Altera a senha do usuário
        /// </summary>
        public void AlterarSenha(string senhaAtual, string novaSenha)
        {
            if (!VerificarSenha(senhaAtual))
                throw new ArgumentException("Senha atual incorreta");

            ValidarPassword(novaSenha);
            PasswordHash = GerarHash(novaSenha);
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Verifica se a senha fornecida corresponde à senha do usuário
        /// </summary>
        public bool VerificarSenha(string password)
        {
            return PasswordHash == GerarHash(password);
        }

        /// <summary>
        /// Gera um token para redefinição de senha
        /// </summary>
        /// <returns>O token gerado</returns>
        public string GerarTokenResetSenha()
        {
            ResetPasswordToken = Guid.NewGuid().ToString("N");
            ResetPasswordTokenExpiration = DateTime.Now.AddHours(2);
            AtualizarDataModificacao();
            return ResetPasswordToken;
        }

        /// <summary>
        /// Redefine a senha usando um token válido
        /// </summary>
        public bool RedefinirSenha(string token, string novaSenha)
        {
            if (string.IsNullOrEmpty(token) || 
                token != ResetPasswordToken || 
                !ResetPasswordTokenExpiration.HasValue || 
                ResetPasswordTokenExpiration.Value < DateTime.Now)
            {
                return false;
            }

            ValidarPassword(novaSenha);
            PasswordHash = GerarHash(novaSenha);
            ResetPasswordToken = null;
            ResetPasswordTokenExpiration = null;
            AtualizarDataModificacao();
            return true;
        }

        /// <summary>
        /// Altera o perfil/role do usuário
        /// </summary>
        public void AlterarRole(string novaRole)
        {
            if (string.IsNullOrWhiteSpace(novaRole))
                throw new ArgumentException("Role não pode ser vazia");

            Role = novaRole;
            AtualizarDataModificacao();
        }

        #region Validações

        private void ValidarUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Nome de usuário não pode ser vazio");

            if (username.Length < USERNAME_MIN_LENGTH || username.Length > USERNAME_MAX_LENGTH)
                throw new ArgumentException($"Nome de usuário deve ter entre {USERNAME_MIN_LENGTH} e {USERNAME_MAX_LENGTH} caracteres");
        }

        private void ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser vazio");

            if (email.Length > EMAIL_MAX_LENGTH)
                throw new ArgumentException($"Email não pode ter mais de {EMAIL_MAX_LENGTH} caracteres");

            // Validação básica de formato de email
            if (!email.Contains("@") || !email.Contains("."))
                throw new ArgumentException("Email inválido");
        }

        private void ValidarPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Senha não pode ser vazia");

            if (password.Length < PASSWORD_MIN_LENGTH)
                throw new ArgumentException($"Senha deve ter pelo menos {PASSWORD_MIN_LENGTH} caracteres");
        }

        #endregion

        #region Helpers

        private string GerarHash(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        #endregion
    }
}

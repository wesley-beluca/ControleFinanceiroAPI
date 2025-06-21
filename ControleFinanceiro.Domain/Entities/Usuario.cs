using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;
using System.Text;
using ControleFinanceiro.Domain.Notifications;

namespace ControleFinanceiro.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um usuário do sistema
    /// </summary>
    public class Usuario : IdentityUser<Guid>
    {
        public const int USERNAME_MIN_LENGTH = 3;
        public const int USERNAME_MAX_LENGTH = 50;
        public const int PASSWORD_MIN_LENGTH = 6;
        public const int EMAIL_MAX_LENGTH = 100;

        public string Role { get; set; }
        
        public DateTime DataInclusao { get; set; }
        public DateTime? DataAlteracao { get; set; }
        public bool Excluido { get; set; }
        public string ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiration { get; set; }

        public Usuario() { }

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        /// <param name="notification">Objeto de notificação para validação</param>
        public Usuario(string username, string email, string password, string role = "User", Notification notification = null)
        {
            var notificationResult = new Notification();
            
            notificationResult.AddNotifications(ValidarUsername(username));
            notificationResult.AddNotifications(ValidarEmail(email));
            notificationResult.AddNotifications(ValidarPassword(password));
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação, lança exceção apenas se não foi passado um objeto de notificação
            if (!notificationResult.IsValid && notification == null)
                throw new ArgumentException(notificationResult.GetErrorMessages());

            UserName = username;
            Email = email;
            PasswordHash = GerarHash(password);
            Role = role;
            Id = Guid.NewGuid();
            DataInclusao = DateTime.Now;
            Excluido = false;
        }

        /// <summary>
        /// Atualiza os dados do usuário
        /// </summary>
        /// <param name="notification">Objeto de notificação para validação</param>
        /// <returns>True se a atualização foi bem-sucedida, False caso contrário</returns>
        public bool AtualizarDados(string username, string email, Notification notification = null)
        {
            var notificationResult = new Notification();
            
            notificationResult.AddNotifications(ValidarUsername(username));
            notificationResult.AddNotifications(ValidarEmail(email));
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação
            if (!notificationResult.IsValid)
            {
                // Se não foi passado um objeto de notificação, lança exceção
                if (notification == null)
                    throw new ArgumentException(notificationResult.GetErrorMessages());
                    
                return false;
            }

            UserName = username;
            Email = email;
            DataAlteracao = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Altera a senha do usuário
        /// </summary>
        /// <param name="notification">Objeto de notificação para validação</param>
        /// <returns>True se a alteração foi bem-sucedida, False caso contrário</returns>
        public bool AlterarSenha(string senhaAtual, string novaSenha, Notification notification = null)
        {
            var notificationResult = new Notification();
            
            if (!VerificarSenha(senhaAtual))
                notificationResult.AddNotification("SenhaAtual", "Senha atual incorreta");
            
            notificationResult.AddNotifications(ValidarPassword(novaSenha));
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação
            if (!notificationResult.IsValid)
            {
                // Se não foi passado um objeto de notificação, lança exceção
                if (notification == null)
                    throw new ArgumentException(notificationResult.GetErrorMessages());
                    
                return false;
            }

            PasswordHash = GerarHash(novaSenha);
            DataAlteracao = DateTime.Now;
            return true;
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
            DataAlteracao = DateTime.Now;
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
            DataAlteracao = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Altera o perfil/role do usuário
        /// </summary>
        /// <param name="notification">Objeto de notificação para validação</param>
        /// <returns>True se a alteração foi bem-sucedida, False caso contrário</returns>
        public bool AlterarRole(string novaRole, Notification notification = null)
        {
            var notificationResult = new Notification();
            
            if (string.IsNullOrWhiteSpace(novaRole))
                notificationResult.AddNotification("Role", "Role não pode ser vazia");
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação
            if (!notificationResult.IsValid)
            {
                // Se não foi passado um objeto de notificação, lança exceção
                if (notification == null)
                    throw new ArgumentException(notificationResult.GetErrorMessages());
                    
                return false;
            }

            Role = novaRole;
            DataAlteracao = DateTime.Now;
            return true;
        }

        #region Validações

        private Notification ValidarUsername(string username)
        {
            var notification = new Notification();
            
            if (string.IsNullOrWhiteSpace(username))
                notification.AddNotification("Username", "Nome de usuário não pode ser vazio");

            if (!string.IsNullOrWhiteSpace(username) && (username.Length < USERNAME_MIN_LENGTH || username.Length > USERNAME_MAX_LENGTH))
                notification.AddNotification("Username", $"Nome de usuário deve ter entre {USERNAME_MIN_LENGTH} e {USERNAME_MAX_LENGTH} caracteres");
                
            return notification;
        }

        private Notification ValidarEmail(string email)
        {
            var notification = new Notification();
            
            if (string.IsNullOrWhiteSpace(email))
                notification.AddNotification("Email", "Email não pode ser vazio");

            if (!string.IsNullOrWhiteSpace(email) && email.Length > EMAIL_MAX_LENGTH)
                notification.AddNotification("Email", $"Email não pode ter mais de {EMAIL_MAX_LENGTH} caracteres");

            // Validação básica de formato de email
            if (!string.IsNullOrWhiteSpace(email) && (!email.Contains("@") || !email.Contains(".")))
                notification.AddNotification("Email", "Email inválido");
                
            return notification;
        }

        private Notification ValidarPassword(string password)
        {
            var notification = new Notification();
            
            if (string.IsNullOrWhiteSpace(password))
                notification.AddNotification("Password", "Senha não pode ser vazia");

            if (!string.IsNullOrWhiteSpace(password) && password.Length < PASSWORD_MIN_LENGTH)
                notification.AddNotification("Password", $"Senha deve ter pelo menos {PASSWORD_MIN_LENGTH} caracteres");
                
            return notification;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Marca o usuário como excluído (soft delete)
        /// </summary>
        public void MarcarComoExcluido()
        {
            Excluido = true;
            DataAlteracao = DateTime.Now;
        }

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

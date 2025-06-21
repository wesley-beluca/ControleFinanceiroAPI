using System;
using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Envia um email
        /// </summary>
        /// <param name="destinatario">Email do destinatário</param>
        /// <param name="assunto">Assunto do email</param>
        /// <param name="corpo">Corpo do email (pode ser HTML)</param>
        /// <returns>True se o email foi enviado com sucesso, False caso contrário</returns>
        Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo);
        
        /// <summary>
        /// Envia um email de redefinição de senha
        /// </summary>
        /// <param name="destinatario">Email do destinatário</param>
        /// <param name="token">Token de redefinição de senha</param>
        /// <param name="username">Nome de usuário</param>
        /// <returns>True se o email foi enviado com sucesso, False caso contrário</returns>
        Task<bool> EnviarEmailResetSenhaAsync(string destinatario, string token, string username);
        
        /// <summary>
        /// Envia um email de notificação de saldo negativo
        /// </summary>
        /// <param name="destinatario">Email do destinatário</param>
        /// <param name="username">Nome de usuário</param>
        /// <param name="saldo">Saldo atual do usuário</param>
        /// <returns>True se o email foi enviado com sucesso, False caso contrário</returns>
        Task<bool> EnviarEmailSaldoNegativoAsync(string destinatario, string username, decimal saldo);
    }
}

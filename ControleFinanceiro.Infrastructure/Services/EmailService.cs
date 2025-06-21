using ControleFinanceiro.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _emailRemetente;
        private readonly string _nomeRemetente;
        private readonly string _baseUrl;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["Email:SmtpServer"];
            
            if (!int.TryParse(_configuration["Email:SmtpPort"], out int smtpPort))
            {
                throw new ArgumentException("SMTP Port is not configured or is invalid");
            }
            _smtpPort = smtpPort;
            
            _smtpUsername = _configuration["Email:SmtpUsername"];
            _smtpPassword = _configuration["Email:SmtpPassword"];
            
            if (string.IsNullOrEmpty(_smtpPassword))
            {
                throw new ArgumentException("SMTP Password is not configured. Please set it in user secrets (development) or environment variables (production)");
            }
            
            _emailRemetente = _configuration["Email:EmailRemetente"];
            _nomeRemetente = _configuration["Email:NomeRemetente"];
            _baseUrl = _configuration["BaseUrl"];
        }

        public virtual async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailRemetente, _nomeRemetente),
                    Subject = assunto,
                    Body = corpo,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(destinatario));

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.EnableSsl = true;

                    await client.SendMailAsync(message);
                }

                return true;
            }
            catch (Exception)
            {
                // Em produção, seria importante logar a exceção
                return false;
            }
        }

        public virtual async Task<bool> EnviarEmailResetSenhaAsync(string destinatario, string token, string username)
        {
            // URL para a página de redefinição de senha no frontend
            var resetUrl = $"{_baseUrl}/reset-password?token={token}";
            
            var assunto = "Redefinição de Senha - Controle Financeiro";
            
            var corpo = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #3b82f6; color: white; padding: 10px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .button {{ background-color: #3b82f6; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }}
                    .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Redefinição de Senha</h2>
                    </div>
                    <div class='content'>
                        <p>Olá <strong>{username}</strong>,</p>
                        <p>Recebemos uma solicitação para redefinir sua senha. Se você não fez esta solicitação, por favor ignore este email.</p>
                        <p>Para redefinir sua senha, clique no botão abaixo:</p>
                        <p style='text-align: center;'>
                            <a href='{resetUrl}' class='button'>Redefinir Senha</a>
                        </p>
                        <p>Este link expirará em 24 horas.</p>
                        <p>Atenciosamente,<br>Equipe de Controle Financeiro</p>
                    </div>
                    <div class='footer'>
                        <p>Este é um email automático. Por favor, não responda.</p>
                    </div>
                </div>
            </body>
            </html>";

            return await EnviarEmailAsync(destinatario, assunto, corpo);
        }
    }
}

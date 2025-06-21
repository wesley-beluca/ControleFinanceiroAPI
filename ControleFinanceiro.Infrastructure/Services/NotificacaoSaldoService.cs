using ControleFinanceiro.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Services
{
    /// <summary>
    /// Serviço para notificação de usuários com saldo negativo
    /// </summary>
    public class NotificacaoSaldoService : INotificacaoSaldoService
    {
        private readonly ISaldoService _saldoService;
        private readonly IEmailService _emailService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<NotificacaoSaldoService> _logger;

        public NotificacaoSaldoService(
            ISaldoService saldoService,
            IEmailService emailService,
            IUsuarioRepository usuarioRepository,
            ILogger<NotificacaoSaldoService> logger)
        {
            _saldoService = saldoService;
            _emailService = emailService;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        /// <summary>
        /// Verifica e notifica usuários com saldo negativo
        /// </summary>
        public async Task<int> NotificarUsuariosSaldoNegativoAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando verificação de saldos negativos");
                
                // Calcula o saldo de todos os usuários
                var saldosUsuarios = await _saldoService.CalcularSaldoTodosUsuariosAsync();
                
                // Filtra apenas os usuários com saldo negativo
                var usuariosComSaldoNegativo = saldosUsuarios
                    .Where(kv => kv.Value < 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                
                int usuariosNotificados = 0;
                
                // Notifica cada usuário com saldo negativo
                foreach (var usuarioSaldo in usuariosComSaldoNegativo)
                {
                    var usuarioId = usuarioSaldo.Key;
                    var saldo = usuarioSaldo.Value;
                    
                    // Obtém os dados do usuário
                    var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
                    
                    if (usuario != null)
                    {
                        // Envia o email de notificação
                        bool enviado = await _emailService.EnviarEmailSaldoNegativoAsync(
                            usuario.Email,
                            usuario.UserName,
                            saldo);
                            
                        if (enviado)
                        {
                            _logger.LogInformation("Notificação enviada para o usuário {Username} (ID: {UsuarioId}) com saldo {Saldo}", 
                                usuario.UserName, usuarioId, saldo);
                            usuariosNotificados++;
                        }
                        else
                        {
                            _logger.LogWarning("Falha ao enviar notificação para o usuário {Username} (ID: {UsuarioId})",
                                usuario.UserName, usuarioId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Usuário com ID {UsuarioId} não encontrado para notificação de saldo negativo",
                            usuarioId);
                    }
                }
                
                _logger.LogInformation("Verificação de saldos negativos concluída. {UsuariosNotificados} usuários notificados de um total de {UsuariosNegativos} com saldo negativo",
                    usuariosNotificados, usuariosComSaldoNegativo.Count);
                    
                return usuariosNotificados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar usuários com saldo negativo");
                throw;
            }
        }
    }
}

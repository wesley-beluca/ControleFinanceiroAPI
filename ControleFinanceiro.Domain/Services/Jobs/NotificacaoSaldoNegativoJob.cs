using ControleFinanceiro.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Services.Jobs
{
    /// <summary>
    /// Job para verificação e notificação de usuários com saldo negativo
    /// </summary>
    [DisallowConcurrentExecution]
    public class NotificacaoSaldoNegativoJob : IJob
    {
        private readonly INotificacaoSaldoService _notificacaoSaldoService;
        private readonly ILogger<NotificacaoSaldoNegativoJob> _logger;

        public NotificacaoSaldoNegativoJob(
            INotificacaoSaldoService notificacaoSaldoService,
            ILogger<NotificacaoSaldoNegativoJob> logger)
        {
            _notificacaoSaldoService = notificacaoSaldoService;
            _logger = logger;
        }

        /// <summary>
        /// Executa o job de verificação e notificação de saldos negativos
        /// </summary>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Iniciando job de verificação de saldos negativos: {DataHora}", DateTime.Now);
                
                // Executa a verificação e notificação de saldos negativos
                int usuariosNotificados = await _notificacaoSaldoService.NotificarUsuariosSaldoNegativoAsync();
                
                _logger.LogInformation("Job de verificação de saldos negativos concluído: {DataHora}. {UsuariosNotificados} usuários notificados", 
                    DateTime.Now, usuariosNotificados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar job de verificação de saldos negativos");
                // Não relançamos a exceção para não interromper o agendamento do job
            }
        }
    }
}

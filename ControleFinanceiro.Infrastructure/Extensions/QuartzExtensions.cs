using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Services.Jobs;
using ControleFinanceiro.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;

namespace ControleFinanceiro.Infrastructure.Extensions
{
    /// <summary>
    /// Extensões para configuração do Quartz.NET
    /// </summary>
    public static class QuartzExtensions
    {
        /// <summary>
        /// Adiciona e configura os serviços do Quartz.NET
        /// </summary>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfiguration configuration)
        {
            // Registra os serviços necessários
            services.AddScoped<ISaldoService, SaldoService>();
            services.AddScoped<INotificacaoSaldoService, NotificacaoSaldoService>();
            
            // Configura o Quartz
            services.AddQuartz(q =>
            {
                // Cria um JobKey para o job de notificação de saldo negativo
                var jobKey = new JobKey("NotificacaoSaldoNegativoJob");
                
                // Obtém a expressão cron da configuração ou usa um valor padrão
                string cronExpression = configuration["QuartzJobs:NotificacaoSaldoNegativo:CronExpression"] ?? "0 0 8 * * ?";
                
                // Verifica se o job está habilitado
                bool jobEnabled = true;
                if (bool.TryParse(configuration["QuartzJobs:NotificacaoSaldoNegativo:Enabled"], out bool enabled))
                {
                    jobEnabled = enabled;
                }
                
                // Se o job estiver habilitado, configura-o
                if (jobEnabled)
                {
                    // Registra o job
                    q.AddJob<NotificacaoSaldoNegativoJob>(opts => opts.WithIdentity(jobKey));
                    
                    // Configura o trigger com a expressão cron
                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("NotificacaoSaldoNegativoJob-trigger")
                        .WithCronSchedule(cronExpression)
                        .WithDescription($"Trigger para verificação diária de saldos negativos ({cronExpression})")
                    );
                }
            });
            
            // Adiciona o hosted service do Quartz
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            
            return services;
        }
    }
}

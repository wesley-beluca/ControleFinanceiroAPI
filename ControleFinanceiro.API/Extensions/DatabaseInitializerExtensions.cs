using ControleFinanceiro.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Extensions
{
    public static class DatabaseInitializerExtensions
    {
        public static async Task InitializeDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Iniciando a população do banco de dados com dados iniciais...");
                await SeedData.InitializeAsync(services, logger);
                logger.LogInformation("População do banco de dados concluída com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocorreu um erro ao inicializar o banco de dados.");
            }
        }
    }
}

using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, ILogger<object> logger)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Garante que o banco de dados está criado e atualizado
                await context.Database.MigrateAsync();

                // Verifica se já existem dados no banco
                if (await context.Usuarios.AnyAsync() || await context.Transacoes.AnyAsync())
                {
                    logger.LogInformation("O banco de dados já está populado. Pulando a inicialização de dados.");
                    return;
                }

                // Cria o usuário master
                var usuarioMaster = new Usuario("master", "admin@controle-financeiro.com", "senhamaster", "Admin");
                await context.Usuarios.AddAsync(usuarioMaster);
                await context.SaveChangesAsync();

                logger.LogInformation($"Usuário master criado com ID: {usuarioMaster.Id}");

                // Cria as transações iniciais
                var transacoes = new List<Transacao>
                {
                    new Transacao(TipoTransacao.Despesa, new DateTime(2022, 8, 29), "Cartão de Crédito", 825.82m, usuarioMaster.Id),
                    new Transacao(TipoTransacao.Despesa, new DateTime(2022, 8, 29), "Curso C#", 200.00m, usuarioMaster.Id),
                    new Transacao(TipoTransacao.Receita, new DateTime(2022, 8, 31), "Salário", 7000.00m, usuarioMaster.Id),
                    new Transacao(TipoTransacao.Despesa, new DateTime(2022, 9, 1), "Mercado", 3000.00m, usuarioMaster.Id),
                    new Transacao(TipoTransacao.Despesa, new DateTime(2022, 9, 1), "Farmácia", 300.00m, usuarioMaster.Id),
                    new Transacao(TipoTransacao.Despesa, new DateTime(2022, 9, 1), "Combustível", 800.25m, usuarioMaster.Id),
                    new Transacao(TipoTransacao.Despesa, new DateTime(2022, 9, 15), "Financiamento Carro", 900.00m, usuarioMaster.Id),
                    new Transacao(TipoTransacao.Despesa, new DateTime(2022, 9, 22), "Financiamento Casa", 1200.00m, usuarioMaster.Id),
                    new Transacao(TipoTransacao.Receita, new DateTime(2022, 9, 25), "Freelance Projeto XPTO", 2500.00m, usuarioMaster.Id)
                };

                await context.Transacoes.AddRangeAsync(transacoes);
                await context.SaveChangesAsync();

                logger.LogInformation($"Foram criadas {transacoes.Count} transações iniciais.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocorreu um erro ao inicializar o banco de dados com dados iniciais.");
                throw;
            }
        }
    }
}

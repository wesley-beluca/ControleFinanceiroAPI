using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using ControleFinanceiro.Infrastructure.Data;
using ControleFinanceiro.Infrastructure.Repositories;
using ControleFinanceiro.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFinanceiro.Infrastructure.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuração do banco de dados
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            // Repositórios
            services.AddScoped<ITransacaoRepository, TransacaoRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            
            // Serviços de Infraestrutura
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Serviços
            services.AddScoped<ITransacaoService, TransacaoService>();
            services.AddScoped<IResumoFinanceiroService, ResumoFinanceiroService>();

            // Validadores
            services.AddScoped<TransacaoDTOValidator>();
            services.AddScoped<CreateTransacaoDTOValidator>();
            services.AddScoped<UpdateTransacaoDTOValidator>();
            
            // Adicionar validadores para DTOs de autenticação se necessário

            return services;
        }
    }
}
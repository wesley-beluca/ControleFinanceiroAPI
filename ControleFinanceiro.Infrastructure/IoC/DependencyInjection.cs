using AutoMapper;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Mappings;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using ControleFinanceiro.Infrastructure.Data;
using ControleFinanceiro.Infrastructure.Extensions;
using ControleFinanceiro.Infrastructure.Repositories;
using ControleFinanceiro.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
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
                    
            // Configuração do Identity para suporte ao UserManager
            services.AddIdentityCore<Usuario>(options =>
            {
                // Configuração mínima para senhas
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppDbContext>();
            
            // Registrar o UserManager como serviço
            services.AddScoped<UserManager<Usuario>>();

            // Repositórios
            services.AddScoped<ITransacaoRepository, TransacaoRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            
            // Serviços de Infraestrutura
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISaldoService, SaldoService>();
            services.AddScoped<INotificacaoSaldoService, NotificacaoSaldoService>();
            
            // Configuração do Quartz para jobs agendados
            services.AddQuartzJobs(configuration);

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Configuração do AutoMapper
            services.AddAutoMapper(typeof(DomainToDTOMappingProfile), typeof(DTOToDomainMappingProfile));
            
            // Serviços
            services.AddScoped<ITransacaoService, TransacaoService>();
            services.AddScoped<IResumoFinanceiroService, ResumoFinanceiroService>();

            // Validadores
            services.AddScoped<TransacaoDTOValidator>();
            services.AddScoped<CreateTransacaoDTOValidator>();
            services.AddScoped<UpdateTransacaoDTOValidator>();


            return services;
        }
    }
}
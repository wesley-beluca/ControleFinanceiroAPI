using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Services;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using ControleFinanceiro.Infrastructure.Data;
using ControleFinanceiro.Infrastructure.IoC;
using ControleFinanceiro.Infrastructure.Repositories;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ControleFinanceiro.Infrastructure.Tests.IoC
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddInfrastructure_DeveRegistrarServicosCorretamente()
        {
            // Arrange
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"ConnectionStrings:DefaultConnection", "Data Source=:memory:;"}
                })
                .Build();

            // Act
            services.AddInfrastructure(configuration);
            
            // Remover o provider SqlServer para evitar conflito com InMemory
            var descriptor = services.FirstOrDefault(d => 
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) &&
                d.ImplementationInstance != null);
                
            if (descriptor != null)
            {
                services.Remove(descriptor);
                
                // Adicionar apenas o provedor InMemory
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            }
            
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<AppDbContext>().Should().NotBeNull();
            serviceProvider.GetService<ITransacaoRepository>().Should().NotBeNull();
        }

        [Fact]
        public void AddApplication_DeveRegistrarServicosCorretamente()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Precisamos registrar as dependências que o Application precisa
            services.AddDbContext<AppDbContext>(options => 
                options.UseInMemoryDatabase("TestDb_Application"));
            services.AddScoped<ITransacaoRepository, TransacaoRepository>();

            // Act
            services.AddApplication();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // Verificando serviços
            serviceProvider.GetService<ITransacaoService>().Should().NotBeNull();
            serviceProvider.GetService<IResumoFinanceiroService>().Should().NotBeNull();

            // Verificando validadores
            serviceProvider.GetService<TransacaoDTOValidator>().Should().NotBeNull();
            serviceProvider.GetService<CreateTransacaoDTOValidator>().Should().NotBeNull();
            serviceProvider.GetService<UpdateTransacaoDTOValidator>().Should().NotBeNull();
        }

        [Fact]
        public void AddInfrastructureEApplication_DeveConfigurarAplicacaoCompleta()
        {
            // Arrange
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"ConnectionStrings:DefaultConnection", "Data Source=:memory:;"}
                })
                .Build();

            // Act
            services.AddInfrastructure(configuration);
            services.AddApplication();
            
            // Remover o provider SqlServer para evitar conflito com InMemory
            var descriptor = services.FirstOrDefault(d => 
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) &&
                d.ImplementationInstance != null);
                
            if (descriptor != null)
            {
                services.Remove(descriptor);
                
                // Adicionar apenas o provedor InMemory
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            }
            
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<AppDbContext>().Should().NotBeNull();
            serviceProvider.GetService<ITransacaoRepository>().Should().NotBeNull();
            serviceProvider.GetService<ITransacaoService>().Should().NotBeNull();
            serviceProvider.GetService<IResumoFinanceiroService>().Should().NotBeNull();
        }
    }
} 
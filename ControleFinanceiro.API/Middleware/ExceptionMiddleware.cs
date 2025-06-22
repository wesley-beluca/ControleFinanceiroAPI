using ControleFinanceiro.Domain.Constants;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Middleware
{
    /// <summary>
    /// Middleware para tratamento global de exceções
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly INotificationService _notificationService;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            INotificationService notificationService)
        {
            _next = next;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            _notificationService.Clear();
            _notificationService.AddNotification(ChavesNotificacao.Erro, MensagensErro.ErroInterno);

            var response = new
            {
                sucesso = false,
                erros = new[] 
                { 
                    new 
                    { 
                        Key = ChavesNotificacao.Erro, 
                        Message = MensagensErro.ErroInterno 
                    } 
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}

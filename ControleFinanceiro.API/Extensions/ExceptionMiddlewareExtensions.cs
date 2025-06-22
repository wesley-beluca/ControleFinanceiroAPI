using ControleFinanceiro.API.Middleware;
using Microsoft.AspNetCore.Builder;

namespace ControleFinanceiro.API.Extensions
{
    /// <summary>
    /// Extensões para configuração do middleware de exceções
    /// </summary>
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// Configura o middleware global de tratamento de exceções
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder configurado</returns>
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}

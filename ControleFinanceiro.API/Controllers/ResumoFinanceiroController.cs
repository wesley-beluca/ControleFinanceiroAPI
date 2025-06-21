using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class ResumoFinanceiroController : ControllerBase
    {
        private readonly IResumoFinanceiroService _resumoFinanceiroService;
        private readonly UserManager<Usuario> _userManager;
        private readonly INotificationService _notificationService;

        public ResumoFinanceiroController(IResumoFinanceiroService resumoFinanceiroService, UserManager<Usuario> userManager, INotificationService notificationService)
        {
            _resumoFinanceiroService = resumoFinanceiroService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<ResumoFinanceiroDTO>>> GetResumoFinanceiro(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            // Obtém o ID do usuário logado
            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            
            var resultado = await _resumoFinanceiroService.GerarResumoFinanceiroAsync(dataInicio, dataFim, usuarioId);
            
            if (!resultado.Success && _notificationService.HasNotifications)
            {
                return BadRequest(new { 
                    message = "Ocorreram erros ao gerar o resumo financeiro", 
                    errors = _notificationService.Notifications.Select(n => n.Message).ToList() 
                });
            }
            
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém o ID do usuário logado usando o UserManager
        /// </summary>
        /// <returns>ID do usuário ou null se não estiver autenticado</returns>
        private async Task<Guid?> ObterUsuarioIdLogadoAsync()
        {
            // Verifica se o usuário está autenticado
            if (!User.Identity.IsAuthenticated)
                return null;
                
            // Obtém o usuário atual usando o UserManager
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                return null;
                
            return usuario.Id;
        }
    }
}
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Constants;
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

    public class ResumoFinanceiroController : BaseController
    {
        private readonly IResumoFinanceiroService _resumoFinanceiroService;

        public ResumoFinanceiroController(
            IResumoFinanceiroService resumoFinanceiroService, 
            UserManager<Usuario> userManager, 
            INotificationService notificationService) : base(notificationService, userManager)
        {
            _resumoFinanceiroService = resumoFinanceiroService;
        }

        [HttpGet]
        public async Task<ActionResult> GetResumoFinanceiro(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            // Valida as datas
            if (dataInicio > dataFim)
            {
                _notificationService.AddNotification(ChavesNotificacao.DataInicio, MensagensErro.DataInicioMaiorQueFinal);
                return RespostaPersonalizada();
            }
            
            // Obtém o ID do usuário logado
            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            
            var resumo = await _resumoFinanceiroService.GerarResumoFinanceiroAsync(dataInicio, dataFim, usuarioId);
            return RespostaPersonalizada(resumo);
        }
    }
}
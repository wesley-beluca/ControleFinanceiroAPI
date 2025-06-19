using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumoFinanceiroController : ControllerBase
    {
        private readonly IResumoFinanceiroService _resumoFinanceiroService;

        public ResumoFinanceiroController(IResumoFinanceiroService resumoFinanceiroService)
        {
            _resumoFinanceiroService = resumoFinanceiroService;
        }

        [HttpGet]
        public async Task<ActionResult<ResumoFinanceiroDTO>> GetResumoFinanceiro(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            if (dataInicio > dataFim)
                return BadRequest("A data de início deve ser anterior ou igual à data de fim");

            var resumo = await _resumoFinanceiroService.GerarResumoFinanceiroAsync(dataInicio, dataFim);
            return Ok(resumo);
        }
    }
} 
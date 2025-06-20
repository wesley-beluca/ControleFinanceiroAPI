using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Entities;
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
        public async Task<ActionResult<Result<ResumoFinanceiroDTO>>> GetResumoFinanceiro(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            var resultado = await _resumoFinanceiroService.GerarResumoFinanceiroAsync(dataInicio, dataFim);
            return Ok(resultado);
        }
    }
} 
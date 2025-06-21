using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransacoesController : ControllerBase
    {
        private readonly ITransacaoService _transacaoService;

        public TransacoesController(ITransacaoService transacaoService)
        {
            _transacaoService = transacaoService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<IEnumerable<TransacaoDTO>>>> GetAll()
        {
            var result = await _transacaoService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<TransacaoDTO>>> GetById(Guid id)
        {
            var result = await _transacaoService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("periodo")]
        public async Task<ActionResult<Result<IEnumerable<TransacaoDTO>>>> GetByPeriodo(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            var result = await _transacaoService.GetByPeriodoAsync(dataInicio, dataFim);
            return Ok(result);
        }

        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult<Result<IEnumerable<TransacaoDTO>>>> GetByTipo(int tipo)
        {
            var result = await _transacaoService.GetByTipoAsync(tipo);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<Guid>>> Create([FromBody] CreateTransacaoDTO transacaoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Obtém o ID do usuário logado
            Guid? usuarioId = ObterUsuarioIdLogado();

            var result = await _transacaoService.AddAsync(transacaoDto, usuarioId);
            return result.Success 
                ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result)
                : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<bool>>> Update(Guid id, [FromBody] UpdateTransacaoDTO transacaoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Obtém o ID do usuário logado
            Guid? usuarioId = ObterUsuarioIdLogado();

            var result = await _transacaoService.UpdateAsync(id, transacaoDto, usuarioId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<bool>>> Delete(Guid id)
        {
            var result = await _transacaoService.DeleteAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Obtém o ID do usuário logado a partir do token JWT
        /// </summary>
        /// <returns>ID do usuário ou null se não estiver autenticado</returns>
        private Guid? ObterUsuarioIdLogado()
        {
            // Verifica se o usuário está autenticado
            if (!User.Identity.IsAuthenticated)
                return null;

            // Obtém o claim com o ID do usuário
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                return null;

            // Converte o ID para Guid
            if (Guid.TryParse(userIdClaim.Value, out Guid userId))
                return userId;

            return null;
        }
    }
}
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransacoesController : ControllerBase
    {
        private readonly ITransacaoService _transacaoService;

        public TransacoesController(ITransacaoService transacaoService)
        {
            _transacaoService = transacaoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransacaoDTO>>> GetAll()
        {
            var transacoes = await _transacaoService.GetAllAsync();
            return Ok(transacoes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransacaoDTO>> GetById(Guid id)
        {
            var transacao = await _transacaoService.GetByIdAsync(id);
            if (transacao == null)
                return NotFound();

            return Ok(transacao);
        }

        [HttpGet("periodo")]
        public async Task<ActionResult<IEnumerable<TransacaoDTO>>> GetByPeriodo(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            var transacoes = await _transacaoService.GetByPeriodoAsync(dataInicio, dataFim);
            return Ok(transacoes);
        }

        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<TransacaoDTO>>> GetByTipo(string tipo)
        {
            try
            {
                var transacoes = await _transacaoService.GetByTipoAsync(tipo);
                return Ok(transacoes);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] TransacaoDTO transacaoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _transacaoService.AddAsync(transacaoDto);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TransacaoDTO transacaoDto)
        {
            if (id != transacaoDto.Id)
                return BadRequest("O ID da transação não corresponde ao ID da URL");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _transacaoService.UpdateAsync(transacaoDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _transacaoService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
} 
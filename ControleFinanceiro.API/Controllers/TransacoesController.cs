using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Entities;
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

            var result = await _transacaoService.AddAsync(transacaoDto);
            return result.Success 
                ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result)
                : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<bool>>> Update(Guid id, [FromBody] TransacaoDTO transacaoDto)
        {
            if (id != transacaoDto.Id)
                return BadRequest(new { Success = false, Message = "O ID da transação não corresponde ao ID da URL" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transacaoService.UpdateAsync(transacaoDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<bool>>> Delete(Guid id)
        {
            var result = await _transacaoService.DeleteAsync(id);
            return Ok(result);
        }
    }
} 
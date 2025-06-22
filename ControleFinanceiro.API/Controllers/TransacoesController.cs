using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Constants;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class TransacoesController : BaseController
    {
        private readonly ITransacaoService _transacaoService;

        public TransacoesController(
            ITransacaoService transacaoService, 
            UserManager<Usuario> userManager, 
            INotificationService notificationService) : base(notificationService, userManager)
        {
            _transacaoService = transacaoService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            
            var transacoes = await _transacaoService.GetAllAsync(usuarioId);
            return RespostaPersonalizada(transacoes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            
            var transacao = await _transacaoService.GetByIdAsync(id, usuarioId);
            return RespostaPersonalizada(transacao);
        }

        [HttpGet("periodo")]
        public async Task<ActionResult> GetByPeriodo(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            
            var transacoes = await _transacaoService.GetByPeriodoAsync(dataInicio, dataFim, usuarioId);
            return RespostaPersonalizada(transacoes);
        }

        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult> GetByTipo(int tipo)
        {
            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            
            var transacoes = await _transacaoService.GetByTipoAsync(tipo, usuarioId);
            return RespostaPersonalizada(transacoes);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateTransacaoDTO transacaoDto)
        {
            if (!ValidarModelState())
            {
                return RespostaPersonalizada();
            }

            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            var novaTransacaoId = await _transacaoService.AddAsync(transacaoDto, usuarioId);
            
            if (_notificationService.HasNotifications)
                return RespostaPersonalizada();
                
            return RespostaPersonalizadaCreated(nameof(GetById), new { id = novaTransacaoId }, novaTransacaoId);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateTransacaoDTO transacaoDto)
        {
            if (!ValidarModelState())
            {
                return RespostaPersonalizada();
            }

            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            var sucesso = await _transacaoService.UpdateAsync(id, transacaoDto, usuarioId);
            return RespostaPersonalizada(sucesso);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            Guid? usuarioId = await ObterUsuarioIdLogadoAsync();
            var sucesso = await _transacaoService.DeleteAsync(id, usuarioId);
            return RespostaPersonalizada(sucesso);
        }
    }
}
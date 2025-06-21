using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.Infrastructure.Services
{
    /// <summary>
    /// Serviço para cálculo de saldo de usuários
    /// </summary>
    public class SaldoService : ISaldoService
    {
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<SaldoService> _logger;

        public SaldoService(
            ITransacaoRepository transacaoRepository,
            IUsuarioRepository usuarioRepository,
            ILogger<SaldoService> logger)
        {
            _transacaoRepository = transacaoRepository;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        /// <summary>
        /// Calcula o saldo atual para um usuário específico
        /// </summary>
        public async Task<decimal> CalcularSaldoUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var todasTransacoes = await _transacaoRepository.GetAllAsync();
                var transacoesUsuario = todasTransacoes.Where(t => t.UsuarioId == usuarioId && !t.Excluido).ToList();

                decimal saldo = 0;
                foreach (var transacao in transacoesUsuario)
                {
                    if (transacao.Tipo == TipoTransacao.Receita)
                    {
                        saldo += transacao.Valor;
                    }
                    else
                    {
                        saldo -= transacao.Valor;
                    }
                }

                return saldo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular saldo do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Calcula o saldo para todos os usuários
        /// </summary>
        public async Task<Dictionary<Guid, decimal>> CalcularSaldoTodosUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuarioRepository.ObterTodosAsync();
                var resultado = new Dictionary<Guid, decimal>();

                var todasTransacoes = await _transacaoRepository.GetAllAsync();
                var transacoesValidas = todasTransacoes.Where(t => t.UsuarioId.HasValue && !t.Excluido).ToList();

                foreach (var usuario in usuarios)
                {
                    var transacoesUsuario = transacoesValidas.Where(t => t.UsuarioId == usuario.Id).ToList();

                    decimal saldo = 0;
                    foreach (var transacao in transacoesUsuario)
                    {
                        if (transacao.Tipo == TipoTransacao.Receita)
                        {
                            saldo += transacao.Valor;
                        }
                        else
                        {
                            saldo -= transacao.Valor;
                        }
                    }

                    resultado.Add(usuario.Id, saldo);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular saldo de todos os usuários");
                throw;
            }
        }
    }
}

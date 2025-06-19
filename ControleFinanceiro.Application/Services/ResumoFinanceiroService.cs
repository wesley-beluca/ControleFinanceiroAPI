using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Services
{
    public class ResumoFinanceiroService : IResumoFinanceiroService
    {
        private readonly ITransacaoRepository _transacaoRepository;

        public ResumoFinanceiroService(ITransacaoRepository transacaoRepository)
        {
            _transacaoRepository = transacaoRepository;
        }

        public async Task<ResumoFinanceiroDTO> GerarResumoFinanceiroAsync(DateTime dataInicio, DateTime dataFim)
        {
            // Buscar todas as transações no período
            var transacoesPeriodo = await _transacaoRepository.GetByPeriodoAsync(dataInicio, dataFim);
            
            // Calcular saldo anterior (todas as transações antes da data de início)
            var transacoesAnteriores = await _transacaoRepository.GetByPeriodoAsync(
                DateTime.MinValue, 
                dataInicio.AddDays(-1));

            decimal saldoAnterior = CalcularSaldo(transacoesAnteriores);
            
            // Calcular totais do período
            decimal totalReceitas = transacoesPeriodo
                .Where(t => t.Tipo == TipoTransacao.Receita)
                .Sum(t => t.Valor);
                
            decimal totalDespesas = transacoesPeriodo
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .Sum(t => t.Valor);
                
            decimal saldoFinal = saldoAnterior + totalReceitas - totalDespesas;
            
            // Agrupar transações por dia
            var transacoesPorDia = transacoesPeriodo
                .GroupBy(t => t.Data.Date)
                .OrderBy(g => g.Key)
                .Select(g => new TransacaoDiariaDTO
                {
                    Data = g.Key,
                    TotalReceitas = g.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor),
                    TotalDespesas = g.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor),
                    SaldoDiario = g.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor) - 
                                  g.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor),
                    Transacoes = g.Select(t => new TransacaoDTO
                    {
                        Id = t.Id,
                        Tipo = t.Tipo.ToString(),
                        Data = t.Data,
                        Descricao = t.Descricao,
                        Valor = t.Valor
                    }).ToList()
                })
                .ToList();
                
            return new ResumoFinanceiroDTO
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                SaldoAnterior = saldoAnterior,
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                SaldoFinal = saldoFinal,
                TransacoesDiarias = transacoesPorDia
            };
        }
        
        private decimal CalcularSaldo(IEnumerable<Transacao> transacoes)
        {
            decimal receitas = transacoes
                .Where(t => t.Tipo == TipoTransacao.Receita)
                .Sum(t => t.Valor);
                
            decimal despesas = transacoes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .Sum(t => t.Valor);
                
            return receitas - despesas;
        }
    }
} 
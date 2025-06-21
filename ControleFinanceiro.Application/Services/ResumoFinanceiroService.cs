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

        public async Task<Result<ResumoFinanceiroDTO>> GerarResumoFinanceiroAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null)
        {
            try
            {
                // Validação das datas
                if (dataInicio > dataFim)
                    return Result<ResumoFinanceiroDTO>.Fail("A data inicial não pode ser maior que a data final");

                // Limitar o período de consulta para evitar sobrecarga
                if ((dataFim - dataInicio).TotalDays > 366)
                    return Result<ResumoFinanceiroDTO>.Fail("O período de consulta não pode ser maior que 1 ano");

                // Buscar todas as transações no período (filtradas por usuário se especificado)
                var transacoesPeriodo = await _transacaoRepository.GetByPeriodoAsync(dataInicio, dataFim, usuarioId);
                
                // Filtrar transações excluídas (garantia extra, já que o repositório deve aplicar o filtro)
                transacoesPeriodo = transacoesPeriodo.Where(t => !t.Excluido).ToList();
                
                // Calcular saldo anterior (todas as transações antes da data de início)
                var transacoesAnteriores = await _transacaoRepository.GetByPeriodoAsync(
                    DateTime.MinValue, 
                    dataInicio.AddDays(-1),
                    usuarioId);
                
                // Filtrar transações excluídas do período anterior também
                transacoesAnteriores = transacoesAnteriores.Where(t => !t.Excluido).ToList();

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
                            Tipo = (int)t.Tipo,
                            Data = t.Data,
                            Descricao = t.Descricao,
                            Valor = t.Valor
                        }).ToList()
                    })
                    .ToList();
                    
                var resumoFinanceiro = new ResumoFinanceiroDTO
                {
                    DataInicio = dataInicio,
                    DataFim = dataFim,
                    SaldoAnterior = saldoAnterior,
                    TotalReceitas = totalReceitas,
                    TotalDespesas = totalDespesas,
                    SaldoFinal = saldoFinal,
                    TransacoesDiarias = transacoesPorDia
                };

                return Result<ResumoFinanceiroDTO>.Ok(resumoFinanceiro, "Resumo financeiro gerado com sucesso");
            }
            catch (Exception ex)
            {
                return Result<ResumoFinanceiroDTO>.Fail($"Erro ao gerar resumo financeiro: {ex.Message}");
            }
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
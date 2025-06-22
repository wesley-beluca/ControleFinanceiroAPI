using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain.Constants;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
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
        private readonly INotificationService _notificationService;

        public ResumoFinanceiroService(ITransacaoRepository transacaoRepository, INotificationService notificationService)
        {
            _transacaoRepository = transacaoRepository;
            _notificationService = notificationService;
        }

        public async Task<ResumoFinanceiroDTO> GerarResumoFinanceiroAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            try
            {
                // Validação das datas
                if (dataInicio > dataFim)
                {
                    _notificationService.AddNotification(ChavesNotificacao.DataInicio, MensagensErro.DataInicioMaiorQueFinal);
                    return null;
                }

                // Limitar o período de consulta para evitar sobrecarga
                if ((dataFim - dataInicio).TotalDays > 366)
                {
                    _notificationService.AddNotification(ChavesNotificacao.Periodo, MensagensErro.PeriodoInvalido);
                    return null;
                }

                // Buscar todas as transações no período (filtradas por usuário se especificado)
                var transacoesPeriodo = await _transacaoRepository.GetByPeriodoAsync(dataInicio, dataFim, usuarioId);
                
                // Filtrar transações excluídas (garantia extra, já que o repositório deve aplicar o filtro)
                transacoesPeriodo = transacoesPeriodo.Where(t => !t.Excluido).ToList();
                
                // Calcular saldo anterior (todas as transações antes da data de início)
                var transacoesAnteriores = await _transacaoRepository.GetByPeriodoAsync(
                    DateTime.MinValue, 
                    dataInicio.AddDays(-1),
                    usuarioId);
                
                transacoesAnteriores = transacoesAnteriores.Where(t => !t.Excluido).ToList();

                decimal saldoAnterior = CalcularSaldo(transacoesAnteriores);
                
                decimal totalReceitas = transacoesPeriodo
                    .Where(t => t.Tipo == TipoTransacao.Receita)
                    .Sum(t => t.Valor);
                    
                decimal totalDespesas = transacoesPeriodo
                    .Where(t => t.Tipo == TipoTransacao.Despesa)
                    .Sum(t => t.Valor);
                    
                decimal saldoFinal = saldoAnterior + totalReceitas - totalDespesas;
                
                var transacoesPorDia = (transacoesPeriodo.Count() > 100 ? transacoesPeriodo.AsParallel() : transacoesPeriodo)
                    .GroupBy(t => t.Data.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => 
                    {
                        var receitas = g.Where(t => t.Tipo == TipoTransacao.Receita);
                        var despesas = g.Where(t => t.Tipo == TipoTransacao.Despesa);
                        var totalReceitas = receitas.Sum(t => t.Valor);
                        var totalDespesas = despesas.Sum(t => t.Valor);
                        
                        return new TransacaoDiariaDTO
                        {
                            Data = g.Key,
                            TotalReceitas = totalReceitas,
                            TotalDespesas = totalDespesas,
                            SaldoDiario = totalReceitas - totalDespesas,
                            Transacoes = g.Select(t => new TransacaoDTO
                            {
                                Id = t.Id,
                                Tipo = (int)t.Tipo,
                                Data = t.Data,
                                Descricao = t.Descricao,
                                Valor = t.Valor
                            }).ToList()
                        };
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

                return resumoFinanceiro;
            }
            catch (Exception ex)
            {
                _notificationService.AddNotification(ChavesNotificacao.Erro, $"Erro ao gerar resumo financeiro: {ex.Message}");
                return null;
            }
        }
        
        private decimal CalcularSaldo(IEnumerable<Transacao> transacoes)
        {
            if (transacoes.Count() > 100)
            {
                var agrupado = transacoes.AsParallel()
                    .GroupBy(t => t.Tipo)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Valor));
                    
                decimal receitas = agrupado.ContainsKey(TipoTransacao.Receita) ? agrupado[TipoTransacao.Receita] : 0;
                decimal despesas = agrupado.ContainsKey(TipoTransacao.Despesa) ? agrupado[TipoTransacao.Despesa] : 0;
                
                return receitas - despesas;
            }
            else
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
} 
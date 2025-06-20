using System;
using System.Collections.Generic;

namespace ControleFinanceiro.Application.DTOs
{
    public class ResumoFinanceiroDTO
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal SaldoFinal { get; set; }
        public List<TransacaoDiariaDTO> TransacoesDiarias { get; set; }
        
        public string Periodo => $"{DataInicio:dd/MM/yyyy} a {DataFim:dd/MM/yyyy}";

        public ResumoFinanceiroDTO()
        {
            TransacoesDiarias = new List<TransacaoDiariaDTO>();
        }
    }

    public class TransacaoDiariaDTO
    {
        public DateTime Data { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal SaldoDiario { get; set; }
        public List<TransacaoDTO> Transacoes { get; set; }

        public TransacaoDiariaDTO()
        {
            Transacoes = new List<TransacaoDTO>();
        }
    }
} 
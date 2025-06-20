using System;

namespace ControleFinanceiro.Application.DTOs
{
    public class CreateTransacaoDTO
    {
        public int Tipo { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
    }
} 
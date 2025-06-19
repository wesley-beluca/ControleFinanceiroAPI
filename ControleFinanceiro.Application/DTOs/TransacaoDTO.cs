using ControleFinanceiro.Domain.Entities;
using System;

namespace ControleFinanceiro.Application.DTOs
{
    public class TransacaoDTO
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
    }
} 
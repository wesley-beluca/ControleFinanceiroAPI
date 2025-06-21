using ControleFinanceiro.Domain.Entities;
using System;

namespace ControleFinanceiro.Application.DTOs
{
    public class TransacaoDTO
    {
        public Guid Id { get; set; }
        public int Tipo { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataInclusao { get; set; }
        public DateTime? DataAlteracao { get; set; }
        public Guid? UsuarioId { get; set; }
    }
} 
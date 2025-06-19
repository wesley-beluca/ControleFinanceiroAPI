using System;

namespace ControleFinanceiro.Domain.Entities
{
    public class Transacao
    {
        public Guid Id { get; set; }
        public TipoTransacao Tipo { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }

        public Transacao()
        {
            Id = Guid.NewGuid();
        }

        public Transacao(TipoTransacao tipo, DateTime data, string descricao, decimal valor)
        {
            Id = Guid.NewGuid();
            Tipo = tipo;
            Data = data;
            Descricao = descricao;
            Valor = valor;
        }
    }
} 
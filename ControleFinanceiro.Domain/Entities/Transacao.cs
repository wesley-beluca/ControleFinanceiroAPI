using System;
using System.Collections.Generic;

namespace ControleFinanceiro.Domain.Entities
{
    public class Transacao
    {
        public const int DESCRICAO_MAX_LENGTH = 200;
        
        public Guid Id { get; private set; }
        public TipoTransacao Tipo { get; private set; }
        public DateTime Data { get; private set; }
        public string Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataAtualizacao { get; private set; }

        // Construtor para EF Core
        protected Transacao() 
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
        }

        public Transacao(TipoTransacao tipo, DateTime data, string descricao, decimal valor)
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
            
            // Validações via métodos
            SetTipo(tipo);
            SetData(data);
            SetDescricao(descricao);
            SetValor(valor);
        }

        // Métodos para atualização de propriedades
        public void SetTipo(TipoTransacao tipo)
        {
            // Validação do tipo de transação
            if (!Enum.IsDefined(typeof(TipoTransacao), tipo))
            {
                throw new ArgumentException("Tipo de transação inválido");
            }
            
            Tipo = tipo;
        }

        public void SetData(DateTime data)
        {
            // Validação da data (não permitir datas futuras)
            if (data > DateTime.Now)
            {
                throw new ArgumentException("Não é permitido registrar transações com data futura");
            }
            
            // Validação para limitar registros muito antigos (por exemplo, 5 anos)
            if (data < DateTime.Now.AddYears(-5))
            {
                throw new ArgumentException("Não é permitido registrar transações com mais de 5 anos");
            }
            
            Data = data;
        }

        public void SetDescricao(string descricao)
        {
            // Validação da descrição
            if (string.IsNullOrWhiteSpace(descricao))
            {
                throw new ArgumentException("A descrição da transação é obrigatória");
            }

            if (descricao.Length > DESCRICAO_MAX_LENGTH)
            {
                throw new ArgumentException($"A descrição deve ter no máximo {DESCRICAO_MAX_LENGTH} caracteres");
            }
            
            Descricao = descricao.Trim();
        }

        public void SetValor(decimal valor)
        {
            // Validação do valor
            if (valor <= 0)
            {
                throw new ArgumentException("O valor da transação deve ser maior que zero");
            }
            
            Valor = valor;
        }

        public void AtualizarDataModificacao()
        {
            DataAtualizacao = DateTime.Now;
        }
    }
} 
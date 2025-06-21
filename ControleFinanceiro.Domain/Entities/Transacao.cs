using System;
using System.Collections.Generic;

namespace ControleFinanceiro.Domain.Entities
{
    public class Transacao : Entity
    {
        public const int DESCRICAO_MAX_LENGTH = 200;
        
        public TipoTransacao Tipo { get; private set; }
        public DateTime Data { get; private set; }
        public string Descricao { get; private set; }
        public decimal Valor { get; private set; }
        
        public Guid? UsuarioId { get; private set; }
        public Usuario Usuario { get; private set; }

        protected Transacao() : base()
        {
        }

        public Transacao(TipoTransacao tipo, DateTime data, string descricao, decimal valor, Guid? usuarioId = null) : base()
        {
            // Validações via métodos
            SetTipo(tipo);
            SetData(data);
            SetDescricao(descricao);
            SetValor(valor);
            UsuarioId = usuarioId;
        }

        public void SetTipo(TipoTransacao tipo)
        {
            if (!Enum.IsDefined(typeof(TipoTransacao), tipo))
            {
                throw new ArgumentException("Tipo de transação inválido");
            }
            
            Tipo = tipo;
            AtualizarDataModificacao();
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
            AtualizarDataModificacao();
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
            AtualizarDataModificacao();
        }

        public void SetValor(decimal valor)
        {
            // Validação do valor
            if (valor <= 0)
            {
                throw new ArgumentException("O valor da transação deve ser maior que zero");
            }
            
            Valor = valor;
            AtualizarDataModificacao();
        }
        
        /// <summary>
        /// Define ou altera o usuário associado à transação
        /// </summary>
        /// <param name="usuarioId">ID do usuário ou null para remover associação</param>
        public void SetUsuario(Guid? usuarioId)
        {
            UsuarioId = usuarioId;
            AtualizarDataModificacao();
        }
    }
}
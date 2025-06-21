using System;

namespace ControleFinanceiro.Domain.Entities
{
    /// <summary>
    /// Classe base para todas as entidades do sistema
    /// </summary>
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime DataInclusao { get; protected set; }
        public DateTime? DataAlteracao { get; protected set; }
        public bool Excluido { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
            DataInclusao = DateTime.Now;
            Excluido = false;
        }

        /// <summary>
        /// Atualiza a data de alteração para o momento atual
        /// </summary>
        public void AtualizarDataModificacao()
        {
            DataAlteracao = DateTime.Now;
        }

        /// <summary>
        /// Marca a entidade como excluída
        /// </summary>
        public void MarcarComoExcluido()
        {
            Excluido = true;
            AtualizarDataModificacao();
        }
        
        /// <summary>
        /// Define a data de inclusão
        /// </summary>
        public void DefinirDataInclusao(DateTime data)
        {
            DataInclusao = data;
        }
        
        /// <summary>
        /// Define a data de alteração
        /// </summary>
        public void DefinirDataAlteracao(DateTime? data)
        {
            DataAlteracao = data;
        }
        
        /// <summary>
        /// Define o status de exclusão
        /// </summary>
        public void DefinirExcluido(bool excluido)
        {
            Excluido = excluido;
        }
    }
} 
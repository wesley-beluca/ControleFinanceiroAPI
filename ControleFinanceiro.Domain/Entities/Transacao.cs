using System;
using System.Collections.Generic;
using ControleFinanceiro.Domain.Notifications;

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

        public Transacao(TipoTransacao tipo, DateTime data, string descricao, decimal valor, Guid? usuarioId = null, Notification notification = null) : base()
        {
            var notificationResult = new Notification();
            
            // Validações via métodos
            notificationResult.AddNotifications(ValidarTipo(tipo));
            notificationResult.AddNotifications(ValidarData(data));
            notificationResult.AddNotifications(ValidarDescricao(descricao));
            notificationResult.AddNotifications(ValidarValor(valor));
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação, lança exceção apenas se não foi passado um objeto de notificação
            if (!notificationResult.IsValid && notification == null)
                throw new ArgumentException(notificationResult.GetErrorMessages());
                
            Tipo = tipo;
            Data = data;
            Descricao = descricao.Trim();
            Valor = valor;
            UsuarioId = usuarioId;
        }

        private Notification ValidarTipo(TipoTransacao tipo)
        {
            var notification = new Notification();
            
            if (!Enum.IsDefined(typeof(TipoTransacao), tipo))
            {
                notification.AddNotification("Tipo", "Tipo de transação inválido");
            }
            
            return notification;
        }
        
        public bool SetTipo(TipoTransacao tipo, Notification notification = null)
        {
            var notificationResult = ValidarTipo(tipo);
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação
            if (!notificationResult.IsValid)
            {
                // Se não foi passado um objeto de notificação, lança exceção
                if (notification == null)
                    throw new ArgumentException(notificationResult.GetErrorMessages());
                    
                return false;
            }
            
            Tipo = tipo;
            AtualizarDataModificacao();
            return true;
        }

        private Notification ValidarData(DateTime data)
        {
            var notification = new Notification();
            
            // Validação da data (não permitir datas futuras)
            if (data > DateTime.Now)
            {
                notification.AddNotification("Data", "Não é permitido registrar transações com data futura");
            }
            
            // Validação para limitar registros muito antigos (por exemplo, 5 anos)
            if (data < DateTime.Now.AddYears(-5))
            {
                notification.AddNotification("Data", "Não é permitido registrar transações com mais de 5 anos");
            }
            
            return notification;
        }
        
        public bool SetData(DateTime data, Notification notification = null)
        {
            var notificationResult = ValidarData(data);
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação
            if (!notificationResult.IsValid)
            {
                // Se não foi passado um objeto de notificação, lança exceção
                if (notification == null)
                    throw new ArgumentException(notificationResult.GetErrorMessages());
                    
                return false;
            }
            
            Data = data;
            AtualizarDataModificacao();
            return true;
        }

        private Notification ValidarDescricao(string descricao)
        {
            var notification = new Notification();
            
            // Validação da descrição
            if (string.IsNullOrWhiteSpace(descricao))
            {
                notification.AddNotification("Descricao", "A descrição da transação é obrigatória");
                return notification; // Retorna imediatamente para evitar NullReferenceException
            }

            if (descricao.Length > DESCRICAO_MAX_LENGTH)
            {
                notification.AddNotification("Descricao", $"A descrição deve ter no máximo {DESCRICAO_MAX_LENGTH} caracteres");
            }
            
            return notification;
        }
        
        public bool SetDescricao(string descricao, Notification notification = null)
        {
            var notificationResult = ValidarDescricao(descricao);
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação
            if (!notificationResult.IsValid)
            {
                // Se não foi passado um objeto de notificação, lança exceção
                if (notification == null)
                    throw new ArgumentException(notificationResult.GetErrorMessages());
                    
                return false;
            }
            
            Descricao = descricao.Trim();
            AtualizarDataModificacao();
            return true;
        }

        private Notification ValidarValor(decimal valor)
        {
            var notification = new Notification();
            
            // Validação do valor
            if (valor <= 0)
            {
                notification.AddNotification("Valor", "O valor da transação deve ser maior que zero");
            }
            
            return notification;
        }
        
        public bool SetValor(decimal valor, Notification notification = null)
        {
            var notificationResult = ValidarValor(valor);
            
            // Se foi passado um objeto de notificação, adiciona as notificações a ele
            if (notification != null)
                notification.AddNotifications(notificationResult);
            
            // Se houver erros de validação
            if (!notificationResult.IsValid)
            {
                // Se não foi passado um objeto de notificação, lança exceção
                if (notification == null)
                    throw new ArgumentException(notificationResult.GetErrorMessages());
                    
                return false;
            }
            
            Valor = valor;
            AtualizarDataModificacao();
            return true;
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
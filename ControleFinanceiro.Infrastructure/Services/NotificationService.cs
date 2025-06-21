using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Notifications;
using System.Collections.Generic;

namespace ControleFinanceiro.Infrastructure.Services
{
    /// <summary>
    /// Implementação do serviço de notificações
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly Notification _notification;

        public NotificationService()
        {
            _notification = new Notification();
        }

        /// <summary>
        /// Verifica se o serviço possui notificações
        /// </summary>
        public bool HasNotifications => !_notification.IsValid;

        /// <summary>
        /// Retorna todas as notificações
        /// </summary>
        public IReadOnlyCollection<NotificationItem> Notifications => _notification.Notifications;

        /// <summary>
        /// Adiciona uma notificação
        /// </summary>
        /// <param name="key">Chave ou propriedade relacionada à notificação</param>
        /// <param name="message">Mensagem de erro</param>
        public void AddNotification(string key, string message)
        {
            _notification.AddNotification(key, message);
        }

        /// <summary>
        /// Adiciona uma lista de notificações
        /// </summary>
        /// <param name="notifications">Lista de notificações a serem adicionadas</param>
        public void AddNotifications(IEnumerable<NotificationItem> notifications)
        {
            _notification.AddNotifications(notifications);
        }

        /// <summary>
        /// Adiciona notificações de outra instância de Notification
        /// </summary>
        /// <param name="notification">Instância de Notification</param>
        public void AddNotifications(Notification notification)
        {
            _notification.AddNotifications(notification);
        }

        /// <summary>
        /// Retorna todas as mensagens de notificação concatenadas
        /// </summary>
        public string GetErrorMessages()
        {
            return _notification.GetErrorMessages();
        }

        /// <summary>
        /// Limpa todas as notificações
        /// </summary>
        public void Clear()
        {
            _notification.Clear();
        }
    }
}

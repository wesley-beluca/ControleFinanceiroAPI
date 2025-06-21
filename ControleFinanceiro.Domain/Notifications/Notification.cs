using System.Collections.Generic;
using System.Linq;

namespace ControleFinanceiro.Domain.Notifications
{
    /// <summary>
    /// Implementação do Notification Pattern para coletar e gerenciar erros de validação
    /// </summary>
    public class Notification
    {
        private readonly List<NotificationItem> _notifications = new List<NotificationItem>();

        /// <summary>
        /// Verifica se o objeto é válido (não possui notificações)
        /// </summary>
        public bool IsValid => !_notifications.Any();

        /// <summary>
        /// Retorna todas as notificações
        /// </summary>
        public IReadOnlyCollection<NotificationItem> Notifications => _notifications.AsReadOnly();

        /// <summary>
        /// Adiciona uma notificação
        /// </summary>
        /// <param name="key">Chave ou propriedade relacionada à notificação</param>
        /// <param name="message">Mensagem de erro</param>
        public void AddNotification(string key, string message)
        {
            _notifications.Add(new NotificationItem(key, message));
        }

        /// <summary>
        /// Adiciona uma lista de notificações
        /// </summary>
        /// <param name="notifications">Lista de notificações a serem adicionadas</param>
        public void AddNotifications(IEnumerable<NotificationItem> notifications)
        {
            _notifications.AddRange(notifications);
        }

        /// <summary>
        /// Adiciona notificações de outra instância de Notification
        /// </summary>
        /// <param name="notification">Instância de Notification</param>
        public void AddNotifications(Notification notification)
        {
            _notifications.AddRange(notification.Notifications);
        }

        /// <summary>
        /// Retorna todas as mensagens de notificação concatenadas
        /// </summary>
        public string GetErrorMessages()
        {
            return string.Join(", ", _notifications.Select(n => n.Message));
        }

        /// <summary>
        /// Limpa todas as notificações
        /// </summary>
        public void Clear()
        {
            _notifications.Clear();
        }
    }
}

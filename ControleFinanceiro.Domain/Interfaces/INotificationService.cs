using ControleFinanceiro.Domain.Notifications;
using System.Collections.Generic;

namespace ControleFinanceiro.Domain.Interfaces
{
    /// <summary>
    /// Interface para o serviço de notificações
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Verifica se o serviço possui notificações
        /// </summary>
        bool HasNotifications { get; }

        /// <summary>
        /// Retorna todas as notificações
        /// </summary>
        IReadOnlyCollection<NotificationItem> Notifications { get; }

        /// <summary>
        /// Adiciona uma notificação
        /// </summary>
        /// <param name="key">Chave ou propriedade relacionada à notificação</param>
        /// <param name="message">Mensagem de erro</param>
        void AddNotification(string key, string message);

        /// <summary>
        /// Adiciona uma lista de notificações
        /// </summary>
        /// <param name="notifications">Lista de notificações a serem adicionadas</param>
        void AddNotifications(IEnumerable<NotificationItem> notifications);

        /// <summary>
        /// Adiciona notificações de outra instância de Notification
        /// </summary>
        /// <param name="notification">Instância de Notification</param>
        void AddNotifications(Notification notification);

        /// <summary>
        /// Retorna todas as mensagens de notificação concatenadas
        /// </summary>
        string GetErrorMessages();

        /// <summary>
        /// Limpa todas as notificações
        /// </summary>
        void Clear();
    }
}

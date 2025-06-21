namespace ControleFinanceiro.Domain.Notifications
{
    /// <summary>
    /// Representa um item de notificação no padrão Notification Pattern
    /// </summary>
    public class NotificationItem
    {
        /// <summary>
        /// Chave ou propriedade relacionada à notificação
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Mensagem de erro
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Cria uma nova notificação
        /// </summary>
        /// <param name="key">Chave ou propriedade relacionada à notificação</param>
        /// <param name="message">Mensagem de erro</param>
        public NotificationItem(string key, string message)
        {
            Key = key;
            Message = message;
        }
    }
}

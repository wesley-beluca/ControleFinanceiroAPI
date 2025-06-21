using System.Threading.Tasks;

namespace ControleFinanceiro.Domain.Interfaces
{
    /// <summary>
    /// Interface para o serviço de notificação de saldo negativo
    /// </summary>
    public interface INotificacaoSaldoService
    {
        /// <summary>
        /// Verifica e notifica usuários com saldo negativo
        /// </summary>
        /// <returns>Número de usuários notificados</returns>
        Task<int> NotificarUsuariosSaldoNegativoAsync();
    }
}

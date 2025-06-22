namespace ControleFinanceiro.Domain.Constants
{
    /// <summary>
    /// Classe que centraliza todas as chaves de notificação usadas no sistema
    /// </summary>
    public static class ChavesNotificacao
    {
        // Chaves gerais
        public const string ModelState = "ModelState";
        public const string Erro = "Erro";
        public const string Validacao = "Validação";
        
        // Autenticação e Usuário
        public const string Autenticacao = "Autenticação";
        public const string Usuario = "Usuário";
        public const string Senha = "Senha";
        public const string Email = "Email";
        public const string Username = "Username";
        public const string Token = "Token";
        
        // Transações
        public const string Transacao = "Transação";
        public const string Valor = "Valor";
        public const string Data = "Data";
        public const string Tipo = "Tipo";
        public const string Descricao = "Descrição";
        
        // Resumo Financeiro
        public const string ResumoFinanceiro = "ResumoFinanceiro";
        public const string Periodo = "Período";
        public const string DataInicio = "DataInicio";
        public const string DataFim = "DataFim";
    }
}

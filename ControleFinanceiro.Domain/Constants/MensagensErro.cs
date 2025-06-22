using System;

namespace ControleFinanceiro.Domain.Constants
{
    /// <summary>
    /// Classe que centraliza todas as mensagens de erro da aplicação
    /// </summary>
    public static class MensagensErro
    {
        // Mensagens gerais
        public const string ErroInterno = "Ocorreu um erro interno no servidor";
        public const string RecursoNaoEncontrado = "Recurso não encontrado";
        public const string AcessoNegado = "Acesso negado";
        
        // Autenticação e Usuário
        public const string UsuarioNaoEncontrado = "Usuário não encontrado";
        public const string SenhaInvalida = "Senha inválida";
        public const string CredenciaisInvalidas = "Nome de usuário ou senha inválidos";
        public const string EmailJaRegistrado = "Este e-mail já está registrado";
        public const string UsernameJaRegistrado = "Este nome de usuário já está em uso";
        public const string EmailJaExiste = "Este e-mail já está em uso";
        public const string UsernameJaExiste = "Este nome de usuário já está em uso";
        public const string SenhasNaoConferem = "As senhas não conferem";
        public const string TokenInvalido = "Token inválido ou expirado";
        public const string ErroEnvioEmail = "Falha ao enviar email de redefinição de senha";
        
        // Transações
        public const string TransacaoNaoEncontrada = "Transação não encontrada";
        public const string TransacaoNaoPertenceUsuario = "Esta transação não pertence ao usuário logado";
        public const string ValorTransacaoInvalido = "O valor da transação deve ser maior que zero";
        public const string DataTransacaoInvalida = "A data da transação é inválida";
        public const string TipoTransacaoInvalido = "O tipo da transação é inválido";
        
        // Validações
        public const string CampoObrigatorio = "O campo {0} é obrigatório";
        public const string TamanhoMaximo = "O campo {0} deve ter no máximo {1} caracteres";
        public const string TamanhoMinimo = "O campo {0} deve ter no mínimo {1} caracteres";
        
        // Resumo Financeiro
        public const string PeriodoInvalido = "O período informado é inválido";
        public const string DataInicioMaiorDataFim = "A data de início não pode ser maior que a data de fim";
        public const string DataInicioMaiorQueFinal = "A data inicial não pode ser maior que a data final";
    }
}

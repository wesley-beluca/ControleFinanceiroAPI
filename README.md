# Controle Financeiro Pessoal

API para controle financeiro pessoal desenvolvida em .NET 8, seguindo arquitetura em camadas e princípios de injeção de dependência. Inclui sistema completo de autenticação com JWT e gerenciamento de usuários.

## Tecnologias Utilizadas

- .NET 8
- Entity Framework Core
- SQL Server
- Swagger
- JWT (JSON Web Tokens)
- SMTP para envio de emails

## Estrutura do Projeto

O projeto segue uma arquitetura em camadas:

- **ControleFinanceiro.API**: Camada de apresentação (API)
- **ControleFinanceiro.Application**: Camada de aplicação (Serviços, DTOs)
- **ControleFinanceiro.Domain**: Camada de domínio (Entidades, Interfaces)
- **ControleFinanceiro.Infrastructure**: Camada de infraestrutura (Repositórios, Banco de Dados)

## Funcionalidades

- Cadastro, edição, exclusão e consulta de transações financeiras
- Filtro de transações por período e tipo (receita/despesa)
- Geração de resumo financeiro com fluxo de caixa diário
- Sistema completo de autenticação com JWT
- Registro e login de usuários
- Recuperação de senha via email
- Associação de transações a usuários específicos

## Como Executar

1. Clone o repositório
2. Configure a string de conexão no arquivo `appsettings.json`
3. Configure as credenciais SMTP para o serviço de email (veja a seção "Configuração de Credenciais Sensíveis")
4. Execute as migrações do Entity Framework Core:

```bash
dotnet ef database update
```

5. Execute o projeto:

```bash
dotnet run --project ControleFinanceiro.API
```

6. Acesse a documentação da API em: `https://localhost:5001/swagger`

# Dados Iniciais do Sistema

## Usuário Master

O sistema é inicializado com um usuário master padrão que pode ser utilizado para acesso inicial à API:

| Campo    | Valor                         |
| -------- | ----------------------------- |
| Username | master                        |
| Email    | admin@controle-financeiro.com |
| Senha    | senhamaster                   |
| Nome     | Admin                         |

## Configuração de Credenciais Sensíveis

Para proteger informações sensíveis como senhas SMTP, chaves de API e strings de conexão, o projeto utiliza User Secrets em ambiente de desenvolvimento e variáveis de ambiente em produção.

### Ambiente de Desenvolvimento

Para configurar as credenciais SMTP em ambiente de desenvolvimento, utilize o User Secrets:

```bash
# Inicializar user secrets (caso ainda não tenha feito)
cd ControleFinanceiro.API
dotnet user-secrets init

# Configurar a senha SMTP
dotnet user-secrets set "Email:SmtpPassword" "sua-senha-smtp"
```

### Ambiente de Produção

Em ambiente de produção, utilize variáveis de ambiente. Os nomes das variáveis devem seguir o formato com dois pontos (`:`), substituídos por dois underscores (`__`):

```
Email__SmtpPassword=sua-senha-smtp
Jwt__Key=sua-chave-jwt
```

## Configuração do Serviço de Email

O envio de emails é uma funcionalidade crucial deste projeto, especialmente para o processo de recuperação de senha. Siga as instruções abaixo para configurar corretamente:

### Opção 1: Configurar uma conta Gmail para envio de emails

1. Crie ou use uma conta Gmail existente para envio de emails
2. Ative a verificação em duas etapas na conta: https://myaccount.google.com/security
3. Gere uma senha de aplicativo:
   - Acesse https://myaccount.google.com/apppasswords
   - Selecione "Outro (Nome personalizado)" e dê um nome como "ControleFinanceiroAPI"
   - Clique em "Gerar" e copie a senha de 16 caracteres gerada
4. Use esta senha no arquivo `.env` (para Docker) ou nos user secrets (para desenvolvimento local)

### Opção 2: Usar um serviço SMTP alternativo

Você pode usar outros serviços SMTP como SendGrid, Mailgun, Amazon SES, etc. Neste caso, atualize as configurações SMTP no arquivo `.env` ou user secrets conforme necessário.

### Usando Docker

O projeto está configurado para ser executado com Docker e Docker Compose. Para isso:

1. Crie um arquivo `.env` na raiz do projeto (este arquivo já está configurado para ser ignorado pelo Git):

```
# Credenciais SMTP - OBRIGATÓRIO para funcionalidade de email
SMTP_PASSWORD=sua-senha-de-aplicativo-gmail

# Configurações SMTP (opcional - apenas se quiser alterar do padrão Gmail)
# SMTP_SERVER=smtp.seuservidor.com
# SMTP_PORT=587
# SMTP_USERNAME=seu-email@exemplo.com
# EMAIL_REMETENTE=seu-email@exemplo.com
# NOME_REMETENTE=Nome Personalizado

# Chave JWT (substitua por uma chave segura em produção)
JWT_KEY=sua-chave-jwt-segura
```

2. Execute o projeto com Docker Compose:

```bash
docker-compose up -d
```

3. A API estará disponível em `http://localhost:8080`

O Docker Compose irá carregar as variáveis de ambiente do arquivo `.env` automaticamente, mantendo suas credenciais seguras e fora do controle de versão.

## Endpoints da API

### Transações

- `GET /api/transacoes`: Lista todas as transações
- `GET /api/transacoes/{id}`: Obtém uma transação específica
- `GET /api/transacoes/periodo?dataInicio={data}&dataFim={data}`: Lista transações por período
- `GET /api/transacoes/tipo/{tipo}`: Lista transações por tipo (Receita/Despesa)
- `POST /api/transacoes`: Cria uma nova transação
- `PUT /api/transacoes/{id}`: Atualiza uma transação existente
- `DELETE /api/transacoes/{id}`: Remove uma transação

### Resumo Financeiro

- `GET /api/resumofinanceiro?dataInicio={data}&dataFim={data}`: Gera um resumo financeiro para o período especificado

### Autenticação

- `POST /auth/login`: Autenticação de usuário
- `POST /auth/register`: Registro de novo usuário
- `POST /auth/forgot-password`: Solicita redefinição de senha
- `POST /auth/reset-password`: Redefine a senha com o token recebido por email

## Frontend

Esta API foi projetada para integração com um frontend Vue 3, do projeto https://github.com/wesley-beluca/GerencieAqui.git.

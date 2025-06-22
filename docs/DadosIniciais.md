# Dados Iniciais do Sistema

## Usuário Master

O sistema é inicializado com um usuário master padrão que pode ser utilizado para acesso inicial à API:

| Campo    | Valor                           |
|----------|--------------------------------|
| Username | master                         |
| Email    | admin@controle-financeiro.com  |
| Senha    | senhamaster                    |
| Nome     | Admin                          |

## Transações Iniciais

O banco de dados é populado com as seguintes transações iniciais associadas ao usuário master:

| Tipo    | Data       | Descrição             | Valor (R$) |
|---------|------------|----------------------|------------|
| Despesa | 29/08/2022 | Cartão de Crédito    | 825,82     |
| Despesa | 29/08/2022 | Curso C#             | 200,00     |
| Receita | 31/08/2022 | Salário              | 7.000,00   |
| Despesa | 01/09/2022 | Mercado              | 3.000,00   |
| Despesa | 01/09/2022 | Farmácia             | 300,00     |
| Despesa | 01/09/2022 | Combustível          | 800,25     |
| Despesa | 15/09/2022 | Financiamento Carro  | 900,00     |
| Despesa | 22/09/2022 | Financiamento Casa   | 1.200,00   |
| Receita | 25/09/2022 | Freelance Projeto XPTO | 2.500,00   |

## Como Funciona a Inicialização

Os dados iniciais são criados automaticamente quando:

1. O aplicativo é iniciado pela primeira vez
2. O banco de dados está vazio (não possui usuários ou transações)

Se o banco de dados já contiver registros, a inicialização será ignorada para evitar duplicação de dados.

## Autenticação com o Usuário Master

Para autenticar com o usuário master, envie uma requisição POST para o endpoint `/auth/login` com o seguinte corpo:

```json
{
  "email": "admin@controle-financeiro.com",
  "senha": "senhamaster"
}
```

O sistema retornará um token JWT que pode ser utilizado para acessar os endpoints protegidos da API.

## Observações de Segurança

É altamente recomendável alterar a senha do usuário master após o primeiro acesso ao sistema em ambiente de produção.

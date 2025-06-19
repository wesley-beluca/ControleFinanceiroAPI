# Controle Financeiro Pessoal

API para controle financeiro pessoal desenvolvida em .NET 8, seguindo arquitetura em camadas e princípios de injeção de dependência.

## Tecnologias Utilizadas

- .NET 8
- Entity Framework Core
- SQL Server
- Swagger

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

## Como Executar

1. Clone o repositório
2. Configure a string de conexão no arquivo `appsettings.json`
3. Execute as migrações do Entity Framework Core:

```bash
dotnet ef database update
```

4. Execute o projeto:

```bash
dotnet run --project ControleFinanceiro.API
```

5. Acesse a documentação da API em: `https://localhost:5001/swagger`

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

## Frontend

Esta API foi projetada para integração com um frontend Vue 3, que será desenvolvido separadamente.

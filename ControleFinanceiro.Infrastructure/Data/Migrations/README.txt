Para criar e aplicar migrações do Entity Framework Core, siga os passos abaixo:

1. Instale a ferramenta dotnet-ef globalmente (se ainda não estiver instalada):
   
   ```
   dotnet tool install --global dotnet-ef
   ```

2. Navegue até a pasta raiz da solução e execute o comando para criar uma nova migração:
   
   ```
   dotnet ef migrations add InitialCreate --project ControleFinanceiro.Infrastructure --startup-project ControleFinanceiro.API
   ```

3. Para aplicar a migração ao banco de dados:
   
   ```
   dotnet ef database update --project ControleFinanceiro.Infrastructure --startup-project ControleFinanceiro.API
   ```

4. Para remover a última migração (caso necessário):
   
   ```
   dotnet ef migrations remove --project ControleFinanceiro.Infrastructure --startup-project ControleFinanceiro.API
   ```

Observações:
- Certifique-se de que a string de conexão no arquivo appsettings.json está configurada corretamente.
- O banco de dados será criado automaticamente ao aplicar a primeira migração.
- Os dados iniciais de exemplo serão inseridos automaticamente através do método Seed no AppDbContext. 
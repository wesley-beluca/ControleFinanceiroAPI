FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ControleFinanceiro.API/ControleFinanceiro.API.csproj", "ControleFinanceiro.API/"]
COPY ["ControleFinanceiro.Application/ControleFinanceiro.Application.csproj", "ControleFinanceiro.Application/"]
COPY ["ControleFinanceiro.Domain/ControleFinanceiro.Domain.csproj", "ControleFinanceiro.Domain/"]
COPY ["ControleFinanceiro.Infrastructure/ControleFinanceiro.Infrastructure.csproj", "ControleFinanceiro.Infrastructure/"]
RUN dotnet restore "ControleFinanceiro.API/ControleFinanceiro.API.csproj"
COPY . .
WORKDIR "/src/ControleFinanceiro.API"
RUN dotnet build "ControleFinanceiro.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ControleFinanceiro.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ControleFinanceiro.API.dll"] 
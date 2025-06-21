# Guia de Implementação do Notification Pattern

## Introdução

Este documento descreve como utilizar o Notification Pattern implementado no projeto ControleFinanceiroAPI. O padrão foi adotado para melhorar o tratamento de erros e validações, permitindo acumular múltiplos erros antes de interromper o fluxo de execução.

## Componentes do Notification Pattern

### 1. Classes Principais

- **NotificationItem**: Representa um item individual de notificação com uma chave e uma mensagem.
- **Notification**: Classe que armazena e gerencia uma coleção de NotificationItems.
- **INotificationService**: Interface que define os métodos para gerenciar notificações na aplicação.
- **NotificationService**: Implementação da interface INotificationService.

### 2. Integração com Entidades

As entidades do domínio foram modificadas para aceitar um objeto Notification opcional em seus métodos de validação. Quando fornecido, os erros são acumulados neste objeto em vez de lançar exceções.

## Como Utilizar o Notification Pattern

### Em Entidades de Domínio

```csharp
// Exemplo de método de validação em uma entidade
public void SetNome(string nome, Notification notification = null)
{
    if (string.IsNullOrWhiteSpace(nome))
    {
        if (notification != null)
        {
            notification.AddError("Nome", "O nome não pode ser vazio");
        }
        else
        {
            throw new ArgumentException("O nome não pode ser vazio");
        }
    }
    
    // Continua a execução se não houver erro ou se estiver coletando erros
    this.Nome = nome;
}
```

### Em Serviços

```csharp
// Exemplo de método em um serviço
public async Task<Result<T>> ProcessarOperacaoAsync(DTO dto)
{
    // Limpa notificações anteriores
    _notificationService.Clear();
    
    // Validação do DTO
    var validationResult = await _validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        foreach (var error in validationResult.Errors)
        {
            _notificationService.Add(error.PropertyName, error.ErrorMessage);
        }
        return Result<T>.Fail("Erros de validação encontrados");
    }
    
    try
    {
        // Cria um objeto Notification para coletar erros da entidade
        var notification = new Notification();
        
        // Cria ou atualiza a entidade passando o notification
        var entidade = new Entidade(dto.Prop1, dto.Prop2, notification);
        
        // Verifica se há erros de validação na entidade
        if (notification.HasErrors())
        {
            // Transfere as notificações da entidade para o serviço
            foreach (var item in notification.Errors)
            {
                _notificationService.Add(item.Key, item.Message);
            }
            return Result<T>.Fail("Erros de validação na entidade");
        }
        
        // Continua o processamento se não houver erros
        // ...
        
        return Result<T>.Ok(resultado);
    }
    catch (Exception ex)
    {
        _notificationService.Add("Erro", ex.Message);
        return Result<T>.Fail($"Erro ao processar operação: {ex.Message}");
    }
}
```

### Em Controllers

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] DTO dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
        
    var result = await _service.ProcessarOperacaoAsync(dto);
    
    if (!result.Success && _notificationService.HasNotifications())
    {
        return BadRequest(new { 
            message = "Ocorreram erros ao processar a requisição", 
            errors = _notificationService.GetNotifications().Select(n => n.Message).ToList() 
        });
    }
    
    return result.Success 
        ? Ok(result)
        : BadRequest(result);
}
```

## Registrando o Notification Service

O serviço de notificação deve ser registrado no contêiner de injeção de dependência:

```csharp
// No método AddInfrastructure da classe DependencyInjection
services.AddScoped<INotificationService, NotificationService>();
```

## Abordagem Híbrida

No projeto ControleFinanceiroAPI, adotamos uma abordagem híbrida:

1. **Notification Pattern**: Usado para validações de negócio e entrada de dados, permitindo acumular múltiplos erros.
2. **Exceções**: Usadas para erros técnicos e situações excepcionais que devem interromper o fluxo imediatamente.

### Quando usar cada abordagem:

- **Notification Pattern**: Para validações de dados de entrada, regras de negócio e situações onde é útil coletar múltiplos erros antes de responder ao usuário.
- **Exceções**: Para erros técnicos (falha de conexão, timeout), violações de segurança, ou situações onde é impossível continuar o fluxo.

## Benefícios

1. **Melhor experiência do usuário**: Retorna todos os erros de uma vez, em vez de um por vez.
2. **Código mais limpo**: Evita aninhamento excessivo de blocos try/catch.
3. **Flexibilidade**: Permite decidir quando interromper o fluxo e quando coletar erros.
4. **Compatibilidade**: Mantém compatibilidade com código legado que espera exceções.

## Considerações Finais

Ao implementar novos recursos ou modificar os existentes, considere se a validação deve interromper o fluxo imediatamente (exceção) ou se deve coletar erros para apresentá-los juntos (Notification Pattern). Na maioria dos casos de validação de dados, o Notification Pattern oferece uma melhor experiência para o usuário final.

# Boas Práticas para Lançamento de Exceções e o Notification Pattern

## Introdução

O lançamento de exceções é uma prática comum no desenvolvimento de software para lidar com situações excepcionais ou erros. No entanto, o uso excessivo ou inadequado de exceções pode levar a problemas de desempenho, manutenção e legibilidade do código. Este documento apresenta diretrizes para o lançamento adequado de exceções e como o Notification Pattern pode ser uma alternativa eficaz em determinados cenários.

## Quando Lançar Exceções

### Use exceções para:

1. **Situações verdadeiramente excepcionais**: Condições que impedem a continuidade normal da execução do programa.
2. **Erros de programação**: Violações de contratos ou pré-condições que indicam bugs no código.
3. **Falhas de recursos externos**: Problemas com recursos externos como banco de dados, rede ou sistema de arquivos.
4. **Violações de segurança**: Tentativas de acesso não autorizado ou outras questões de segurança.

### Evite exceções para:

1. **Controle de fluxo**: Não use exceções como mecanismo de controle de fluxo normal do programa.
2. **Validações de entrada previsíveis**: Para dados de entrada que frequentemente podem ser inválidos.
3. **Situações de negócio esperadas**: Condições que fazem parte do fluxo normal de negócio.

## Boas Práticas para Exceções

1. **Crie hierarquias de exceções**: Desenvolva uma hierarquia de exceções específicas para o domínio da aplicação.
2. **Use mensagens claras e informativas**: Inclua detalhes suficientes para entender e resolver o problema.
3. **Documente as exceções**: Documente todas as exceções que um método pode lançar.
4. **Capture apenas o que pode tratar**: Não capture exceções que você não pode tratar adequadamente.
5. **Preserve a pilha de chamadas**: Ao relançar exceções, preserve a pilha de chamadas original.
6. **Evite exceções genéricas**: Prefira exceções específicas em vez de `Exception` genérica.
7. **Limpe recursos**: Use blocos `finally` ou construções `using` para garantir a liberação de recursos.

## O Notification Pattern

O Notification Pattern é uma alternativa ao uso excessivo de exceções, especialmente para validações de entrada e regras de negócio.

### Conceito

O Notification Pattern consiste em coletar todas as violações de regras ou erros durante a execução de uma operação, em vez de interromper o fluxo na primeira ocorrência. Isso permite:

1. **Validação completa**: O usuário recebe todos os erros de uma vez, não apenas o primeiro encontrado.
2. **Melhor experiência do usuário**: Especialmente em interfaces de usuário, onde é frustrante corrigir erros um por um.
3. **Melhor desempenho**: Evita o custo de criação e tratamento de múltiplas exceções.

### Implementação

Uma implementação típica do Notification Pattern inclui:

1. **Coleção de notificações**: Uma lista ou coleção para armazenar mensagens de erro.
2. **Métodos de adição**: Para adicionar erros à coleção.
3. **Verificação de validade**: Método para verificar se há erros na coleção.
4. **Acesso às notificações**: Métodos para acessar as mensagens de erro coletadas.

### Exemplo de Implementação

```csharp
public class Notification
{
    private readonly List<string> _errors = new List<string>();
    
    public void AddError(string message)
    {
        _errors.Add(message);
    }
    
    public bool IsValid => !_errors.Any();
    
    public IReadOnlyCollection<string> Errors => _errors.AsReadOnly();
    
    public string ErrorMessage => string.Join(", ", _errors);
}
```

## Correlação entre Exceções e Notification Pattern

### Quando usar exceções:

1. **Falhas técnicas**: Problemas de infraestrutura, conexão, etc.
2. **Erros de programação**: Violações de contrato, null references, etc.
3. **Situações que impedem completamente a execução**: Quando não faz sentido continuar.

### Quando usar Notification Pattern:

1. **Validações de entrada**: Formulários, APIs, etc.
2. **Regras de negócio**: Validações de domínio que podem ter múltiplas falhas.
3. **Operações em lote**: Quando é desejável continuar processando mesmo com alguns erros.

## Abordagem Híbrida

Uma abordagem eficaz é combinar os dois padrões:

1. Use o Notification Pattern para coletar erros de validação e regras de negócio.
2. Se a validação falhar (há notificações), lance uma exceção específica que contenha todas as notificações.
3. Use exceções puras para erros técnicos e situações verdadeiramente excepcionais.

## Middleware Global de Tratamento de Exceções

Para complementar o Notification Pattern, implementamos um middleware global de tratamento de exceções que captura exceções não tratadas em qualquer ponto da aplicação e as converte em respostas padronizadas.

### Implementação

O middleware foi implementado na classe `ExceptionMiddleware` e registrado no pipeline HTTP da aplicação:

```csharp
// ExceptionMiddleware.cs
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly INotificationService _notificationService;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        INotificationService notificationService)
    {
        _next = next;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        _notificationService.Clear();
        _notificationService.AddNotification(ChavesNotificacao.Erro, MensagensErro.ErroInterno);

        var response = new
        {
            sucesso = false,
            erros = new[] { new { Key = ChavesNotificacao.Erro, Message = MensagensErro.ErroInterno } }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}
```

### Registro no Pipeline

O middleware é registrado no pipeline HTTP da aplicação através de uma extensão:

```csharp
// Program.cs
app.UseGlobalExceptionHandler();
```

### Benefícios

1. **Tratamento centralizado**: Todas as exceções não tratadas são capturadas em um único ponto.
2. **Respostas padronizadas**: Todas as exceções são convertidas em respostas JSON padronizadas.
3. **Logging automático**: Todas as exceções são automaticamente registradas no sistema de logs.
4. **Segurança**: Detalhes sensíveis de exceções não são expostos para o cliente.
5. **Integração com o Notification Pattern**: O middleware utiliza o serviço de notificação para manter a consistência com o resto da aplicação.

## Conclusão

O equilíbrio entre o uso de exceções, o Notification Pattern e o middleware global de tratamento de exceções resulta em um código mais robusto, eficiente e amigável ao usuário. A combinação dessas abordagens proporciona:

1. **Validação completa** para o usuário com todas as mensagens de erro de uma vez.
2. **Tratamento consistente de erros** em toda a aplicação.
3. **Respostas padronizadas** para todos os tipos de erro.
4. **Melhor experiência do usuário** com mensagens de erro claras e em português.
5. **Maior segurança** ao não expor detalhes internos de exceções.

A chave é entender quando cada abordagem é mais apropriada e implementá-las de forma consistente em toda a aplicação.

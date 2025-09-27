# Cenários práticos de cache de requisições no .NET

Este guia complementa o documento [Estratégias de cache de requisições no .NET](./http-request-caching.md) com exemplos reais de como aplicar as abordagens de cache em projetos modernos. Cada cenário inclui os objetivos de negócio, arquitetura sugerida, configuração de cache e pontos de atenção para operação em produção.

## 1. API pública com Output Caching + CDN

**Contexto**: uma API de catálogo (GET `/produtos`) é consultada por milhares de clientes B2C. Os dados são atualizados a cada 15 minutos via processo ETL.

**Estratégia**:

1. Utilizar o middleware de Output Caching para manter o resultado da rota em memória por 5 minutos.
2. Colocar um CDN (Azure Front Door) na frente da API para absorver tráfego global.
3. Propagar cabeçalhos `Cache-Control: public, max-age=300` e `ETag` para que clientes e CDN reutilizem a resposta.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Catalogo", policy =>
        policy.Expire(TimeSpan.FromMinutes(5))
              .SetCacheKeyPrefix("catalogo")
              .Tag("catalogo-lista"));
});

var app = builder.Build();

app.MapGet("/produtos", async (ICatalogoService catalogo) =>
{
    var dados = await catalogo.ObterProdutosAsync();
    return Results.Ok(dados);
})
.WithName("ListaProdutos")
.CacheOutput("Catalogo")
.WithMetadata(new ResponseCacheAttribute
{
    Duration = 300,
    Location = ResponseCacheLocation.Any
});

app.Run();
```

**Operação**:

- Sempre que um lote de atualização terminar, publicar um evento em um tópico (por exemplo, Service Bus) para que um worker invalide o cache via `OutputCacheStore.EvictByTagAsync("catalogo-lista")`.
- No CDN, configure *cache purge* automático ouvindo o mesmo evento ou via API REST quando a atualização concluir.

## 2. Microserviço agregador com cache distribuído

**Contexto**: o serviço `OrquestradorPedidos` chama três APIs externas (clientes, estoque, preços) para montar uma resposta. Durante promoções, o volume triplica e as APIs parceiros impõem limite de requests por minuto.

**Estratégia**:

1. Adicionar um `DelegatingHandler` que cacheia o payload das chamadas GET em Redis.
2. Configurar tempos de expiração diferentes por recurso (clientes: 5 minutos, estoque: 30 segundos, preços: 60 segundos).
3. Usar `IHttpClientFactory` para isolar configurações por cliente.

```csharp
public sealed class RedisCacheHandler : DelegatingHandler
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _ttl;

    public RedisCacheHandler(IDistributedCache cache, TimeSpan ttl)
    {
        _cache = cache;
        _ttl = ttl;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var cacheKey = $"http:{request.RequestUri}";
        var cached = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached.ToHttpResponseMessage(request);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            await _cache.SetAsync(cacheKey, await response.ToByteArrayAsync(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _ttl
            }, cancellationToken);
        }

        return response;
    }
}
```

```csharp
public static class HttpResponseCacheExtensions
{
    public static async Task<byte[]> ToByteArrayAsync(this HttpResponseMessage response)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync();
        return JsonSerializer.SerializeToUtf8Bytes(new CachedHttpResponse
        {
            StatusCode = (int)response.StatusCode,
            Content = bytes,
            Headers = response.Headers.Concat(response.Content.Headers)
                .ToDictionary(h => h.Key, h => h.Value.ToArray())
        });
    }

    public static HttpResponseMessage ToHttpResponseMessage(this byte[] cached, HttpRequestMessage request)
    {
        var stored = JsonSerializer.Deserialize<CachedHttpResponse>(cached)!;
        var message = new HttpResponseMessage((HttpStatusCode)stored.StatusCode)
        {
            RequestMessage = request,
            Content = new ByteArrayContent(stored.Content)
        };

        foreach (var header in stored.Headers)
        {
            if (!message.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return message;
    }

    private sealed record CachedHttpResponse
    {
        public int StatusCode { get; init; }
        public byte[] Content { get; init; } = Array.Empty<byte>();
        public Dictionary<string, string[]> Headers { get; init; } = new();
    }
}
```

> Esses helpers utilizam `System.Text.Json` e combinam os cabeçalhos da resposta para reconstruir o `HttpResponseMessage` quando um *cache hit* acontece.

```csharp
builder.Services.AddStackExchangeRedisCache(o => o.Configuration = configuration["Redis:ConnectionString"]);

builder.Services.AddHttpClient("Clientes", client =>
    {
        client.BaseAddress = new Uri(configuration["Apis:Clientes"]);
    })
    .AddHttpMessageHandler(sp => new RedisCacheHandler(
        sp.GetRequiredService<IDistributedCache>(),
        TimeSpan.FromMinutes(5)));

builder.Services.AddHttpClient("Estoque", client =>
    {
        client.BaseAddress = new Uri(configuration["Apis:Estoque"]);
    })
    .AddHttpMessageHandler(sp => new RedisCacheHandler(
        sp.GetRequiredService<IDistributedCache>(),
        TimeSpan.FromSeconds(30)));
```

**Operação**:

- Armazene métricas de `hit ratio` no Redis usando `INCR` em chaves específicas para cada recurso.
- Quando uma API externa enviar webhook de atualização de estoque, invalide o cache usando `IDatabase.KeyDeleteAsync(cacheKey)`.

## 3. Relatórios gerados sob demanda com cache em memória

**Contexto**: um dashboard interno permite que analistas exportem relatórios pesados (CSV de vendas). O cálculo demora ~8 segundos, mas os analistas costumam repetir a mesma consulta várias vezes por hora.

**Estratégia**:

1. Usar `IMemoryCache` para guardar o resultado do relatório, chaveando por usuário + filtros.
2. Aplicar *sliding expiration* de 20 minutos para manter o cache enquanto houver uso.
3. Persistir o arquivo temporário em `IFileProvider` ou armazenamento temporário para não ocupar memória excessiva.

```csharp
public class RelatorioService
{
    private readonly IMemoryCache _cache;
    private readonly IRelatorioRepository _repo;

    public RelatorioService(IMemoryCache cache, IRelatorioRepository repo)
    {
        _cache = cache;
        _repo = repo;
    }

    public async Task<Stream> GerarAsync(RelatorioFiltro filtro, string usuarioId)
    {
        var cacheKey = $"relatorio:{usuarioId}:{filtro.Hash()}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(20);
            entry.Size = 1; // habilite SizeLimit para controlar memória

            var csv = await _repo.GerarCsvAsync(filtro);
            return csv;
        });
    }
}
```

**Operação**:

- Configure `MemoryCacheOptions.SizeLimit` para evitar pressão de memória.
- Use um `IHostedService` diário para limpar arquivos temporários antigos.

## 4. Mobile backend com cache condicional no cliente

**Contexto**: o app mobile consome uma API de promoções. Para economizar dados móveis, o cliente armazena respostas localmente e faz *conditional requests*.

**Estratégia**:

1. Incluir cabeçalhos `ETag` e `Last-Modified` nas respostas da API.
2. No cliente Xamarin/.NET MAUI, usar `HttpClient` que guarda os valores e envia `If-None-Match`.
3. Persistir o payload em SQLite para leitura offline.

```csharp
// Handler simplificado para MAUI/Xamarin
public class ConditionalRequestHandler : DelegatingHandler
{
    private readonly IMetadataStore _metadata;

    public ConditionalRequestHandler(IMetadataStore metadata)
        => _metadata = metadata;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Get)
        {
            var metadata = await _metadata.ObterAsync(request.RequestUri!);
            if (metadata is not null)
            {
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(metadata.ETag));
                request.Headers.IfModifiedSince = metadata.LastModified;
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            return await _metadata.RetornarRespostaCacheadaAsync(request.RequestUri!);
        }

        if (response.Headers.ETag is not null)
        {
            await _metadata.SalvarAsync(request.RequestUri!, response.Headers.ETag.Tag!, response.Content.Headers.LastModified);
        }

        return response;
    }
}
```

**Operação**:

- Ao detectar `401 Unauthorized`, limpe o cache para evitar mostrar promoções antigas para usuários diferentes.
- Faça *prefetch* quando o usuário estiver em Wi-Fi para melhorar experiência offline.

## 5. Auditoria e observabilidade do cache

Em todos os cenários anteriores, implemente telemetria que mostre:

- Latência com e sem cache (ex.: registrando `X-Cache: HIT|MISS` nas respostas).
- Saturação de armazenamento (memória, Redis) e número de evictions.
- Alertas quando o *hit ratio* estiver abaixo da meta (ex.: < 70%).

Um exemplo usando Application Insights:

```csharp
var hit = response.Headers.TryGetValues("X-Cache", out var values) && values.Contains("HIT");
telemetryClient.TrackMetric("cache-hit", hit ? 1 : 0, new Dictionary<string, string>
{
    ["rota"] = request.RequestUri!.AbsolutePath,
    ["servico"] = "Catalogo"
});
```

## Checklist para colocar em produção

- [ ] Definir claramente quais rotas podem ser cacheadas e qual o nível de consistência esperado.
- [ ] Ter estratégia de invalidação automatizada (eventos, purge na CDN, limpeza agendada).
- [ ] Revisar implicações de compliance (LGPD/GDPR) antes de armazenar respostas personalizadas.
- [ ] Instrumentar logs e métricas que permitam identificar quedas no acerto do cache.
- [ ] Documentar cabeçalhos suportados para clientes externos.

Com esses exemplos, é possível aplicar as estratégias de cache no mundo real, reduzindo custos de infraestrutura enquanto mantém a experiência do usuário rápida e estável.

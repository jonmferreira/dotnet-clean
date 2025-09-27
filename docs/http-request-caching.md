# Estratégias de cache de requisições no .NET

Quando falamos em cache de requisições no .NET estamos normalmente tentando reduzir latência e carga repetindo resultados idempotentes em vez de reexecutar chamadas HTTP ou lógicas pesadas. A escolha da estratégia depende de onde o cache ficará (cliente, servidor de API ou entre eles), dos requisitos de consistência e de como a aplicação será implantada. Abaixo estão as principais abordagens suportadas pelo ecossistema .NET.

## 1. Cache de resposta no servidor (API ASP.NET Core)

- **Middleware `ResponseCaching`**: adiciona suporte a cabeçalhos `Cache-Control`, `Expires` e `Vary` para que o próprio ASP.NET Core sirva respostas cacheadas. Útil para dados públicos ou que toleram alguma defasagem.
- **Output Caching** (ASP.NET Core 7+): substitui o middleware anterior com política mais flexível (por rota, claims, query strings). Boa escolha para APIs REST que repetem resultados idempotentes.
- **ETag e condicionais HTTP**: mesmo sem guardar payloads, usar `ETag`/`If-None-Match` ou `If-Modified-Since` permite que clientes evitem baixar corpo completo quando nada mudou.

## 2. Cache na aplicação cliente (`HttpClient`)

- **Delegating handlers**: um `HttpMessageHandler` customizado pode interceptar respostas e salvá-las em `IMemoryCache`, `IDistributedCache` ou Redis. Isso evita reexecutar chamadas externas.
- **`IHttpClientFactory` + Polly**: Polly oferece uma política de cache (`CachePolicy`) que, combinada com o `IAsyncCacheProvider`, guarda respostas idempotentes em memória ou Redis. Ideal para microservices.
- **Bibliotecas de terceiros**: opções como CacheCow, LazyCache e FusionCache já oferecem handlers prontos com suporte a expiração absoluta, sliding window e invalidação.

## 3. Cache em memória local

- **`IMemoryCache`**: simples e eficiente, recomendado para instâncias únicas (mono-servidor) ou dados específicos da requisição. Combine com `MemoryCacheEntryOptions` para TTL, prioridade e eviction.
- **`MemoryCache` com `CacheItemPolicy`** (para .NET Framework): alternativa quando ainda não está em ASP.NET Core.

## 4. Cache distribuído

- **`IDistributedCache`**: abstração nativa para Redis, SQL Server ou NCache. Combinado com handlers de `HttpClient` permite cache compartilhado entre instâncias.
- **StackExchange.Redis / Redis Output Cache**: aplicações ASP.NET Core podem emparelhar o Output Cache com Redis para cenários horizontais.
- **Cache Aside Pattern**: padrão comum para manter consistência – ler do cache, se não existir buscar do serviço e gravar.

## 5. CDN e Reverse Proxy

- Quando a aplicação expõe APIs públicas, colocar um CDN (CloudFront, Azure Front Door) ou reverse proxy (NGINX, YARP) com políticas `Cache-Control` reduz a necessidade de hits ao servidor.
- Combine com `Vary` e bust automático (purge) para lidar com dados semi-estáticos.

## 6. Estratégias de invalidação

- Defina tempos de expiração compatíveis com o SLA de atualização (TTL e sliding expiration).
- Use cache busting por chave baseada em versão (`cacheKey = $"clientes:{versao}:{id}"`).
- Avalie triggers explícitas (eventos, message bus) para invalidar caches distribuídos quando dados forem alterados.

## 7. Boas práticas gerais

- Separe requisições idempotentes (GET) das mutações; apenas as primeiras devem ser cacheadas.
- Monitore hit ratio e tempo de expiração com métricas (Application Insights, Prometheus).
- Documente políticas e cabeçalhos expostos para garantir interoperabilidade com clientes.
- Teste concorrência e invalidação usando testes de integração; race conditions são comuns.

## Exemplo básico com Polly e `IMemoryCache`

```csharp
services.AddMemoryCache();
services.AddHttpClient("github")
        .AddPolicyHandler((sp, request) =>
        {
            var cache = sp.GetRequiredService<IMemoryCache>();
            return Policy.CacheAsync(new MemoryCacheProvider(cache), TimeSpan.FromMinutes(5));
        });
```

Esse padrão cria um handler que busca a resposta cacheada antes de ir à rede. Para cenários distribuídos, troque `MemoryCacheProvider` por uma implementação baseada em Redis.

## Quando não usar cache

- Dados altamente dinâmicos (preços em tempo real, estoque) onde consistência forte é crítica.
- Respostas personalizadas por usuário sem um mecanismo de invalidação precisa.
- Quando o custo de serializar/deserializar excede o ganho do cache.

Com esses componentes, é possível compor uma solução de cache de requisições robusta no .NET, alinhando performance e consistência conforme o contexto do projeto.

> Para ver como aplicar essas estratégias em projetos reais, consulte [Cenários práticos de cache de requisições no .NET](./http-request-caching-scenarios.md).

# Paralelismo e Concorrência no .NET

O .NET oferece diversas primitivas para lidar com **concorrência** (trabalhos que progridem juntos
compartilhando recursos) e **paralelismo** (trabalhos que realmente executam simultaneamente, tipicamente
para ganho de desempenho em workloads CPU-bound). No contexto deste projeto, ambos os conceitos podem
ser aplicados para otimizar operações de I/O, como chamadas a APIs externas, e para dividir cálculos
intensivos entre múltiplos núcleos.

## Abordagens idiomáticas no .NET

1. **Task-based Asynchronous Pattern (TAP)**: `Task` e `ValueTask` expõem operações assíncronas com
   suporte a `async`/`await`. A API da `Parking.Api` já segue esse padrão nos controladores, liberando
   threads do servidor para lidar com mais requisições enquanto aguarda I/O (por exemplo, acesso ao
   banco ou APIs externas).
2. **`IAsyncEnumerable<T>`**: permite transmitir resultados conforme vão ficando prontos, reduzindo uso de
   memória em consultas mais longas. Pode ser útil para listar tickets de forma paginada ou streamada.
3. **Canalizações com `System.Threading.Channels`**: úteis para desacoplar produtores e consumidores em
   cenários de alto throughput, como processar eventos de entrada de veículos.
4. **Primitivas de sincronização**: `SemaphoreSlim`, `Mutex`, `ReaderWriterLockSlim` e `Monitor` controlam
   acesso a recursos compartilhados quando concorrência não pode ser completamente eliminada.
5. **`Parallel` e PLINQ**: aceleram workloads CPU-bound, como cálculos agregados sobre um grande conjunto de
   tickets arquivados.

## Boas práticas

- Prefira APIs assíncronas fim-a-fim. Métodos `async` que retornam `Task<IActionResult>` impedem deadlocks
  em servidores ASP.NET Core e melhoram o throughput.
- Evite acessar `Result` ou `Wait()` em `Task`, pois bloqueiam threads do pool.
- Em workloads CPU-bound, considere `Parallel.ForEachAsync`, `Task.Run` com limites de paralelismo ou
  `IAsyncEnumerable<T>` para processar bateladas em lotes pequenos.
- Centralize recursos compartilhados (como caches ou contextos EF) com injeção de dependência e escopos
  apropriados. Use contextos `DbContext` scoped por requisição.

## Rotas sugeridas para exemplificar o uso

Para ilustrar o uso de concorrência e paralelismo dentro da API, seguem rotas hipotéticas que poderiam ser
adicionadas:

| Rota | Verbo | Objetivo | Conceito demonstrado |
| ---- | ----- | -------- | -------------------- |
| `/api/tickets/batch-checkout` | `POST` | Recebe uma lista de identificadores de tickets e os finaliza em paralelo, utilizando `Parallel.ForEachAsync` para distribuir a carga entre núcleos. | Paralelismo CPU-bound e controle de paralelismo com `ParallelOptions.MaxDegreeOfParallelism`. |
| `/api/tickets/summary/concurrent` | `GET` | Calcula estatísticas (total ativo, receita, média de permanência) agregando dados de múltiplos repositórios por meio de `Task.WhenAll`. | Concorrência assíncrona com múltiplas consultas I/O-bound em paralelo. |
| `/api/vehicleinspections/stream` | `GET` | Devolve inspeções em streaming com `IAsyncEnumerable`, permitindo que o cliente consuma os resultados conforme chegam. | Concorrência cooperativa usando stream assíncrono. |
| `/api/cnpj/batch` | `POST` | Consulta o cadastro de diversos CNPJs utilizando `SemaphoreSlim` para limitar chamadas concorrentes à API externa. | Concorrência controlada e _rate limiting_. |

Esses exemplos podem servir como ponto de partida para implementar cenários reais. Cada rota pode ser
prototipada com _services_ específicos (ex.: `ITicketBatchCheckoutService`) e testes cobrindo situações
como conflitos de concorrência, erros individuais em lotes e limites de paralelismo.

## Próximos passos sugeridos

1. **Adicionar serviços de aplicação** que encapsulem as estratégias acima (ex.: serviço de checkout em
   lote que aceita um `ParallelOptions`).
2. **Definir políticas de resiliência** com `Polly` ou `HttpClientFactory` para lidar com falhas em
   chamadas externas quando várias ocorrem simultaneamente.
3. **Instrumentar métricas** (ex.: `EventCounters`, `ILogger`, `ActivitySource`) para monitorar filas,
   tempos de espera e grau de paralelismo efetivo.
4. **Criar documentação** demonstrando exemplos de uso e comparações de desempenho ao habilitar o
   processamento concorrente.

Com essas abordagens, o projeto pode demonstrar claramente como a plataforma .NET lida com workloads
concorrentes e paralelos mantendo legibilidade e manutenibilidade.

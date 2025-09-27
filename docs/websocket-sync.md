# Rota WebSocket para sincronismo em tempo real

## Visão geral
Para dar suporte a sincronização em tempo real entre clientes (por exemplo, painéis administrativos e totens de entrada), proponho expor um endpoint WebSocket dedicado a eventos do domínio de tickets. Isso evita o uso de polling constante sobre `/api/tickets` e mantém a API REST existente enxuta.

## Endpoint sugerido
- **Handshake HTTP**: `GET /ws/tickets/stream`
- **Motivação**: O namespace `tickets` já agrupa a maioria dos fluxos que precisam de atualização imediata (criação, conclusão e expiração). Mantê-lo sob `/ws` deixa claro que o canal é de streaming e facilita a configuração de proxies (NGINX, API Gateway, etc.).
- **Autenticação**: reaproveite o JWT atual usando o cabeçalho `Authorization: Bearer <token>` durante o handshake. Bibliotecas como `wscat` e clientes server-side conseguem encaminhar esse header; em navegadores, opte por enviar o token na query string (`/ws/tickets/stream?access_token=...`) e validar no servidor.

## Contrato de mensagens
Envie payloads em JSON com o formato a seguir:

```json
{
  "event": "ticket.created",
  "timestamp": "2024-05-30T14:27:11.125Z",
  "data": {
    "id": "4f9fdb89-bc74-4f84-9181-169c6e1ee3bd",
    "plate": "ABC1D23",
    "status": "Active"
  }
}
```

Eventos esperados:

| Evento                | Disparado por                                      |
| --------------------- | --------------------------------------------------- |
| `ticket.created`      | `POST /api/tickets` após persistência do ticket     |
| `ticket.completed`    | `POST /api/tickets/{id}/complete`                   |
| `ticket.expiringSoon` | job agendado que identifica tickets prestes a expirar |
| `keepalive`           | heartbeat enviado pelo servidor a cada 30 segundos  |

## Sketch de implementação
1. **Program.cs**: habilite o middleware e mapeie a rota.
   ```csharp
   builder.Services.AddSingleton<ITicketUpdateNotifier, TicketUpdateNotifier>();
   app.UseWebSockets();
   app.Map("/ws/tickets/stream", TicketStreamHandler.HandleAsync);
   ```
2. **Handler**: injete `ITicketUpdateNotifier` e gerencie o `WebSocket`.
   ```csharp
   public static async Task HandleAsync(HttpContext context, ITicketUpdateNotifier notifier)
   {
       if (!context.WebSockets.IsWebSocketRequest)
       {
           context.Response.StatusCode = StatusCodes.Status400BadRequest;
           return;
       }

       using var socket = await context.WebSockets.AcceptWebSocketAsync();
       await notifier.SubscribeAsync(socket, context.RequestAborted);
   }
   ```
3. **Broadcast**: no `IParkingTicketService`, publique eventos após `StartParkingAsync` e `CompleteParkingAsync`.

## Passos de teste com `wscat`
### Instalando o `wscat`
- **Node.js**: instale a partir de https://nodejs.org (qualquer versão LTS funciona).
- **Instalação global**: com o Node configurado, rode `npm install -g wscat`. Isso disponibiliza o binário no seu `PATH`.
- **Verificação**: confirme com `wscat --version`. Se estiver usando Windows, lembre-se de abrir o terminal como administrador para a instalação global.

1. Obtenha um token JWT válido via `/api/auth/login`.
2. Suba a aplicação: `dotnet run --project src/Parking.Api`.
3. Conecte-se via WebSocket:
   ```bash
   wscat -c "ws://localhost:5187/ws/tickets/stream" -H "Authorization: Bearer <TOKEN>"
   ```
4. Em outro terminal, crie um ticket:
   ```bash
   http POST :5187/api/tickets plate=ABC1D23 entryAt="2024-05-30T14:21:00Z" "Authorization:Bearer <TOKEN>"
   ```
5. Observe a mensagem `ticket.created` chegar na sessão `wscat`. Repita com `POST /api/tickets/{id}/complete` para validar `ticket.completed`.

### Consumindo mensagens em um cliente web
Se você quiser que o canal seja consumido diretamente por um front-end (por exemplo, React, Vue ou vanilla JS), basta criar uma instância de `WebSocket` apontando para o mesmo endpoint. Um exemplo mínimo em JavaScript:

```javascript
const token = "<TOKEN_JWT>";
const socket = new WebSocket(`ws://localhost:5187/ws/tickets/stream?access_token=${token}`);

socket.onopen = () => {
  console.log("Canal conectado");
};

socket.onmessage = (event) => {
  const payload = JSON.parse(event.data);
  console.log("Evento recebido", payload.event, payload.data);
  // Atualize seu estado/UI a partir daqui
};

socket.onclose = () => {
  console.log("Canal encerrado");
};

socket.onerror = (err) => {
  console.error("Falha no WebSocket", err);
};
```

Frameworks como React ou Angular podem encapsular o código acima em hooks/services para distribuir os eventos entre componentes. No handler ASP.NET, valide o token vindo do query string e promova o contexto de usuário da mesma forma que faria com o header Authorization.

## Considerações de infraestrutura
- **Escalabilidade**: em produção, considere um backplane (Redis) para coordenar múltiplas instâncias.
- **Timeouts**: configure `KeepAliveInterval` para ~15 s no `WebSocketOptions` e um timeout de inatividade para encerrar conexões zumbis.
- **Fallback**: exponha também um endpoint REST que retorne o último estado agregado para clientes que não suportam WebSocket.

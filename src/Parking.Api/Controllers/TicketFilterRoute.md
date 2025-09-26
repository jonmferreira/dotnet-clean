# Ticket filter route

## Implementação
- `POST /api/tickets/filter` recebe um `TicketFilterRequest` com todos os operadores SQL clássicos mapeados para propriedades fortemente tipadas.
- O request é convertido para `ParkingTicketFilter` (camada de domínio) via `TicketFilterMappingExtensions` e a consulta é executada com EF Core em `ParkingTicketRepository.FilterAsync`.
- A configuração `ParkingTicketConfiguration` faz o seeding de seis tickets para que os filtros funcionem de forma imediata no banco em memória.

## JSON
```json
{
  "plateEquals": "XYZ9B76",
  "plateNotEquals": "UVW4E44",
  "plateIn": ["XYZ9B76", "OPQ2C22"],
  "plateNotIn": ["RST3D33"],
  "totalAmountEquals": 45,
  "totalAmountNotEquals": 28,
  "totalAmountGreaterThan": 30,
  "totalAmountGreaterThanOrEqual": 45,
  "totalAmountLessThan": 60,
  "totalAmountLessThanOrEqual": 55,
  "entryAtBetween": {
    "from": "2024-01-01T00:00:00Z",
    "to": "2024-02-01T23:59:59Z"
  },
  "exitAtNotBetween": {
    "from": "2024-01-15T00:00:00Z",
    "to": "2024-01-31T23:59:59Z"
  }
}
```

## Estratégia
- Normalizamos as placas para caixa alta e removemos entradas vazias antes de aplicar filtros `IN`/`NOT IN`.
- Para `BETWEEN` e `NOT BETWEEN` usamos `RangeFilter<T>` garantindo que `from <= to` e evitando filtros inválidos.
- Todos os filtros numéricos verificam `HasValue` para `TotalAmount`, suportando tickets em aberto (`TotalAmount` nulo) ao aplicar `NOT BETWEEN` e `!=`.

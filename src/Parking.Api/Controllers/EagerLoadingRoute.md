# Rota `GET /api/tickets/{ticketId}/inspection-strategies/eager`

A rota exemplifica **Eager Loading**, incluindo a vistoria imediatamente na consulta com `Include`. É ideal para relatórios completos ou integrações que precisam sempre do ticket com a vistoria (por exemplo, exportar para auditoria), evitando round-trips adicionais e garantindo consistência dos dados retornados.

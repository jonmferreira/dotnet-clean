# Rota `GET /api/tickets/{ticketId}/inspection-strategies/explicit`

Aqui utilizamos **Explicit Loading**, buscando o ticket primeiro e carregando a vistoria somente quando a aplicação decide que é necessário. Essa estratégia é útil em cenários em que múltiplas entidades relacionadas podem ser carregadas seletivamente de acordo com regras de negócio (por exemplo, backoffice que analisa várias entidades, mas só busca a vistoria quando o ticket está encerrado).

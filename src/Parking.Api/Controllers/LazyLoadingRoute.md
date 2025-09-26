# Rota `GET /api/tickets/{ticketId}/inspection-strategies/lazy`

Esta rota demonstra **Lazy Loading** para recuperar a vistoria associada a um ticket. A consulta busca apenas o ticket e deixa que o Entity Framework carregue a vistoria sob demanda quando o controlador precisa projetar a resposta. Essa abordagem reduz o custo inicial quando o consumidor pode, eventualmente, ignorar os dados relacionados (por exemplo, um painel que inicialmente mostra apenas os dados do ticket e carrega a vistoria somente quando o usuário expande os detalhes).

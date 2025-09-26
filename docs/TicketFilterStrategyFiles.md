# Estratégia de filtragem de tickets

Este documento descreve a função de cada arquivo envolvido na rota `POST /api/tickets/filter`, destacando como eles colaboram para disponibilizar os operadores (=, !=, <, <=, >, >=, BETWEEN, NOT BETWEEN, IN e NOT IN).

## Camada de API

- **`src/Parking.Api/Controllers/TicketsController.cs`**
  - Expõe o endpoint `POST /api/tickets/filter`, valida o corpo da requisição e converte o resultado do serviço em `ParkingTicketResponse`.
  - Atua como porta de entrada da aplicação, isolando a API das regras de negócio.
- **`src/Parking.Api/Models/Requests/TicketFilterRequest.cs`**
  - Define o contrato que o cliente envia, com propriedades para cada operador (igualdade, desigualdade, comparações, `IN`/`NOT IN` e intervalos).
  - Inclui o record auxiliar `DateRangeFilterRequest` para mapear `BETWEEN` e `NOT BETWEEN`.
- **`src/Parking.Api/Mappings/TicketFilterMappingExtensions.cs`**
  - Normaliza dados sensíveis (placas em caixa alta, remoção de entradas vazias) e converte `TicketFilterRequest` para `ParkingTicketFilter`.
  - Garante que as listas passem pelos filtros corretos e que os intervalos sejam instanciados somente quando válidos.
- **`src/Parking.Api/Controllers/TicketFilterRoute.md`**
  - Documenta a rota, fornece um payload JSON completo de exemplo e detalha a estratégia geral de normalização e validação dos filtros.

## Camada de Aplicação

- **`src/Parking.Application/Abstractions/IParkingTicketService.cs`**
  - Expõe o método `FilterTicketsAsync`, permitindo que a API solicite filtragens sem depender da implementação concreta.
- **`src/Parking.Application/Services/ParkingTicketService.cs`**
  - Implementa `FilterTicketsAsync`, delegando a execução para o repositório e convertendo entidades em DTOs.
  - Centraliza as validações de negócio que pertencem à aplicação antes de atingir a infraestrutura.

## Camada de Domínio

- **`src/Parking.Domain/Repositories/Filters/ParkingTicketFilter.cs`**
  - Representa o filtro tipado utilizado pelos repositórios, separado das preocupações da API.
- **`src/Parking.Domain/Repositories/Filters/RangeFilter.cs`**
  - Modela intervalos genéricos (`BETWEEN` e `NOT BETWEEN`), validando que `From` seja menor ou igual a `To`.
- **`src/Parking.Domain/Repositories/IParkingTicketRepository.cs`**
  - Declara o contrato `FilterAsync`, conectando a camada de domínio à infraestrutura para execução das consultas.

## Camada de Infraestrutura

- **`src/Parking.Infrastructure/Repositories/ParkingTicketRepository.cs`**
  - Traduz `ParkingTicketFilter` em expressões LINQ/EF Core, aplicando todos os operadores suportados.
  - Ordena o resultado e garante que tickets sem `TotalAmount` ou `ExitAt` sejam tratados nos casos de desigualdade e `NOT BETWEEN`.
- **`src/Parking.Infrastructure/Persistence/Configurations/ParkingTicketConfiguration.cs`**
  - Configura o mapeamento EF Core da entidade e faz o seeding de dados de teste para que os filtros sejam exercitados com vários cenários (valores distintos, intervalos e listas).

## Resumo do fluxo

1. A API recebe `TicketFilterRequest` e o converte em `ParkingTicketFilter` via extensão de mapeamento.
2. O serviço de aplicação delega para o repositório através da abstração `IParkingTicketService`.
3. O repositório aplica os filtros no banco (com suporte dos tipos do domínio) e retorna as entidades.
4. O controlador transforma o resultado em respostas HTTP e o documento `TicketFilterRoute.md` serve como referência de uso.

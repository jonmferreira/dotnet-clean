# Parking Clean Architecture Sample

Este repositório demonstra uma API ASP.NET Core estruturada com Clean Architecture para o cadastro e controle de veículos em um estacionamento. O domínio calcula o valor do tempo estacionado com uma política acumulativa de faixas de 15 minutos, 30 minutos, 1h, 2h, 4h e 8h.

## Estrutura de projetos

```
Parking.sln
├── src
│   ├── Parking.Domain           # Entidades de domínio, contratos e serviços (cálculo de tarifas)
│   ├── Parking.Application      # Casos de uso, DTOs e orquestração de serviços
│   ├── Parking.Infrastructure   # Persistência com Entity Framework Core (InMemory)
│   └── Parking.Api              # API REST com endpoints para tickets de estacionamento
└── tests
    └── Parking.UnitTests        # Testes unitários do domínio
```

## Tarifas acumulativas

As tarifas são aplicadas de forma cumulativa sempre que o tempo estacionado ultrapassa um novo limite. A tabela padrão é a seguinte:

| Limite | Valor |
| ------ | ----- |
| 15 minutos | R$ 5,00 |
| 30 minutos | R$ 8,00 |
| 1 hora | R$ 12,00 |
| 2 horas | R$ 20,00 |
| 4 horas | R$ 32,00 |
| 8 horas | R$ 50,00 |

Para períodos superiores a 8 horas o valor de 8 horas é reaplicado em blocos completos adicionais.

## Executando a API

```bash
dotnet build
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Parking.Api
```

A documentação interativa (Swagger) estará disponível em `https://localhost:5001/swagger`.

### Endpoints principais

- `POST /api/tickets` – cria um novo ticket para uma placa.
- `POST /api/tickets/{id}/complete` – encerra o ticket e calcula o valor a pagar.
- `GET /api/tickets` – lista todos os tickets registrados.
- `GET /api/tickets/{id}` – consulta um ticket pelo identificador.
- `GET /api/tickets/active/{plate}` – consulta o ticket ativo de uma placa.
- `POST /api/vehicleinspections` – registra o checklist de inspeção de um ticket existente.
- `PUT /api/vehicleinspections/{id}` – atualiza um checklist já registrado.
- `GET /api/vehicleinspections/{id}` – consulta um checklist específico.
- `GET /api/vehicleinspections/ticket/{ticketId}` – consulta o checklist vinculado a um ticket.

Os checklists registram o estado visual do veículo (arranhões, itens perdidos, chave perdida e batidas fortes). Sempre que algum item for reprovado (`false`), é obrigatório informar a URL da foto de evidência correspondente.

## Testes

```bash
dotnet test
```

Os testes cobrem o cálculo acumulativo de tarifas para garantir a regra de negócio proposta.

# Entity Framework Core no projeto Parking

Este documento resume como o Entity Framework Core (EF Core) é utilizado na solução **Parking** e aponta onde encontrar os principais pontos de integração.

## Estrutura base

- **Contexto**: o `ParkingDbContext` herda de `DbContext` e expõe coleções (`DbSet`) para os agregados principais do domínio: tickets de estacionamento, metas mensais e vistorias de veículos. Ele também aplica automaticamente todas as configurações mapeadas na camada de infraestrutura via `ApplyConfigurationsFromAssembly`. 【F:src/Parking.Infrastructure/Persistence/ParkingDbContext.cs†L6-L24】
- **Configurações de entidade**: cada entidade possui uma classe de configuração dedicada em `Persistence/Configurations`. Essas classes definem chaves primárias, tamanhos máximos, restrições de obrigatoriedade, índices e relacionamentos. 【F:src/Parking.Infrastructure/Persistence/Configurations/ParkingTicketConfiguration.cs†L8-L23】【F:src/Parking.Infrastructure/Persistence/Configurations/MonthlyTargetConfiguration.cs†L8-L20】【F:src/Parking.Infrastructure/Persistence/Configurations/VehicleInspectionConfiguration.cs†L8-L31】

## Configuração de infraestrutura

O método de extensão `AddInfrastructure` encapsula toda a preparação do EF Core:

1. Adiciona o `ParkingDbContext` ao contêiner de injeção de dependência utilizando a opção configurada em `appsettings`. O projeto suporta tanto SQL Server quanto o provedor **InMemory**. Caso o provedor seja definido como `SqlServer`, uma connection string válida é obrigatória; se não houver configuração específica, um banco em memória chamado `ParkingDb` é usado por padrão. 【F:src/Parking.Infrastructure/DependencyInjection.cs†L13-L35】
2. Registra os repositórios concretos (`ParkingTicketRepository`, `MonthlyTargetRepository`, `VehicleInspectionRepository`), que encapsulam o acesso ao `DbContext` e expõem operações assíncronas voltadas ao domínio. 【F:src/Parking.Infrastructure/DependencyInjection.cs†L37-L41】

## Padrão de repositório

Os repositórios da camada de infraestrutura utilizam o `ParkingDbContext` para executar consultas e persistir alterações:

- Os métodos são assíncronos e utilizam `SaveChangesAsync`, `AddAsync` e consultas LINQ (`AsNoTracking`, `OrderByDescending`, `Where`, `FirstOrDefaultAsync`). 【F:src/Parking.Infrastructure/Repositories/ParkingTicketRepository.cs†L16-L52】
- Repositórios adicionais (por exemplo, `MonthlyTargetRepository` e `VehicleInspectionRepository`) seguem o mesmo padrão, garantindo consistência no acesso a dados via EF Core. 【F:src/Parking.Infrastructure/Repositories/MonthlyTargetRepository.cs†L14-L55】【F:src/Parking.Infrastructure/Repositories/VehicleInspectionRepository.cs†L14-L58】

## Testes e ambientes

Graças ao uso do provedor InMemory como padrão, é possível executar testes automatizados sem depender de um banco real. Ao configurar `Database:Provider` como `SqlServer`, o mesmo código é reutilizado para produção, bastando fornecer a connection string apropriada. Essa flexibilidade facilita cenários de desenvolvimento local, pipelines de CI e execução de testes end-to-end com dados efêmeros.

## Próximos passos sugeridos

- Criar migrações e seeders quando for necessário persistir dados em um banco real.
- Centralizar strings de conexão sensíveis em variáveis de ambiente ou Azure Key Vault ao usar SQL Server.
- Avaliar o uso de `AsNoTrackingWithIdentityResolution` e projeções específicas para consultas mais complexas, caso surjam requisitos de performance.

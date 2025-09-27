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
- `GET /api/cnpj/{cnpj}` – consulta dados cadastrais de uma empresa via CNPJa Open API.

Para exemplos de rotas focadas em cenários concorrentes e paralelos, consulte
[`docs/concurrency-parallelism.md`](docs/concurrency-parallelism.md).

Os checklists registram o estado visual do veículo (arranhões, itens perdidos, chave perdida e batidas fortes). Sempre que algum item for reprovado (`false`), é obrigatório informar a URL da foto de evidência correspondente.

## Testes

```bash
dotnet test
```

Os testes cobrem o cálculo acumulativo de tarifas para garantir a regra de negócio proposta.

## Integração com a CNPJa Open API

A seção `Cnpja` no `appsettings.json` define a URL base e o caminho do recurso para consultas de CNPJ.
Caso possua um token de acesso, informe-o no campo `Token` (ou defina a variável de ambiente
`Cnpja__Token`). O endpoint `GET /api/cnpj/{cnpj}` utiliza essas configurações para buscar as
informações da empresa diretamente na API pública.

## Instalando o .NET SDK no container

Os comandos `dotnet build`, `dotnet test` e `dotnet format` dependem do CLI do .NET 8. Caso o
ambiente do container não possua o SDK instalado, siga os passos abaixo (válidos para imagens
baseadas em Debian/Ubuntu):

```bash
# Adiciona o repositório oficial da Microsoft (ajuste a versão se necessário)
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Atualiza a lista de pacotes e instala o SDK do .NET 8
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Confirma a instalação exibindo a versão instalada
dotnet --version  # saída esperada: 8.0.120 ou superior
```

Após concluir esses passos, os comandos de build, testes e formatação estarão disponíveis no
container.

## Executando com Docker

### Imagem da API

O repositório inclui um `Dockerfile` baseado no .NET 8 SDK/ASP.NET que restaura as dependências, publica a aplicação e expõe as portas padrão (`8080` e `8081`). Para gerar a imagem execute:

```bash
docker build -t parking-api .
```

### Subindo API e banco com Docker Compose

O arquivo `docker-compose.yml` define dois serviços:

- **db**: SQL Server 2022 com um _healthcheck_ que aguarda o banco estar pronto antes de liberar a API. A senha padrão (`Your_strong_password123`) deve ser alterada em produção.
- **api**: a aplicação ASP.NET configurada para usar SQL Server quando as variáveis `Database__Provider` e `ConnectionStrings__DefaultConnection` são definidas.

Para subir ambos os serviços execute:

```bash
docker compose up --build
```

A API ficará disponível em `http://localhost:8080` quando o banco estiver saudável.

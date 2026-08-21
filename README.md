# Microservices Project

A small e-commerce-style backend built to practice and demonstrate a .NET microservices
architecture: independent services, an async event bus (RabbitMQ via MassTransit),
per-service databases (Postgres), and a GraphQL search API (HotChocolate).

## Architecture

```
                         ┌─────────────────────┐
   REST  ──────────────▶ │   OrderService.Api   │──── OrderCreated ───┐
                         │  (Postgres: orderdb) │      (RabbitMQ)     │
                         └─────────────────────┘                     │
                                                                      ▼
                         ┌─────────────────────┐        ┌────────────────────────┐
   REST  ──────────────▶ │ InventoryService.Api │◀───────│  NotificationService.Api │
                         │(Postgres: inventorydb)│       │  (consumes OrderCreated) │
                         └─────────────────────┘        └────────────────────────┘

                         ┌─────────────────────┐
  GraphQL ─────────────▶ │   SearchService.Api  │
                         │   (HotChocolate)      │
                         └─────────────────────┘
```

- **OrderService.Api** – REST API for creating/managing orders. Publishes an `OrderCreated`
  event to RabbitMQ whenever a new order is placed.
- **InventoryService.Api** – REST API for products/stock. Consumes `OrderCreated` to log
  inventory-side reactions (a stand-in for real stock decrementing logic).
- **NotificationService.Api** – Consumes `OrderCreated` and simulates sending the customer
  a notification (logged, no real email/SMS provider wired up).
- **SearchService.Api** – GraphQL endpoint (`/graphql`) for product search. Ships with a
  small in-memory sample index; wiring it to a real data source (e.g. consuming inventory
  events into Elasticsearch/Postgres full-text) is a natural next step.
- **Shared.Contracts** – Event contracts (e.g. `OrderCreated`) shared between publishers and
  consumers.
- **OrderService.Infrastructure / InventoryService.Infrastructure** – Placeholders for
  future infrastructure-layer code (repositories, external clients, etc.) per service.

## Tech stack

- .NET 9 / ASP.NET Core Web API
- Entity Framework Core + Npgsql (PostgreSQL)
- MassTransit + RabbitMQ (async messaging)
- HotChocolate (GraphQL)
- Docker & Docker Compose

## Project structure

```
MicroservicesProject/
├── OrderService.Api/            REST API – orders
├── InventoryService.Api/        REST API – products/stock
├── NotificationService.Api/     Event consumer – notifications
├── SearchService.Api/           GraphQL API – product search
├── OrderService.Infrastructure/ Infra placeholder (Order side)
├── InventoryService.Infrastructure/ Infra placeholder (Inventory side)
├── Shared.Contracts/            Shared event contracts
├── db/init-db.sh                Creates the second Postgres database on first boot
├── docker-compose.yml           Postgres + RabbitMQ + all 4 services
└── MicroservicesProject.sln
```

## Running with Docker (recommended)

Requires [Docker](https://docs.docker.com/get-docker/) and Docker Compose (bundled with
Docker Desktop).

```bash
git clone <your-repo-url>
cd MicroservicesProject

docker compose up --build -d
```

This starts Postgres, RabbitMQ, and all four services. Each API applies its own EF Core
migrations automatically on startup, so there's no manual database setup step.

| Service               | URL                                    |
|------------------------|-----------------------------------------|
| OrderService.Api       | http://localhost:5001/swagger           |
| InventoryService.Api   | http://localhost:5002/swagger           |
| NotificationService.Api| http://localhost:5003/health            |
| SearchService.Api      | http://localhost:5004/graphql           |
| RabbitMQ management UI | http://localhost:15672 (guest/guest)    |
| Postgres               | localhost:5432 (postgres/postgres)      |

Try the flow end to end:

```bash
# Create an order (publishes OrderCreated to RabbitMQ)
curl -X POST http://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Jane Doe"}'

# Watch InventoryService.Api / NotificationService.Api container logs — both should
# log that they received the OrderCreated event
docker compose logs -f inventory-service notification-service
```

Stop everything:

```bash
docker compose down          # keep the Postgres volume
docker compose down -v       # also wipe the Postgres volume
```

## Running locally without Docker

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download), a local Postgres instance,
and a local RabbitMQ instance (e.g. `docker run -p 5672:5672 -p 15672:15672 rabbitmq:3-management-alpine`).

```bash
dotnet restore MicroservicesProject.sln
dotnet build MicroservicesProject.sln

dotnet run --project OrderService.Api          # http://localhost:5266
dotnet run --project InventoryService.Api      # http://localhost:5170
dotnet run --project NotificationService.Api   # http://localhost:5193
dotnet run --project SearchService.Api         # http://localhost:5184
```

By default, `appsettings.json` in `OrderService.Api` / `InventoryService.Api` points at
`localhost:5432` with `postgres/postgres`, and `RabbitMq:Host` is `localhost`. Update
those (or use `appsettings.Development.json`, which is machine-specific and gitignored)
to match your local setup.

## Configuration

All settings can be overridden with environment variables (standard ASP.NET Core
configuration binding, `__` as the section separator) — this is how `docker-compose.yml`
points each service at the `postgres` / `rabbitmq` containers instead of `localhost`.

| Variable                                  | Used by                              | Default (appsettings.json)          |
|--------------------------------------------|----------------------------------------|--------------------------------------|
| `ConnectionStrings__DefaultConnection`     | OrderService.Api, InventoryService.Api | `Host=localhost;...;Password=postgres` |
| `RabbitMq__Host`                           | OrderService.Api, InventoryService.Api, NotificationService.Api | `localhost` |
| `RabbitMq__Username` / `RabbitMq__Password`| same as above                          | `guest` / `guest`                    |

> The credentials above are local/demo defaults only (matching the throwaway Postgres and
> RabbitMQ containers in `docker-compose.yml`) — don't reuse them for anything real. In an
> actual deployment these would come from a secrets manager / environment variables, never
> from a committed file.

## API reference

**OrderService.Api** (`/api/orders`)
- `GET /api/orders` – list orders
- `GET /api/orders/{id}` – get one order
- `POST /api/orders` – create an order (publishes `OrderCreated`)
- `PUT /api/orders/{id}` – update an order
- `DELETE /api/orders/{id}` – delete an order

**InventoryService.Api** (`/api/product`)
- `GET /api/product` – list products
- `GET /api/product/{id}` – get one product
- `POST /api/product` – create a product
- `PUT /api/product/{id}` – update a product
- `DELETE /api/product/{id}` – delete a product

**SearchService.Api** (`/graphql`)
```graphql
query {
  products(term: "mouse") {
    id
    name
    quantityAvailable
  }
}
```

**NotificationService.Api**
- `GET /health` – health check
- Consumes `OrderCreated` in the background and logs a simulated notification

## Known limitations / roadmap

This is a learning/portfolio project, not a production system. A few things are
intentionally simplified and would be the next steps in a real build-out:

- SearchService.Api's index is in-memory/sample data rather than fed by real events.
- InventoryService.Api doesn't yet decrement stock when an order is placed.
- No authentication/authorization, API gateway, or distributed tracing yet.
- No automated tests yet — a good next addition (xUnit + WebApplicationFactory per service).

## License

MIT — see [LICENSE](LICENSE).

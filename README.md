# signalrhub-dotnet

.NET 10 Web API that acts as a **SignalR connection hub**. Clients (web, mobile or services) open a
realtime connection authenticated with a JWT, and backends push notifications through a REST API to
**every connection of a client** or to **one specific connection**.

## Architecture

Clean Architecture, with dependencies pointing inwards:

```
signalrhub-dotnet
├── .vscode/                     Build/test/watch tasks and debug configuration
├── SignalRHub.App/              Minimal API endpoints, JWT + API key authentication,
│                                ProblemDetails, global exception handler, OpenAPI + Swagger UI
├── SignalRHub.Application/      Use cases (commands/queries), FluentValidation validators,
│                                delivery strategies, handler decorators,
│                                ports: IConnectionRegistry, INotificationDispatcher
├── SignalRHub.Domain/           Value objects (ClientId, ConnectionId), ClientConnection,
│                                Notification, NotificationTarget, Result pattern, error catalogs
├── SignalRHub.Infrastructure/   SignalR hub and dispatcher adapter, in-memory connection registry
├── SignalRHub.Test/             xUnit + NSubstitute + Shouldly, one folder per layer
├── .dockerignore
├── .gitignore
├── docker-compose.yml           API container with local defaults
├── Dockerfile
├── SignalRHub.slnx
└── README.md
```

Each project declares its namespaces in a `GlobalUsings.cs`.

Design patterns:

- **Ports and adapters**: the application layer defines `IConnectionRegistry` and
  `INotificationDispatcher`; the infrastructure layer implements them.
- **Repository**: `IConnectionRegistry`, implemented by `InMemoryConnectionRegistry`.
- **Strategy + Factory**: `INotificationDeliveryStrategy` (`ClientDeliveryStrategy`,
  `ConnectionDeliveryStrategy`), selected by `DeliveryStrategyResolver` from the `NotificationTarget`.
  A new kind of target (group, broadcast, ...) is a new `NotificationTarget` plus a new strategy,
  without touching the handler.
- **Decorator**: `ValidationCommandHandlerDecorator` and `LoggingCommandHandlerDecorator` wrap every
  command handler (registered with Scrutor).
- **Lightweight CQRS**: own `ICommandHandler<,>` / `IQueryHandler<,>` interfaces, no MediatR.
- **Result**: business errors are values (`Result<T>`), mapped to `ProblemDetails` by the API.
- **Adapter**: `SignalRNotificationDispatcher` turns the dispatcher port into SignalR calls.
- Strongly typed hub (`Hub<INotificationClient>`), Options (validated on start), DI extension methods.

## Connection model

| Concept            | Description                                                                         |
|--------------------|-------------------------------------------------------------------------------------|
| `ClientId`         | Owner of the connections (tenant, user or application). Read from a JWT claim       |
| `ConnectionId`     | One SignalR connection (a browser tab, device or service instance)                  |
| `ClientConnection` | ConnectionId, ClientId, ConnectedAt, UserAgent                                      |

A client can hold any number of connections at the same time. Connections are registered when they
open and removed when they close, so the registry only holds active connections. Both ids are
trimmed and limited to 128 characters.

## Hub

The hub is mapped at `/hubs/notifications` and requires a JWT. Browsers send it in the
`access_token` query string, which SignalR does automatically through `accessTokenFactory`.
Connections without a valid token get `401`, and connections whose token has no client id claim
are aborted.

| Direction       | Method                | Description                                                     |
|-----------------|-----------------------|-----------------------------------------------------------------|
| Client → server | `GetConnectionId()`   | Returns the id of the current connection                        |
| Server → client | `ReceiveNotification` | Delivers a notification `{ id, type, payload, createdAt }`       |

```js
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5108/hubs/notifications", { accessTokenFactory: () => token })
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", notification => console.log(notification.type, notification.payload));
await connection.start();
const connectionId = await connection.invoke("GetConnectionId");
```

## Endpoints

Every `/api` endpoint requires the `X-Api-Key` header with one of the keys in `ApiKeys:Keys`.

| Method | Route                                          | Description                                                  |
|--------|------------------------------------------------|--------------------------------------------------------------|
| POST   | `/api/notifications/clients/{clientId}`        | Sends `{ type, payload }` to every connection of the client  |
| POST   | `/api/notifications/connections/{connectionId}` | Sends `{ type, payload }` to one connection                  |
| GET    | `/api/clients/{clientId}/connections`          | Lists the active connections of the client                   |
| POST   | `/dev/token`                                   | Issues a client JWT for `{ clientId }` (Development only)    |
| GET    | `/health`                                      | Liveness check                                               |

`type` is required (up to 100 characters) and names the event, for example `order.updated`.
`payload` is optional and can be any JSON value; clients receive it as is.

### Response format

Sending a notification returns `200` with the id of the notification and the number of
connections it was delivered to:

```json
{
  "notificationId": "01a19d8c-375b-769b-9563-d42430ed3c79",
  "deliveredConnections": 2,
  "sentAt": "2026-11-02T18:36:14.5559651+00:00"
}
```

Listing connections returns `200` with an array (empty when the client has no connections):

```json
[
  {
    "connectionId": "TeRy0U5HZxl8q1K2eLD_3w",
    "clientId": "client-1",
    "connectedAt": "2026-11-02T18:36:14.4938723+00:00",
    "userAgent": "Microsoft SignalR/10.0 (...)"
  }
]
```

Failures are RFC 9457 `ProblemDetails` with a `code` extension and, for validation failures, an
`errors` dictionary:

```json
{
  "type": "https://httpstatuses.io/404",
  "title": "Not Found",
  "status": 404,
  "detail": "The client 'client-1' has no active connections.",
  "code": "Client.NoConnections"
}
```

| Status | Code                                          | When                                              |
|--------|-----------------------------------------------|---------------------------------------------------|
| `400`  | `General.Validation`, `ClientId.*`, `ConnectionId.*` | Invalid body or route values               |
| `401`  |                                               | Missing or invalid API key                        |
| `404`  | `Client.NoConnections`                        | The client has no active connections              |
| `404`  | `Connection.NotFound`                         | The connection does not exist                     |
| `500`  |                                               | Unexpected error                                  |

## Sequence diagrams

### Connection lifecycle

```mermaid
sequenceDiagram
    autonumber
    participant Client
    participant Hub as NotificationHub
    participant Registry as IConnectionRegistry

    Client->>Hub: Connect /hubs/notifications?access_token=JWT
    alt Missing or invalid token
        Hub-->>Client: 401 Unauthorized
    end
    Hub->>Hub: Read client id claim (Realtime:ClientIdClaim)
    alt Claim missing or invalid
        Hub-->>Client: Abort connection
    end
    Hub->>Registry: RegisterConnectionCommand (ConnectionId, ClientId, UserAgent)
    Hub-->>Client: Connected
    Client->>Hub: GetConnectionId()
    Hub-->>Client: connectionId
    Client-xHub: Disconnect
    Hub->>Registry: UnregisterConnectionCommand (ConnectionId)
```

### Send to a client

`POST /api/notifications/clients/{clientId}`

```mermaid
sequenceDiagram
    autonumber
    participant Backend
    participant API as Hub API
    participant Registry as IConnectionRegistry
    participant Hub as SignalR
    participant Clients as Client connections

    Backend->>API: POST /api/notifications/clients/{clientId} {type, payload}
    alt Missing or invalid API key
        API-->>Backend: 401 Unauthorized
    end
    alt Invalid client id or body
        API-->>Backend: 400 Bad Request
    end
    API->>API: ClientDeliveryStrategy
    API->>Registry: GetByClientAsync(clientId)
    alt No active connections
        API-->>Backend: 404 Not Found (Client.NoConnections)
    end
    API->>Hub: Clients(connectionIds).ReceiveNotification
    Hub->>Clients: ReceiveNotification {id, type, payload, createdAt}
    API-->>Backend: 200 OK (deliveredConnections = N)
```

### Send to a connection

`POST /api/notifications/connections/{connectionId}`

```mermaid
sequenceDiagram
    autonumber
    participant Backend
    participant API as Hub API
    participant Registry as IConnectionRegistry
    participant Hub as SignalR
    participant Connection

    Backend->>API: POST /api/notifications/connections/{connectionId} {type, payload}
    alt Missing or invalid API key
        API-->>Backend: 401 Unauthorized
    end
    alt Invalid connection id or body
        API-->>Backend: 400 Bad Request
    end
    API->>API: ConnectionDeliveryStrategy
    API->>Registry: GetAsync(connectionId)
    alt Connection not registered
        API-->>Backend: 404 Not Found (Connection.NotFound)
    end
    API->>Hub: Clients(connectionId).ReceiveNotification
    Hub->>Connection: ReceiveNotification {id, type, payload, createdAt}
    API-->>Backend: 200 OK (deliveredConnections = 1)
```

### Command pipeline

Every command goes through the decorators before reaching its handler:

```mermaid
sequenceDiagram
    autonumber
    participant Caller as Endpoint / Hub
    participant Logging as LoggingCommandHandlerDecorator
    participant Validation as ValidationCommandHandlerDecorator
    participant Handler as Command handler

    Caller->>Logging: Handle(command)
    Logging->>Validation: Handle(command)
    alt Validation fails
        Validation-->>Logging: Result failure (ValidationError)
    else Valid
        Validation->>Handler: Handle(command)
        Handler-->>Validation: Result
        Validation-->>Logging: Result
    end
    Logging-->>Caller: Result (outcome and duration logged)
```

## Health checks

| Endpoint      | Checks                                 | Used by                     |
|---------------|----------------------------------------|-----------------------------|
| `GET /health` | The API is running (no dependencies)   | Docker healthcheck          |

It answers `Healthy` with `200`. The Docker healthcheck calls it with `curl` (installed in the image)
every 30 seconds.

## Running with Docker

Builds the image and starts the API on port `8080`:

```bash
docker compose up -d --build
```

The image runs in the `Production` environment, so Swagger UI and `/dev/token` are disabled. Set
`ASPNETCORE_ENVIRONMENT=Development` to enable them.

| Variable                 | Default                                         | Setting               |
|--------------------------|-------------------------------------------------|-----------------------|
| `JWT_SIGNING_KEY`        | `local-docker-signing-key-change-me-0123456789` | `Jwt:SigningKey`      |
| `PUBLISHER_API_KEY`      | `docker-api-key-change-me`                      | `ApiKeys:Keys:0`      |
| `ASPNETCORE_ENVIRONMENT` | `Production`                                    | Hosting environment   |

The defaults are only meant for local runs; replace them in any shared environment.

## Running locally

```bash
dotnet run --project SignalRHub.App
```

Or use the `SignalRHub.App` debug configuration in VS Code. In the Development environment Swagger
UI is available at `http://localhost:5108/swagger`, the API key is `dev-api-key-change-me`, and a
client token can be issued with:

```bash
curl -X POST http://localhost:5108/dev/token -H "Content-Type: application/json" -d '{"clientId":"client-1"}'
```

Then send a notification:

```bash
curl -X POST http://localhost:5108/api/notifications/clients/client-1 \
  -H "X-Api-Key: dev-api-key-change-me" -H "Content-Type: application/json" \
  -d '{"type":"order.updated","payload":{"orderId":7}}'
```

## Configuration

`appsettings.json`:

- `Realtime`: `HubPath` (route of the hub, `/hubs/notifications`) and `ClientIdClaim` (JWT claim
  that holds the client id, `sub`).
- `Jwt`: `Issuer`, `Audience` and `SigningKey` (HMAC-SHA256, at least 32 characters) used to
  validate client tokens. `SigningKey` is empty in `appsettings.json` and must be provided per
  environment; `appsettings.Development.json` holds a development key.
- `ApiKeys`: `HeaderName` (`X-Api-Key`) and `Keys`, the keys of the backends allowed to send
  notifications. Keys are compared in constant time.
- `Cors:AllowedOrigins`: browser origins allowed to connect to the hub.

The API does not start when `Jwt` or `ApiKeys` are missing or invalid. Environment variables
override any value using `__` as separator, for example `Jwt__SigningKey` or `ApiKeys__Keys__0`.

## Scaling out

`InMemoryConnectionRegistry` only knows the connections of its own instance. To run several
instances, implement `IConnectionRegistry` on a shared store such as Redis and add the SignalR Redis
backplane (`AddStackExchangeRedis`), so notifications reach connections held by other instances.

## Tests

```bash
dotnet test
```

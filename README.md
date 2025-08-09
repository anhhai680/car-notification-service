# Notification Service

Sends user notifications (e.g., internal, email, push) based on system events. Currently uses in-memory storage.

## Tech Stack
- .NET 7 Web API
- RabbitMQ (planned for event ingestion)
- Swagger/OpenAPI
- Optional persistence (future: MongoDB or PostgreSQL)

## Local URLs
- HTTP: `http://localhost:5031`
- HTTPS: `https://localhost:7232`
- Swagger UI: `/swagger`

## Features
- Create and fetch notifications
- Mark notifications as read
- Planned: listen to events from car-listing-service and order-service
- Planned: integrate with email/push providers

## Requirements
- .NET 7 SDK

## Configuration
- No external dependencies required for in-memory mode
- Future: RabbitMQ connection and optional database connection
- Environment: `ASPNETCORE_ENVIRONMENT=Development` (profiles set this)

## Getting Started
```bash
dotnet restore
dotnet run
```

Open Swagger at `http://localhost:5031/swagger`.

## Data Model
`Notification` DTO shape:

```json
{
  "id": "guid",
  "userId": "string",
  "message": "string",
  "type": "email | push | internal",
  "createdAt": "2024-01-01T00:00:00Z",
  "isRead": false
}
```

## API Endpoints
Base route: `/Notification`

- `GET /Notification` — Get all notifications
- `GET /Notification/{id}` — Get notification by id (Guid)
- `POST /Notification` — Create a new notification
- `PUT /Notification/{id}/read` — Mark as read
- `DELETE /Notification/{id}` — Delete a notification

### Request/Response Examples

Create:
```bash
curl -X POST http://localhost:5031/Notification \
  -H 'Content-Type: application/json' \
  -d '{
    "userId": "user-123",
    "message": "Your order has been paid",
    "type": "internal"
  }'
```

Mark as read:
```bash
curl -X PUT http://localhost:5031/Notification/<guid>/read
```

Delete:
```bash
curl -X DELETE http://localhost:5031/Notification/<guid>
```

## Swagger
Interactive docs at `http://localhost:5031/swagger`.

## Notes & Future Work
- Consume events from RabbitMQ to create notifications
- Optional persistence with MongoDB/PostgreSQL
- Provider integrations (email/push)
- Add authentication/authorization and request validation

## Troubleshooting
- 404s: confirm you are using `/Notification` (capital N) and the correct Guid format
- Swagger not loading: ensure the app is running and you are using the HTTP port above
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

## System Flow Overview

### Event-Driven Architecture
```
External Services → RabbitMQ → Notification Service → Notification Delivery
     ↓              ↓              ↓                    ↓
Car Listing    Message Queue   Process Events    Email/Push/SMS
Order Service  Event Storage   Create & Store   User Devices
```

### Detailed Sequence Diagram Flow

#### 1. External Event Triggers
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Car Listing     │    │ Order Service   │    │ Order Service   │
│ Service         │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │ Car Listed Event      │ Car Purchased Event   │ Order Confirmed Event
         │ {userId, carId,       │ {userId, carId,       │ {userId, orderId,
         │  price, timestamp}    │  orderId, amount,     │  status, timestamp}
         │                       │  timestamp}           │
         ▼                       ▼                       ▼
         ┌─────────────────────────────────────────────────────┐
         │                    RabbitMQ                         │
         │              Message Queue & Storage                │
         └─────────────────────────────────────────────────────┘
```

#### 2. Event Processing & Notification Creation
```
┌─────────────────────────────────────────────────────────────┐
│                Notification Service                         │
├─────────────────────────────────────────────────────────────┤
│ 1. Event Consumer listens to RabbitMQ                     │
│ 2. Parse event type and extract data                       │
│ 3. Create notification object with:                        │
│    - Generated GUID                                        │
│    - User ID from event                                    │
│    - Formatted message based on event type                 │
│    - Current timestamp                                     │
│    - Default unread status                                 │
│ 4. Store notification in storage                           │
└─────────────────────────────────────────────────────────────┘
```

#### 3. Notification Delivery Flow
```
┌─────────────────────────────────────────────────────────────┐
│                Notification Processor                       │
├─────────────────────────────────────────────────────────────┤
│ 1. Determine notification type (email/push/SMS)            │
│ 2. Format content based on event type:                     │
│    • Car Listed: "Your car {carId} has been listed for ${price}" │
│    • Car Purchased: "Congratulations! You've purchased car {carId}" │
│    • Order Confirmed: "Your order {orderId} has been confirmed" │
│ 3. Send via appropriate provider                           │
│ 4. Update delivery status and timestamp                    │
│ 5. Log delivery attempt for monitoring                     │
└─────────────────────────────────────────────────────────────┘
```

#### 4. API Operations Flow
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Client    │    │ Notification    │    │ In-Memory      │
│                 │    │ Controller      │    │ Storage        │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │ GET /Notification     │                       │
         │──────────────────────▶│                       │
         │                       │ Retrieve All           │
         │                       │──────────────────────▶│
         │                       │                       │ List<Notification>
         │                       │◀──────────────────────│
         │ JSON Response         │                       │
         │◀──────────────────────│                       │
         │                       │                       │
         │ POST /Notification    │                       │
         │──────────────────────▶│                       │
         │                       │ Validate & Create     │
         │                       │──────────────────────▶│
         │                       │                       │ Store New
         │                       │                       │◀──────────────────────│
         │ 201 Created +         │                       │
         │ Location Header       │                       │
         │◀──────────────────────│                       │
```

#### 5. Error Handling & Recovery
```
┌─────────────────────────────────────────────────────────────┐
│                    Error Scenarios                         │
├─────────────────────────────────────────────────────────────┤
│ • Event Processing Errors:                                 │
│   - Invalid event format → Log error, skip processing      │
│   - Missing required fields → Log error, skip processing   │
│   - Storage failures → Retry with exponential backoff      │
│                                                             │
│ • Delivery Failures:                                       │
│   - Email provider down → Queue for retry                  │
│   - Push notification failed → Log and retry               │
│   - Rate limiting → Implement backoff strategy             │
│                                                             │
│ • API Errors:                                              │
│   - Validation errors → Return 400 Bad Request             │
│   - Not found → Return 404 Not Found                      │
│   - Server errors → Return 500 Internal Server Error       │
└─────────────────────────────────────────────────────────────┘
```

## API Endpoints
- `GET /Notification` - Get all notifications
- `GET /Notification/{id}` - Get notification by id
- `POST /Notification` - Create a new notification
- `PUT /Notification/{id}/read` - Mark notification as read
- `DELETE /Notification/{id}` - Delete a notification

## Data Model

### Notification Object
```json
{
  "id": "guid",
  "userId": "string",
  "message": "string",
  "type": "email|push|sms",
  "createdAt": "datetime",
  "isRead": "boolean"
}
```

### Event Payloads
```json
// Car Listed Event
{
  "userId": "user123",
  "carId": "car456",
  "price": 25000,
  "timestamp": "2024-01-01T10:00:00Z"
}

// Car Purchased Event
{
  "userId": "user123",
  "carId": "car456",
  "orderId": "order789",
  "amount": 25000,
  "timestamp": "2024-01-01T10:00:00Z"
}
```

## Architecture Components

### Core Services
- **API Gateway**: Routes HTTP requests to controllers
- **Notification Controller**: Handles REST API operations
- **Event Consumer**: Listens to RabbitMQ events
- **Notification Processor**: Processes events and creates notifications

### External Integrations
- **RabbitMQ**: Message queue for event consumption
- **Email Provider**: SMTP service for email notifications
- **Push Provider**: FCM/APNS for mobile push notifications

### Storage
- **Current**: In-memory storage using `List<Notification>`
- **Planned**: Database integration (MongoDB/PostgreSQL)
- **Future**: Redis for caching and performance

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

## Future Enhancements
- [ ] Database persistence layer
- [ ] RabbitMQ event consumer implementation
- [ ] Email service integration
- [ ] Push notification service integration
- [ ] Notification templates and localization
- [ ] Retry mechanisms for failed deliveries
- [ ] Metrics and monitoring
- [ ] Rate limiting and throttling

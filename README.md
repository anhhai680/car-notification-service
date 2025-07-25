# Notification Service

This service sends notifications (email or internal) to users based on events from other services.

## Tech Stack
- .NET 8 Web API
- RabbitMQ (or Kafka)
- MongoDB or PostgreSQL (optional, for persistence)

## Features
- Sends notifications when:
  - Car is listed successfully
  - Car is purchased
  - Order is confirmed/paid
- Listens to events from car-listing-service and order-service (TODO)
- Can integrate with email/push notification providers

## Getting Started
1. Install .NET 8 SDK
2. Configure RabbitMQ (or Kafka) connection in `appsettings.json` (TODO)
3. (Optional) Configure database in `appsettings.json`
4. Run the service:
   ```bash
   dotnet run
   ```

## API Endpoints
- `GET /Notification` - Get all notifications
- `GET /Notification/{id}` - Get notification by id
- `POST /Notification` - Create a new notification
- `PUT /Notification/{id}/read` - Mark notification as read
- `DELETE /Notification/{id}` - Delete a notification

## API Documentation
Swagger UI available at `/swagger` when running locally. 
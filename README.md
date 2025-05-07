# MediaLocator

MediaLocator is a comprehensive web application that enables users to search, view, and download openly licensed media content (audio and images) through integration with the OpenVerse API. The application features a modern, responsive interface with robust user authentication and media management capabilities.

## Features

- **Media Search**: Search for audio files and images with advanced filtering options
- **Media Preview**: Play audio files and view images directly in the application
- **Save to History**: Track searched and viewed media in personal history
- **Download Capabilities**: Download media files directly from the application
- **User Authentication**: Secure registration, login, and account management
- **Role-Based Access**: Admin and standard user permissions
- **Responsive Design**: Works across desktop and mobile devices

## Architecture

The project follows Clean Architecture principles with a layered approach:

- **Domain Layer**: Core entities, enums, exceptions, and domain logic
- **Application Layer**: Business logic, CQRS commands/queries with MediatR
- **Infrastructure Layer**: External service implementations and data access
- **API Layer**: REST API endpoints for the frontend
- **Web Layer**: Blazor WebAssembly frontend application
- **Service Layer**: Background processing services

## Technology Stack

### Backend
- **ASP.NET Core 9.0**: Web API framework
- **Entity Framework Core**: ORM for database access
- **PostgreSQL**: Primary database
- **MediatR**: CQRS implementation
- **FluentValidation**: Request validation
- **MassTransit**: Message bus abstraction
- **RabbitMQ**: Message queue for event processing
- **Redis**: Hybrid caching
- **JWT**: Token-based authentication
- **Identity**: User management

### Frontend
- **Blazor WebAssembly**: Client-side web framework
- **Bootstrap 5**: UI framework
- **Bootstrap Icons**: UI icons
- **Custom CSS**: Enhanced styling

### DevOps & Monitoring
- **Docker**: Containerisation
- **Docker Compose**: Local development environment
- **GitHub Actions**: CI/CD pipelines
- **OpenTelemetry**: Distributed tracing
- **Jaeger**: Trace visualisation
- **Health Checks**: System monitoring

### Health Monitoring

The application implements comprehensive health checks to monitor system status:

- **/health**: Overall system health status
- **/health/ready**: Readiness checks for service dependencies
- **/health/live**: Liveness probe for container orchestration
- **/health/db**: Database connection health

These endpoints return detailed health information in a standardised format, facilitating integration with monitoring tools and container orchestration platforms.

### Testing
- **NUnit**: Unit testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library
- **Functional Tests**: Testing application behavior
- **Integration Tests**: Testing component interactions
- **Acceptance Tests**: End-to-end testing

## Additional Features

### Authentication & Security
- **Email Verification**: Two-step verification process for new accounts
- **Password Management**: Secure password reset flow with email verification
- **Token Refresh**: Automatic JWT token refresh for seamless authentication
- **Password History**: Prevents reuse of previous passwords
- **Account Lockout**: Temporary account lockout after failed login attempts

### Admin Capabilities
- **User Management**: View, activate, deactivate, and delete user accounts
- **Access Control**: Manage user roles and permissions
- **Search History Monitoring**: View and analyse user search patterns

### Performance Optimisation
- **Caching**: Redis-based caching for improved response times
- **Response Compression**: Optimised payload delivery
- **API Versioning**: Support for multiple API versions

### Notification System
- **Email Notifications**: Transactional emails for account events
- **Event-Driven Architecture**: Using RabbitMQ for asynchronous processing

### System Resilience
- **Circuit Breaker Pattern**: Prevents cascading failures
- **Retry Policies**: Automatic retry for transient errors
- **Distributed Tracing**: Request flow visibility across services

## Project Structure

```
MediaLocator/
├── src/
│   ├── main/
│   │   ├── ML.Api/            # Web API controllers and configuration
│   │   ├── ML.Application/    # Business logic and CQRS handlers
│   │   ├── ML.Domain/         # Domain entities and interfaces
│   │   ├── ML.Infrastructure/ # External services implementation
│   │   ├── ML.Service/        # Background services
│   │   └── ML.Web/            # Blazor WebAssembly frontend
│   └── tests/
│       ├── ML.AcceptanceTests/
│       ├── ML.Application.FunctionalTests/
│       └── ML.IntegrationTests/
├── docker/                    # Docker configuration
│   ├── docker-compose.yml
│   └── .env
└── .github/
    └── workflows/             # CI/CD pipelines
```

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Docker and Docker Compose
- Node.js (for playwright)

### Running the Application
1. Clone the repository
2. Navigate to the docker directory
3. Create a `.env` file with required environment variables
4. Run `docker-compose up -d`
5. Access the web application at `http://localhost:5000`

### Environment Variables
- `POSTGRES_CONNECTION_STRING`: PostgreSQL connection string
- `REDIS_CONNECTION_STRING`: Redis connection string
- `JWT_SECRET`: Secret key for JWT token generation
- `RABBITMQ_HOST`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD`: RabbitMQ configuration
- `OPENVERSE_CLIENT_ID`, `OPENVERSE_CLIENT_SECRET`: OpenVerse API credentials

## Development Guidelines
- **API Versioning**: All endpoints are on v1 for now
- **Authentication**: JWT tokens with refresh mechanism
- **Error Handling**: Standardised error responses
- **Validation**: Request validation with FluentValidation
- **Testing**: Unit, integration and acceptance tests
- **Documentation**: API documentation with Swagger/OpenAPI

## License

MIT License

Copyright (c) 2025 Isaac C. Helge Oselukwue

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
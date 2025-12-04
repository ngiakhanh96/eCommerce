# eCommerce Microservices Platform

A microservices-based eCommerce platform built with **.NET 10**, **Domain-Driven Design (DDD)**, **CQRS**, and **Event-Driven Architecture** using Apache Kafka.

## 📋 Table of Contents

- [Architecture Overview](#-architecture-overview)
- [Technology Stack](#️-technology-stack)
- [Services](#-services)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Event-Driven Communication](#-event-driven-communication)
- [Design Decisions](#-design-decisions)
- [Testing Strategy](#-testing-strategy)
- [AI Usage Documentation](#-ai-usage-documentation)
- [Project Structure](#-project-structure)
- [Observability Features](#-observability-features)
- [Resilience Patterns](#-resilience-patterns)
- [Configuration Management](#️-configuration-management)
- [Future Improvements](#-future-improvements)
- [Author](#-author)

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           .NET Aspire AppHost                           │
│                    (Orchestration & Service Discovery)                  │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────┐          ┌────────────────┐          ┌───────────────┐
│  UserService  │          │    Kafka       │          │ OrderService  │
│               │◄────────►│                │◄────────►│               │
├───────────────┤          ├────────────────┤          ├───────────────┤
│ • Create User │          │ Topics:        │          │ • Create Order│
│ • Get User    │          │ • user-created │          │ • Get Order   │
│ • Get Orders  │          │ • order-created│          │               │
└───────┬───────┘          └────────────────┘          └───────┬───────┘
        │                                                      │
        ▼                                                      ▼
┌───────────────┐                                      ┌───────────────┐
│ UserDbContext │                                      │ OrderDbContext│
│ (In-Memory DB)│                                      │ (In-Memory DB)│
│ • Users       │                                      │ • Orders      │
│ • RefOrders   │                                      │ • RefUsers    │
└───────────────┘                                      └───────────────┘
```

### Key Architectural Patterns

| Pattern                       | Implementation                                          |
| ----------------------------- | ------------------------------------------------------- |
| **Domain-Driven Design**      | Aggregate roots, entities, value objects, repositories  |
| **CQRS**                      | Separate command and query handlers via custom Mediator |
| **Event-Driven Architecture** | Kafka for async inter-service communication             |
| **Repository Pattern**        | Abstraction over data access layer                      |
| **Clean Architecture**        | Domain → Application → Infrastructure layers            |

---

## 🛠️ Technology Stack

| Layer                | Technology                        | Version |
| -------------------- | --------------------------------- | ------- |
| **Framework**        | .NET Core                         | 10.0    |
| **ORM**              | Entity Framework Core (In-Memory) | 10.0    |
| **Messaging**        | Apache Kafka (Confluent.Kafka)    | -       |
| **Orchestration**    | .NET Aspire                       | 13.0.1  |
| **Containerization** | Docker & Docker Compose           | -       |
| **Observability**    | OpenTelemetry, Aspire Dashboard   | -       |

---

## 📦 Services

### UserService

Manages user accounts and maintains a local copy of orders (RefOrders) via events.

**Endpoints:**

| Method | Endpoint             | Description               |
| ------ | -------------------- | ------------------------- |
| `POST` | `/users`             | Create a new user         |
| `GET`  | `/users/{id}`        | Get user by ID            |
| `GET`  | `/users/{id}/orders` | Get all orders for a user |

**Domain Model:**

- `User` (Aggregate Root): id, name, email, createdAt
- `RefOrder` (Reference Model): Referenced order data from OrderService. **Note:** Reference models are NOT aggregate roots in DDD - they are read-only copies of data from other bounded contexts maintained for query purposes.

### OrderService

Handles order creation and management. Validates user existence before order creation.

**Endpoints:**

| Method | Endpoint       | Description        |
| ------ | -------------- | ------------------ |
| `POST` | `/orders`      | Create a new order |
| `GET`  | `/orders/{id}` | Get order by ID    |

**Domain Model:**

- `Order` (Aggregate Root): id, userId, product, quantity, price, createdAt
- `RefUser` (Reference Model): Referenced user data from UserService. **Note:** Reference models are NOT aggregate roots in DDD - they are read-only copies of data from other bounded contexts maintained for query purposes.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- IDE: Visual Studio / VS Code / Rider

### Option 1: Run with Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/ngiakhanh96/eCommerce.git
cd eCommerce

# Build and run all services
docker-compose up --build

# Or run in detached mode
docker-compose up -d --build
```

**Service URLs:**

| Service          | URL                    |
| ---------------- | ---------------------- |
| UserService      | http://localhost:55453 |
| OrderService     | http://localhost:54987 |
| Aspire Dashboard | http://localhost:18888 |

### Option 2: Run with .NET Aspire (For Development)

```bash
# Clone the repository
git clone https://github.com/ngiakhanh96/eCommerce.git
cd eCommerce

# Run the Aspire AppHost
dotnet run --project eCommerce.AppHost
```

### Verify Services are Running

```bash
# Check health (returns "Healthy")
curl http://localhost:55453/health
curl http://localhost:54987/health

# Check liveness probe
curl http://localhost:55453/alive
curl http://localhost:54987/alive
```

---

## 📖 API Documentation

Complete API documentation is available in **[API.md](API.md)**, including:

- All endpoints with request/response examples
- Validation rules and error responses
- Event schemas for Kafka messages
- Testing examples (cURL and PowerShell)

### Quick Reference

| Service           | Endpoints                                                  |
| ----------------- | ---------------------------------------------------------- |
| **User Service**  | `POST /users`, `GET /users/{id}`, `GET /users/{id}/orders` |
| **Order Service** | `POST /orders`, `GET /orders/{id}`                         |

### Quick Testing Examples

Follow this sequence to test the full event-driven flow:

| Step | Action                          | Endpoint                 | Expected Result                                                                                           |
| ---- | ------------------------------- | ------------------------ | --------------------------------------------------------------------------------------------------------- |
| 1️⃣   | Create order with random userId | `POST /orders`           | ❌ **Fails** - User doesn't exist                                                                         |
| 2️⃣   | Create a new user               | `POST /users`            | ✅ **Success** - User created, `UserCreated` event published                                              |
| 3️⃣   | Get the created user            | `GET /users/{id}`        | ✅ **Success** - Returns user details                                                                     |
| 4️⃣   | Get user's orders               | `GET /users/{id}/orders` | ✅ **Success** - Returns empty list `[]`                                                                  |
| 5️⃣   | Create order with valid userId  | `POST /orders`           | ✅ **Success** - OrderService already received `UserCreated` event and now publishes `OrderCreated` event |
| 6️⃣   | Get user's orders again         | `GET /users/{id}/orders` | ✅ **Success** - UserService already received the order (via `OrderCreated` event)                        |
| 7️⃣   | Get order details               | `GET /orders/{id}`       | ✅ **Success** - Returns order details                                                                    |

---

## 📨 Event-Driven Communication

### Integration Events

| Event                          | Publisher    | Consumer     | Topic           |
| ------------------------------ | ------------ | ------------ | --------------- |
| `UserCreatedIntegrationEvent`  | UserService  | OrderService | `user-created`  |
| `OrderCreatedIntegrationEvent` | OrderService | UserService  | `order-created` |

### Event Flow

```
1. User creates account → UserService publishes UserCreatedIntegrationEvent
   └─► OrderService receives event → Creates RefUser record

2. User places order → OrderService publishes OrderCreatedIntegrationEvent
   └─► UserService receives event → Creates RefOrder record
```

### EventBus Architecture

- **Publisher**: `KafkaEventPublisher` - Serializes events to JSON and publishes to Kafka topics
- **Subscriber**: `KafkaEventSubscriber` - Consumes messages and routes to appropriate handlers
- **Handlers**: `BaseEventHandler<TEvent>` - Abstract base for type-safe event handling

---

## 🎯 Design Decisions

### 1. Why Custom Mediator over MediatR?

While MediatR is the industry standard for CQRS patterns, a custom implementation was chosen for this project:

**Cost & Licensing Considerations:**

- MediatR transitioned to dual licensing (free community + premium commercial)
- Custom implementation eliminates dependency on external commercial packages
- Reduces licensing complexity for enterprise adoption

**Educational & Transparency:**

- Demonstrates deep understanding of CQRS pattern mechanics
- Shows ability to implement core architectural patterns from scratch
- Provides complete visibility into command/query routing logic

### 2. Why .NET Aspire?

.NET Aspire is Microsoft's modern platform for building distributed cloud-native applications with .NET. For this project, it provides several critical advantages:

**Development Experience:**

- **Single Command Startup**: `dotnet run --project eCommerce.AppHost` starts all services, infrastructure, and orchestration
- **Service Discovery**: Automatic DNS resolution between services (no manual configuration)
- **Environment Consistency**: Development environment mirrors production setup

**Built-in Observability:**

- **OpenTelemetry Integration**: Distributed tracing, metrics, and logs automatically instrumented
- **Aspire Dashboard**: Real-time visualization of services, traces, logs, and performance metrics
- **Health Checks**: Automatic `/health` and `/alive` endpoints configured per service
- **Structured Logging**: JSON-formatted logs for aggregation and analysis

**Production Readiness:**

- **Cloud-Native by Default**: Designed for containerized and Kubernetes deployments
- **Resource Management**: Built-in patterns for resilience, retry policies, and circuit breakers
- **Extensible**: Pluggable components for databases, caches, queues, and external services

**Educational Value:**

- Demonstrates understanding of modern .NET distributed systems patterns
- Shows practical application of cloud-native principles
- Aligns with industry best practices and Microsoft's direction

---

## 🧪 Testing Strategy

### Test Projects Structure

```
eCommerce.Tests/
├── eCommerce.OrderService.Tests/      # OrderService tests (72 tests)
├── eCommerce.UserService.Tests/       # UserService tests (79 tests)
├── eCommerce.EventBus.Tests/          # EventBus component tests (28 tests)
├── eCommerce.Mediator.Tests/          # Mediator component tests (31 tests)
└── eCommerce.ServiceDefaults.Tests/   # ServiceDefaults middleware tests (30 tests)
```

### Testing Approach

The project includes comprehensive unit tests covering all critical components to ensure code quality and reliability.

| Layer               | Test Type  | Coverage Focus                                                           |
| ------------------- | ---------- | ------------------------------------------------------------------------ |
| **Domain**          | Unit Tests | Aggregate roots, entity creation, immutability, factory methods          |
| **Application**     | Unit Tests | Command/Query handlers, validation pipelines, DTOs mapping               |
| **Infrastructure**  | Unit Tests | Repository implementations, event handlers, integration events           |
| **EventBus**        | Unit Tests | Event publishing, resilience policies (retry, circuit breaker), handlers |
| **Mediator**        | Unit Tests | Command/Query buses, validating decorators, validation exceptions        |
| **ServiceDefaults** | Unit Tests | Global exception handling middleware, request logging middleware         |

### Running Tests

```bash
# Run all tests
dotnet test eCommerce.slnx

# Run tests for a specific project
dotnet test eCommerce.EventBus.Tests
dotnet test eCommerce.Mediator.Tests
dotnet test eCommerce.OrderService.Tests
dotnet test eCommerce.UserService.Tests
dotnet test eCommerce.ServiceDefaults.Tests
```

**Quality Benefits:**

- ✅ **Regression Prevention**: Tests catch breaking changes early
- ✅ **Refactoring Confidence**: Safe to modify code with test coverage
- ✅ **Documentation**: Tests serve as executable specifications
- ✅ **Design Feedback**: Testable code promotes better architecture

---

## 🤖 AI Usage Documentation

This project was developed with assistance from **GitHub Copilot** (Claude):

### Areas Where AI Assisted

1. **Code Review**: AI reviewed the codebase and identified missing components
2. **Documentation**: README structure and content suggestions
3. **Test Generation**: Unit test scaffolding and test case identification
4. **Best Practices**: Suggestions for resilience patterns, logging, validation

### AI Usage Philosophy

- AI was used as a **pair programming partner**, not a replacement for understanding
- All generated code was **reviewed and understood** before integration
- AI suggestions were **adapted** to fit the project's specific architecture

---

## 📁 Project Structure

```
eCommerce/
├── eCommerce.AppHost/           # .NET Aspire orchestration
├── eCommerce.ServiceDefaults/   # Shared service configurations
├── eCommerce.Mediator/          # Custom CQRS implementation
├── eCommerce.EventBus/          # Kafka event publishing/subscribing
├── eCommerce.OrderService/      # Order management microservice
│   ├── Application/             # Commands, Queries, DTOs
│   ├── Domain/
│   │   ├── AggregatesModel/     # Aggregate roots (Order)
│   │   └── References/          # Reference models (RefUser)
│   └── Infrastructure/          # EF Core, repositories
├── eCommerce.UserService/       # User management microservice
│   ├── Application/             # Commands, Queries, DTOs
│   ├── Domain/
│   │   ├── AggregatesModel/     # Aggregate roots (User)
│   │   └── References/          # Reference models (RefOrder)
│   └── Infrastructure/          # EF Core, repositories
├── docker-compose.yml           # Production Docker setup
├── README.md                    # This file
└── API.md                       # API documentation
```

---

## ✅ Observability Features

- **Health Checks**: `/health` and `/alive` endpoints on each service
- **OpenTelemetry**: Distributed tracing across services
- **Aspire Dashboard**: Real-time logs, traces, and metrics visualization
- **Structured Logging**: JSON-formatted logs for aggregation
- **Request Logging Middleware**: Captures request/response timing, status codes, and metadata

---

## 🔄 Resilience Patterns

The EventBus implements robust resilience patterns using **Polly**:

### Retry Policy (Exponential Backoff)

- 3 retry attempts with exponential delays (1s, 2s, 4s)
- Handles transient failures gracefully

### Circuit Breaker

- Opens after 5 consecutive failures, stays open for 30 seconds
- Prevents cascade failures during outages

---

## ⚙️ Configuration Management

### Environment-Specific Settings

| Environment     | Log Level Default | Consumer Group Name              |
| --------------- | ----------------- | -------------------------------- |
| **Development** | Information       | user-service-consumer-group      |
| **Production**  | Warning           | user-service-prod-consumer-group |

### Configuration Files

```
OrderService/
├── appsettings.json              # Base configuration
├── appsettings.Development.json  # Dev overrides
└── appsettings.Production.json   # Prod overrides
```

---

## 🚀 Future Improvements

The following enhancements would further strengthen the architecture for production readiness:

### Reliability & Consistency

| Pattern            | Description                                                            | Benefit                                                  |
| ------------------ | ---------------------------------------------------------------------- | -------------------------------------------------------- |
| **Idempotency**    | Add idempotency keys to POST endpoints to prevent duplicate processing | Safely retry failed requests without side effects        |
| **Outbox Pattern** | Store events in local DB before publishing to Kafka                    | Guarantee at least once delivery, prevent message loss   |
| **Saga Pattern**   | Implement distributed transactions across services                     | Handle complex multi-service workflows with compensation |

### Observability & Debugging

| Feature                             | Description                                        | Benefit                                            |
| ----------------------------------- | -------------------------------------------------- | -------------------------------------------------- |
| **Distributed Tracing Correlation** | Propagate correlation IDs through Kafka messages   | End-to-end request tracing across async boundaries |
| **Metrics Dashboard**               | Add Prometheus/Grafana for custom business metrics | Real-time monitoring and alerting                  |
| **Structured Error Tracking**       | Integrate with Sentry or Application Insights      | Centralized error aggregation and analysis         |

### Security & Performance

| Feature                          | Description                               | Benefit                                      |
| -------------------------------- | ----------------------------------------- | -------------------------------------------- |
| **Authentication/Authorization** | Add JWT-based auth with role-based access | Secure API endpoints                         |
| **Rate Limiting**                | Implement request throttling per client   | Prevent abuse and ensure fair usage          |
| **Caching**                      | Add Redis for frequently accessed data    | Reduce database load, improve response times |
| **API Versioning**               | Implement URL or header-based versioning  | Backward compatibility for API consumers     |

### Data & Infrastructure

| Feature                   | Description                                  | Benefit                                |
| ------------------------- | -------------------------------------------- | -------------------------------------- |
| **Persistent Database**   | Replace In-Memory with PostgreSQL/SQL Server | Production-ready data persistence      |
| **Event Sourcing**        | Store all state changes as events            | Complete audit trail, temporal queries |
| **CQRS Read Models**      | Separate optimized read databases            | Better query performance at scale      |
| **Kubernetes Deployment** | Add Helm charts for K8s deployment           | Container orchestration for production |

---

## 👤 Author

**Khanh Nguyen**

- GitHub: [@ngiakhanh96](https://github.com/ngiakhanh96)

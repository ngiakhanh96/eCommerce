# 📖 API Documentation

This document provides comprehensive API documentation for the eCommerce microservices platform.

## Table of Contents

- [Overview](#overview)
- [Base URLs](#base-urls)
- [Authentication](#authentication)
- [Error Handling](#error-handling)
  - [Standard Error Response](#standard-error-response)
  - [Validation Error Response](#validation-error-response)
  - [HTTP Status Codes](#http-status-codes)
- [User Service API](#user-service-api)
  - [Health Checks](#health-checks)
  - [Create User](#create-user)
  - [Get User by ID](#get-user-by-id)
  - [Get User Orders](#get-user-orders)
- [Order Service API](#order-service-api)
  - [Health Checks](#health-checks-1)
  - [Create Order](#create-order)
  - [Get Order by ID](#get-order-by-id)

---

## Overview

The eCommerce platform consists of two microservices that communicate via REST APIs and Kafka events:

| Service           | Description           | Technology                        |
| ----------------- | --------------------- | --------------------------------- |
| **User Service**  | Manages user accounts | ASP.NET Core, EF Core (In-Memory) |
| **Order Service** | Manages orders        | ASP.NET Core, EF Core (In-Memory) |

---

## Base URLs

| Environment | User Service              | Order Service             |
| ----------- | ------------------------- | ------------------------- |
| Development | `http://localhost:55453/` | `http://localhost:54987/` |

---

## Authentication

Currently, the APIs do not require authentication. This is suitable for demonstration purposes.

---

## Error Handling

All errors follow the [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807) standard.

### Standard Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "instance": "/users",
  "traceId": "00-61c7dfa3fb129067caee139b7a11c102-ca95a47f17410bb5-01",
  "timestamp": "2025-11-29T10:30:00.0000000Z"
}
```

### Validation Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/users",
  "errors": {
    "Name": ["Name is required."]
  },
  "traceId": "00-61c7dfa3fb129067caee139b7a11c102-ca95a47f17410bb5-01",
  "timestamp": "2025-11-29T12:17:03.9001492Z"
}
```

### HTTP Status Codes

| Code  | Description           | When Used                            |
| ----- | --------------------- | ------------------------------------ |
| `200` | OK                    | Successful GET request               |
| `201` | Created               | Successful POST request              |
| `400` | Bad Request           | Validation errors, invalid arguments |
| `404` | Not Found             | Resource not found                   |
| `500` | Internal Server Error | Unexpected server errors             |

---

## User Service API

Base path: `/`

### Health Checks

#### `GET /health`

Readiness probe - checks if the service is ready to accept traffic.

**Response** `200 OK`

```
Healthy
```

#### `GET /alive`

Liveness probe - checks if the service is alive and responsive.

**Response** `200 OK`

```
Healthy
```

> **Note**: Health check endpoints are only available in Development environment.

---

### Create User

#### `POST /users`

Creates a new user account and publishes a `UserCreated` event to Kafka.

**Request Headers**

| Header       | Value              |
| ------------ | ------------------ |
| Content-Type | `application/json` |

**Request Body**

| Field   | Type   | Required | Description          | Validation         |
| ------- | ------ | -------- | -------------------- | ------------------ |
| `name`  | string | ✅       | User's full name     | 1-100 characters   |
| `email` | string | ✅       | User's email address | Valid email format |

**Example Request**

```json
{
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

**Response** `201 Created`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

**Error Responses**

| Status | Description                               |
| ------ | ----------------------------------------- |
| `400`  | Validation failed (invalid name or email) |
| `500`  | Internal server error                     |

**Example Validation Error**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/users",
  "errors": {
    "Name": ["Name is required."]
  },
  "traceId": "00-61c7dfa3fb129067caee139b7a11c102-ca95a47f17410bb5-01",
  "timestamp": "2025-11-29T12:17:03.9001492Z"
}
```

**Side Effects**

- Publishes `UserCreated` event to Kafka topic `user-events`

---

### Get User by ID

#### `GET /users/{id}`

Retrieves a user by their unique identifier.

**Path Parameters**

| Parameter | Type | Description            |
| --------- | ---- | ---------------------- |
| `id`      | GUID | Unique user identifier |

**Example Request**

```
GET /users/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response** `200 OK`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

**Error Responses**

| Status | Description           |
| ------ | --------------------- |
| `404`  | User not found        |
| `500`  | Internal server error |

**Example Not Found Error**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "User with ID 'bb8c9e5a-428f-4015-88ac-1559a2dcc26f' not found.",
  "instance": "/users/bb8c9e5a-428f-4015-88ac-1559a2dcc26f",
  "traceId": "00-4519aeff2c6f67434f3f13c66113951d-a2dfeef4e4d44ca7-01",
  "timestamp": "2025-11-29T12:31:46.0702194Z"
}
```

---

### Get User Orders

#### `GET /users/{id}/orders`

Retrieves all orders placed by a specific user. This data is synchronized from the Order Service via Kafka events.

**Path Parameters**

| Parameter | Type | Description            |
| --------- | ---- | ---------------------- |
| `id`      | GUID | Unique user identifier |

**Example Request**

```
GET /users/3fa85f64-5717-4562-b3fc-2c963f66afa6/orders
```

**Response** `200 OK`

```json
[
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "product": "iPhone 15 Pro",
    "quantity": 2,
    "price": 1999.99
  },
  {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "product": "MacBook Pro",
    "quantity": 1,
    "price": 2499.99
  }
]
```

**Error Responses**

| Status | Description           |
| ------ | --------------------- |
| `500`  | Internal server error |

**Example Internal Server Error**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "instance": "/users/bb8c9e5a-428f-4015-88ac-1559a2dcc26f/orders",
  "traceId": "00-4519aeff2c6f67434f3f13c66113951d-a2dfeef4e4d44ca7-01",
  "timestamp": "2025-11-29T12:31:46.0702194Z"
}
```

> **Note**: Returns an empty array `[]` if the user has no orders.

---

## Order Service API

Base path: `/`

### Health Checks

#### `GET /health`

Readiness probe - checks if the service is ready to accept traffic.

**Response** `200 OK`

```
Healthy
```

#### `GET /alive`

Liveness probe - checks if the service is alive and responsive.

**Response** `200 OK`

```
Healthy
```

> **Note**: Health check endpoints are only available in Development environment.

---

### Create Order

#### `POST /orders`

Creates a new order and publishes an `OrderCreated` event to Kafka.

**Request Headers**

| Header       | Value              |
| ------------ | ------------------ |
| Content-Type | `application/json` |

**Request Body**

| Field      | Type    | Required | Description                      | Validation            |
| ---------- | ------- | -------- | -------------------------------- | --------------------- |
| `userId`   | GUID    | ✅       | ID of the user placing the order | Valid GUID, non-empty |
| `product`  | string  | ✅       | Product name                     | 1-255 characters      |
| `quantity` | integer | ✅       | Number of items                  | Greater than 0        |
| `price`    | decimal | ✅       | Total price                      | Greater than 0        |

**Example Request**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "product": "iPhone 15 Pro",
  "quantity": 2,
  "price": 1999.99
}
```

**Response** `201 Created`

```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "product": "iPhone 15 Pro",
  "quantity": 2,
  "price": 1999.99
}
```

**Error Responses**

| Status | Description           |
| ------ | --------------------- |
| `400`  | Validation failed     |
| `404`  | User not found        |
| `500`  | Internal server error |

**Example Validation Error**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/orders",
  "errors": {
    "Quantity": ["Quantity must be greater than 0."]
  },
  "traceId": "00-eb45822861656cfd3073afecba59ee27-2615416f715f9306-01",
  "timestamp": "2025-11-29T12:36:34.6152979Z"
}
```

**Side Effects**

- Publishes `OrderCreated` event to Kafka topic `order-events`

---

### Get Order by ID

#### `GET /orders/{id}`

Retrieves an order by its unique identifier.

**Path Parameters**

| Parameter | Type | Description             |
| --------- | ---- | ----------------------- |
| `id`      | GUID | Unique order identifier |

**Example Request**

```
GET /orders/4fa85f64-5717-4562-b3fc-2c963f66afa7
```

**Response** `200 OK`

```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "product": "iPhone 15 Pro",
  "quantity": 2,
  "price": 1999.99
}
```

**Error Responses**

| Status | Description           |
| ------ | --------------------- |
| `404`  | Order not found       |
| `500`  | Internal server error |

**Example Not Found Error**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Order with ID 'ccc97dfe-3694-4e0e-a000-b21c937c4a84' not found.",
  "instance": "/orders/ccc97dfe-3694-4e0e-a000-b21c937c4a84",
  "traceId": "00-b62f88be290620e6bf1d3b8ae837c591-1fede3eeae708b51-01",
  "timestamp": "2025-11-29T12:37:29.3427628Z"
}
```

---

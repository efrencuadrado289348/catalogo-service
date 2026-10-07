# Catalog Microservice – Electronics E-Commerce

Catalog microservice for the Distributed Systems course project.
Manages electronic products and categories in the store.

## Architecture

This project follows Clean Architecture and Domain-Driven Design (DDD) principles:
- **`Catalog.Domain`**: Core domain entities (`Product`, `Category`), value objects (`Price`), exceptions, and repository interfaces.
- **`Catalog.Application`**: CQRS application logic powered by MediatR (commands, queries, DTOs).
- **`Catalog.Infrastructure`**: Persistence layer with Entity Framework Core, PostgreSQL, and repository implementations.
- **`Catalog.API`**: RESTful API endpoints, Swagger documentation, and global exception handling.

## Team Members

- Simón Vanegas
- Efrén Cuadrado
- Sofía Castrillón
- Franyelica García
- Santiago Castaño

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) & Docker Compose

### Running with Docker Compose

To start both PostgreSQL and the API service:

```bash
docker compose up --build
```

The API will be available at `http://localhost:5080` (Swagger at `http://localhost:5080/swagger`).

### Running Locally

1. Start only the PostgreSQL database:
```bash
docker compose up catalog-db -d
```

2. Run the API:
```bash
dotnet run --project src/Catalog.API/Catalog.API.csproj
```

## API Endpoints

### Categories
- `POST /api/categories`: Create a new category
- `GET /api/categories`: List all categories
- `GET /api/categories/{id}`: Get a category by ID

### Products
- `POST /api/products`: Create a new product
- `GET /api/products`: List products (optional query param `?categoryId={id}`)
- `GET /api/products/{id}`: Get a product by ID
- `PATCH /api/products/{id}/price`: Update product price
- `PATCH /api/products/{id}/stock`: Adjust product stock
- `PATCH /api/products/{id}/deactivate`: Deactivate a product
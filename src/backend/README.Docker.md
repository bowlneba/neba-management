# Docker Setup for Neba Backend

This document explains how to run the Neba API and PostgreSQL database using Docker Compose.

## Quick Start

```bash
# From the src/backend directory
docker-compose up --build
```

The API will be available at: http://localhost:5150

## Services

### neba.api
- **Container Name**: `neba-api`
- **Image**: Built from `Neba.Api/Dockerfile`
- **Port**: 5150:8080
- **Environment**: Docker (uses `appsettings.Docker.json`)
- **Features**:
  - Release build configuration
  - No Key Vault access (uses local configuration)
  - Connects to `neba.db` database

### neba.db
- **Container Name**: `neba-db`
- **Image**: postgres:17.6
- **Port**: 19630:5432 (external:internal)
- **Database**: bowlneba
- **Credentials**:
  - Username: `neba`
  - Password: `neba`
- **Data**: Persisted in `neba-db-data` volume

## Connection Details

### From Host Machine
Connect to the database from your local machine (e.g., for migrations):
```
Host=localhost;Port=19630;Database=bowlneba;Username=neba;Password=neba
```

### Between Docker Containers
The API connects to the database using the service name:
```
Host=neba.db;Port=5432;Database=bowlneba;Username=neba;Password=neba
```

## Network

Both services are connected via a custom bridge network (`neba-network`) which allows:
- Service discovery by service name
- Isolated network communication
- Container-to-container connectivity

## Running Migrations

You have two options for running migrations:

### Option 1: From Host (Recommended for Development)
```bash
# From the repository root
dotnet ef database update --project src/backend/Neba.Infrastructure --startup-project src/backend/Neba.Api
```

This uses the connection string from `appsettings.Development.json` (localhost:19630).

### Option 2: Inside Docker Container
```bash
# Exec into the API container
docker exec -it neba-api sh

# Run migrations (requires EF Core tools installed in container)
dotnet ef database update
```

## Useful Commands

```bash
# Start services
docker-compose up

# Start in background
docker-compose up -d

# Rebuild images
docker-compose up --build

# Stop services
docker-compose down

# Stop and remove volumes (WARNING: deletes database data)
docker-compose down -v

# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f neba.api
docker-compose logs -f neba.db
```

## Troubleshooting

### Database Connection Failed

If you see connection errors:

1. Ensure both containers are on the same network: `docker network inspect neba-management-backend_neba-network`
2. Check database is ready: `docker logs neba-db`
3. Verify connection string uses `Port=5432` (internal port) in `appsettings.Docker.json`

### Database Not Initialized

If tables are missing:

```bash
# Run migrations from your host machine
dotnet ef database update --project src/backend/Neba.Infrastructure --startup-project src/backend/Neba.Api
```

### Port Already in Use

If port 5150 or 19630 is already in use, modify the ports in `docker-compose.yaml`:

```yaml
ports:
  - "YOUR_PORT:8080"  # Change YOUR_PORT
```

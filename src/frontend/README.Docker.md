# Frontend Docker Setup

## Quick Start

1. **Generate .env file from user secrets:**
   ```bash
   cd src/frontend
   ./setup-env.sh
   ```

2. **Start the frontend:**
   ```bash
   docker compose up
   ```

The frontend will be available at `http://localhost:5250`

## Prerequisites

- Backend services must be running: `cd src/backend && docker compose up -d`
- User secrets configured with Azure Maps subscription key

## Environment Variables

The `.env` file (auto-generated, git-ignored) contains:
- `AZURE_MAPS_SUBSCRIPTION_KEY` - Your Azure Maps subscription key
- `AZURE_MAPS_ACCOUNT_ID` - Your Azure Maps account ID (optional)

These are read from your local user secrets and injected into the Docker container, keeping them out of source control.

## Manual Setup

If you prefer to manually create the `.env` file:

1. Copy the example:
   ```bash
   cp .env.example .env
   ```

2. Get your secrets:
   ```bash
   dotnet user-secrets list --project Neba.Web.Server/Neba.Web.Server.csproj
   ```

3. Edit `.env` and paste your values

## Rebuilding

After code changes:
```bash
docker compose up --build
```

## Stopping

```bash
docker compose down
```

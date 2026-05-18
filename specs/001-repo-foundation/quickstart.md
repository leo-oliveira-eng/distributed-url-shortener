# Quickstart: Phase 0 Repository Foundation

This quickstart describes how a maintainer should verify the Phase 0 foundation
after implementation tasks are completed.

## Prerequisites

- .NET SDK 8.0 or newer
- Python 3.12 or newer
- Docker Compose v2
- Git

## Verify Feature Context

```powershell
git branch --show-current
Get-Content .specify/feature.json
```

Expected branch:

```text
001-repo-foundation
```

Expected feature directory:

```json
{
  "feature_directory": "specs/001-repo-foundation"
}
```

## Verify Required Files

```powershell
Test-Path .env.example
Test-Path docker-compose.yml
Test-Path DistributedUrlShortener.sln
Test-Path docs/architecture.md
Test-Path docs/tradeoffs.md
```

Each command should return `True`.

## Verify Solution Build

```powershell
dotnet restore DistributedUrlShortener.sln
dotnet build DistributedUrlShortener.sln --no-restore
```

Expected result: all Phase 0 .NET projects in the Shortener bounded context
compile successfully. The solution must not include Statistics Python projects.

## Verify API Health Shells

Run one API shell at a time, then request `/health` from the configured port.
The exact ports should come from `.env.example` and `docker-compose.yml`.

```powershell
dotnet run --project src/Shortener/DistributedUrlShortener.Shortener.Api/DistributedUrlShortener.Shortener.Api.csproj
```

```powershell
Invoke-WebRequest http://localhost:<shortener-api-port>/health
```

Repeat for:

- `src/Shortener/DistributedUrlShortener.Redirect.Service/`

Expected result: each applicable API shell returns a successful health response.
For Redirect Service, `/health` is the only allowed Phase 0 endpoint; `GET
/{shortCode}`, redirect logic, cache lookup, and event publishing must not be
implemented.

## Verify Statistics Python Shells

Install or run the Statistics shells according to the minimal Python project
setup under `src/Statistics/`.

```powershell
python --version
python -m pip install -e src/Statistics
```

Start the Statistics API shell and request `/health` from its configured port.

```powershell
python -m statistics_api
```

```powershell
Invoke-WebRequest http://localhost:<statistics-api-port>/health
```

Start each worker shell separately.

```powershell
python -m statistics_event_writer
python -m statistics_batch_processor
```

Expected result: Statistics.Api returns a successful health response.
Statistics.EventWriter.Worker starts without consuming events.
Statistics.BatchProcessor.Worker starts without scheduling jobs or processing
batches. The shared Statistics package/module contains no business logic.

## Verify Compose Baseline

```powershell
docker compose --env-file .env.example config
```

Expected result: Compose configuration renders without secrets, missing
variables, Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, or
observability services.

## Verify Documentation

Read the following files and confirm that they are in English and describe Phase
0 boundaries without claiming business behavior is implemented:

- `docs/architecture.md`
- `docs/tradeoffs.md`
- `CHANGELOG.md`

## Out-of-Scope Verification

Confirm that Phase 0 did not implement:

- URL creation
- Redirect resolution
- Redirect Service `GET /{shortCode}`
- Cache lookup
- Cassandra schema
- Redis cache behavior
- Redpanda event publishing
- MinIO/Parquet writing
- DuckDB batch processing
- ClickHouse dashboard queries
- Keycloak integration
- Angular frontend implementation
- Observability stack
- Load tests

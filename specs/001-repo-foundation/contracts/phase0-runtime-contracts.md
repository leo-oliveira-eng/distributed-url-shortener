# Phase 0 Runtime Contracts

Phase 0 contracts define only shell-level behavior. They intentionally avoid
product behavior from later phases.

## Solution Contract

`DistributedUrlShortener.sln` must include:

- `src/Shortener/DistributedUrlShortener.Shortener.Api/`
- `src/Shortener/DistributedUrlShortener.Redirect.Service/`
- `src/Statistics/DistributedUrlShortener.Statistics.Api/`
- `src/Statistics/DistributedUrlShortener.Statistics.EventWriter.Worker/`
- `src/Statistics/DistributedUrlShortener.Statistics.BatchProcessor.Worker/`
- `src/Statistics/DistributedUrlShortener.Statistics.Infrastructure/` as a
  non-runtime class library

Verification command:

```powershell
dotnet build DistributedUrlShortener.sln
```

## API Health Contract

Applicable API shells:

- Shortener API
- Redirect Service
- Statistics.Api

Endpoint:

```http
GET /health
```

Expected behavior:

- Returns a successful HTTP status when the shell process is running.
- Represents Phase 0 process health only.
- Is the only allowed endpoint for Redirect Service in Phase 0.
- Redirect Service MUST NOT implement `GET /{shortCode}` in Phase 0.
- Does not check Cassandra, Redis, Redpanda, MinIO, DuckDB, ClickHouse,
  Keycloak, ApiGateway, or frontend readiness.
- Does not expose URL creation, redirect resolution, cache lookup, event
  publishing, dashboard, analytics, or authentication behavior.

## Worker Shell Contract

Applicable worker shells:

- Statistics.EventWriter.Worker
- Statistics.BatchProcessor.Worker

Expected behavior:

- Builds as a separate executable project.
- Starts from a minimal `Program.cs`.
- Uses environment-based configuration.
- Does not consume events, write object-storage files, schedule jobs, process
  batches, or write analytics data in Phase 0.

## Configuration Contract

`.env.example` must provide safe local example values for Phase 0 shell
configuration and Compose rendering.

Required categories:

- Compose project name
- Application environment
- HTTP ports for API shells

Rules:

- No committed real secrets.
- No production endpoints.
- No credentials that imply real infrastructure access.
- Names should be stable enough for Phase 1 to extend.
- Do not add variables for Cassandra, Redis, Redpanda, MinIO, ClickHouse,
  Keycloak, or observability services until those services are in scope.

## Compose Contract

`docker-compose.yml` must provide a local-development baseline that can be
validated and extended.

Expected behavior:

- Uses `.env.example` variables where practical.
- May include only minimal application shell services or remain a minimal
  baseline.
- Does not include Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, or
  observability services in Phase 0.
- Phase 1 infrastructure must extend this file rather than replace it.
- Does not create Cassandra schemas, Redis cache behavior, Redpanda topics,
  MinIO buckets, ClickHouse tables, Keycloak realms, or observability services.
- Keeps future service boundaries visible and easy to extend.

## Statistics.Infrastructure Contract

`Statistics.Infrastructure` is a class library only.

Expected behavior:

- Builds as a non-executable project.
- Is included in `DistributedUrlShortener.sln`.
- Contains no business logic in Phase 0.
- Does not host workers, APIs, scheduled jobs, consumers, dashboard queries, or
  analytics processing.

# Phase 0 Runtime Contracts

Phase 0 contracts define only shell-level behavior. They intentionally avoid
product behavior from later phases.

## Solution Contract

`DistributedUrlShortener.sln` must include:

- `src/Shortener/Shortener.Api/`
- `src/Shortener/Redirect.Service/`

`DistributedUrlShortener.sln` must not include any Python Statistics project or
package.

Verification command:

```powershell
dotnet restore DistributedUrlShortener.sln
dotnet build DistributedUrlShortener.sln --no-restore
```

## API Health Contract

Applicable API shells:

- Shortener API
- Redirect Service
- Statistics.Api (Python)

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

- Statistics.EventWriter.Worker (Python)
- Statistics.BatchProcessor.Worker (Python)

Expected behavior:

- Starts as a separate Python application shell.
- Uses Python 3.12+.
- Starts from a minimal Python module entry point.
- Uses environment-based configuration.
- Does not consume events, write object-storage files, schedule jobs, process
  batches, or write analytics data in Phase 0.

## Statistics Python Package Contract

`src/Statistics/` must contain only Python project/package shells for the
Statistics bounded context.

Expected structure:

- `src/Statistics/statistics_api/`
- `src/Statistics/statistics_event_writer/`
- `src/Statistics/statistics_batch_processor/`
- `src/Statistics/shared/`
- Minimal Python dependency/configuration files as appropriate, such as
  `src/Statistics/pyproject.toml`

Expected behavior:

- Statistics.Api exposes only `/health`.
- Statistics.Api uses FastAPI for the health-only API shell.
- Statistics.EventWriter.Worker starts without consuming events.
- Statistics.BatchProcessor.Worker starts without scheduling jobs or processing
  batches.
- Shared Statistics package/module contains no business logic.
- Dependencies remain minimal.
- No Redpanda, MinIO, DuckDB, ClickHouse, Parquet, scheduling, or dashboard
  query behavior is implemented in Phase 0.

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

# distributed-url-shortener

![Status](https://img.shields.io/badge/status-phase%200%20foundation-yellow)
![Architecture](https://img.shields.io/badge/architecture-distributed-blue)
![Stack](https://img.shields.io/badge/phase%200-.NET%208%20%7C%20Python%203.12%2B-green)

A distributed URL shortener project built incrementally around explicit bounded
contexts, independent runtimes, and a future analytics pipeline.

## Phase 0 Status

Phase 0 is repository foundation only. It creates the monorepo layout, local
configuration baseline, .NET solution, Shortener service shells, Python
Statistics shells, and baseline documentation.

Phase 0 does not implement URL creation, redirect resolution, cache lookup,
event publishing, event consumption, batch processing, dashboard queries,
authentication, Angular frontend behavior, infrastructure services, or an
observability stack.

## Repository Structure

```text
src/
  ApiGateway/
  User/
  Shortener/
    Shortener.Api/
    Redirect.Service/
  Statistics/
    statistics_api/
    statistics_event_writer/
    statistics_batch_processor/
    shared/
  Front/
infra/
docs/
scripts/
tests/
  e2e/
  integration/
  load/
```

## Runtime Boundaries

- Shortener API: .NET 8 shell included in `DistributedUrlShortener.sln`.
- Redirect Service: independent .NET 8 shell included in
  `DistributedUrlShortener.sln`; Phase 0 exposes only `GET /health`.
- Statistics.Api: Python 3.12+ FastAPI shell exposing only `GET /health`.
- Statistics.EventWriter.Worker: Python shell that starts without consuming
  events.
- Statistics.BatchProcessor.Worker: Python shell that starts without scheduling
  jobs or processing data.
- Statistics shared package: Python package with no business logic.

Python Statistics packages are not included in the .NET solution.

## Documentation

- [Architecture](docs/architecture.md)
- [Trade-offs](docs/tradeoffs.md)
- [Roadmap](docs/roadmap.md)
- [Phase 0 quickstart](specs/001-repo-foundation/quickstart.md)

## Phase 0 Verification

```powershell
dotnet restore DistributedUrlShortener.sln
dotnet build DistributedUrlShortener.sln --no-restore
python -m pip install -e src/Statistics
docker compose --env-file .env.example config
```

Run API shells one at a time and request `/health` on the ports documented in
`.env.example`:

- Shortener API: `SHORTENER_API_HTTP_PORT`
- Redirect Service: `REDIRECT_SERVICE_HTTP_PORT`
- Statistics.Api: `STATISTICS_API_HTTP_PORT`

Worker shell checks:

```powershell
python -m event_writer_worker
python -m batch_processor_worker
```

## Future Target

Later phases will add authenticated URL management, public redirect resolution,
cache/storage integration, event-driven analytics, dashboard APIs, frontend
implementation, observability, and load testing while preserving the runtime
boundaries established in Phase 0.

# Trade-offs

## Redirect Service Separation

Redirect Service is separate from Shortener API because public redirect traffic
has a different availability, latency, and scaling profile than authenticated
URL management APIs. Phase 0 creates only the independent shell and limits it to
`GET /health` so later redirect behavior can be added without changing the
runtime boundary.

## Python Statistics Runtime Separation

Statistics.Api, Statistics.EventWriter.Worker, and
Statistics.BatchProcessor.Worker are separate Python 3.12+ runtimes because
HTTP query serving, future event writing, and future batch processing have
different lifecycles. Statistics.Api uses FastAPI only for the Phase 0 health
endpoint. The shared Statistics module is a Python package for future common
code and contains no business logic in Phase 0.

## Runtime Separation

Shortener services are .NET 8 projects and are the only projects included in
`DistributedUrlShortener.sln`. Statistics services are Python packages under
`src/Statistics/` and are not added to the .NET solution. This keeps build,
deployment, and ownership boundaries explicit from the first implementation
phase.

## Shell-first Implementation

Phase 0 creates buildable and runnable shells before adding business behavior.
This validates names, process boundaries, health checks, local configuration,
and documentation while keeping later URL, redirect, analytics, authentication,
frontend, and infrastructure work out of scope.

## Compose Baseline

The Compose file is intentionally minimal and validates local configuration
without pretending that Phase 1 infrastructure exists. Later phases should
extend the baseline with concrete services rather than replacing it.

## Known Phase 0 Limitations

Phase 0 does not include URL creation, redirect resolution, cache logic, event
publishing, event consumption, batch processing, dashboard queries,
authentication, Angular frontend implementation, Cassandra, Redis, Redpanda,
MinIO, DuckDB, ClickHouse, Keycloak, scheduling frameworks, or observability
services.


# Architecture

## Phase 0 Scope

Phase 0 establishes the repository foundation, runtime boundaries, local
configuration baseline, and documentation baseline. It does not implement URL
creation, redirect resolution, event publishing, event consumption, analytics
processing, authentication, frontend behavior, infrastructure services, or
observability services.

## Bounded Contexts

- `src/ApiGateway/`: future edge routing for authenticated API traffic.
- `src/User/`: future user, plan, and identity-facing service.
- `src/Shortener/`: .NET 8 services for URL ownership and public redirect
  ownership. Phase 0 contains only Shortener API and Redirect Service shells.
- `src/Statistics/`: Python 3.12+ Statistics runtimes. Phase 0 contains only a
  FastAPI health shell, two worker shells, and a shared Python package with no
  business logic.
- `src/Front/`: future frontend application placeholder.

## Request Flow

Authenticated API traffic is intended to enter through ApiGateway in later
phases. ApiGateway will route user-facing API requests to bounded-context APIs
such as Shortener and User. Phase 0 does not implement gateway routing,
authentication, authorization, or backend business endpoints.

## Redirect Service Independence

Future public `GET /{shortCode}` traffic belongs to Redirect Service outside
ApiGateway so the redirect path can scale and fail independently from
authenticated APIs. Phase 0 preserves this process boundary but exposes only
`GET /health`. Redirect lookup, cache lookup, storage access, event publishing,
and `GET /{shortCode}` are intentionally absent.

## Analytics Flow

The planned analytics flow is high level only in Phase 0: future redirect
events may be emitted from the redirect path, written by a Statistics event
writer, processed by a Statistics batch processor, and queried through
Statistics.Api. Phase 0 creates only the Python runtime shells and does not
consume events, write files, schedule jobs, process data, or serve dashboard
queries.

## Phase 0 Limitations

Only health endpoints and worker startup shells are present. Docker Compose is
kept as a minimal baseline for later extension and does not include Cassandra,
Redis, Redpanda, MinIO, ClickHouse, Keycloak, or an observability stack.


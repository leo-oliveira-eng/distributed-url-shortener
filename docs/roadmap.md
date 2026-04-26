# Implementation Roadmap

This roadmap organizes the evolution of `distributed-url-shortener` into
incremental phases. It follows `.specify/memory/constitution.md`, so every
implementation step must preserve separate runtime execution models, Redirect
Service independence, query-driven Cassandra modeling, non-blocking analytics,
observability, testing, and reproducible local development.

## Execution Principles

- Implement the system in small phases instead of a big bang.
- Keep clear ownership boundaries between `ApiGateway`, `User`, `Shortener`,
  `Statistics`, and `Front`.
- Treat `GET /{shortCode}` as an independent public path, outside the API
  Gateway and without authentication dependencies.
- Keep operational storage on Cassandra and Redis, and analytical processing on
  Redpanda, MinIO/Parquet, DuckDB, and ClickHouse.
- Use aggregated data for dashboards; raw events must not be queried by
  dashboard APIs.
- Document architectural trade-offs and decisions for each relevant phase.
- Include health checks, environment configuration, structured logs, metrics,
  and tests according to the risk of each phase.

## Phase 0 - Repository Foundation

**Goal:** prepare the project for incremental evolution.

**Scope:**

- Create or validate the base structure:

```text
src/
  ApiGateway/
  User/
  Shortener/
  Statistics/
  Front/
infra/
docs/
scripts/
tests/
```

- Add initial files:
  - `README.md`
  - `.specify/memory/constitution.md`
  - `.env.example`
  - `.gitignore`
  - `docker-compose.yml`
  - `docs/architecture.md`
  - `docs/tradeoffs.md`
- Create the initial solution and projects:
  - Shortener API
  - Redirect Service
  - Statistics.Api
  - Statistics.EventWriter.Worker
  - Statistics.BatchProcessor.Worker

**Acceptance criteria:**

- The repository structure follows the constitution.
- Each planned runtime has its own folder or project.
- The README explains the goal, target architecture, and intended local flow.

## Phase 1 - Minimal Local Infrastructure

**Goal:** run the base environment with Docker Compose.

**Scope:**

- Run the local services:
  - Nginx
  - Redis
  - Cassandra
  - Redpanda
  - MinIO
  - ClickHouse
  - Keycloak
- Do not include full observability yet.
- Add health checks.
- Create idempotent initialization scripts:
  - Cassandra schema
  - ClickHouse schema
  - Redpanda topics
  - MinIO buckets

**Acceptance criteria:**

- `docker compose up` starts the minimal infrastructure.
- Health checks report readiness for essential services.
- Initialization scripts can run more than once without breaking the
  environment.

## Phase 2 - Shortener API

**Goal:** create and list shortened URLs.

**Endpoints:**

- `POST /api/short-urls`
- `GET /api/short-urls`

**Responsibilities:**

- Receive authenticated requests through API Gateway and Keycloak.
- Validate the original URL.
- Generate the `shortCode`.
- Enforce Free and Pro quotas.
- Persist data in Cassandra.
- Populate Redis when useful for future redirects.

**Initial Cassandra model:**

- `urls_by_short_code`
- `urls_by_user_month`

**Constitution rules:**

- Quota enforcement belongs to Shortener API, never Redirect Service.
- Cassandra must be modeled by query, without relying on joins or secondary
  indexes for high-volume paths.
- TTL must follow plan retention:
  - Free: 1 month
  - Pro: 5 years

**Deliverables:**

- URL creation.
- User URL listing.
- Monthly Free and Pro quota enforcement.
- TTL-based expiration.
- Unit tests.
- Cassandra and Redis integration tests.

## Phase 3 - Redirect Service

**Goal:** implement the critical redirect path.

**Endpoint:**

- `GET /{shortCode}`

**Required flow:**

```text
Client
  -> Nginx/Ingress
  -> Redirect Service
  -> Redis
  -> Cassandra fallback
  -> Redis populate
  -> 302 redirect
  -> publish ClickEvent async
```

**Rules:**

- Does not depend on Keycloak.
- Does not go through the API Gateway.
- Does not call Shortener API.
- Does not block redirects when analytics fails.
- Fails fast on invalid configuration.
- Exposes health checks for required dependencies.

**Deliverables:**

- Functional redirects.
- Redis cache hit and miss behavior.
- Cassandra fallback.
- Asynchronous Redpanda publication.
- Basic metrics:
  - `redirect_requests_total`
  - `redirect_latency_ms`
  - `redis_cache_hit_total`
  - `redis_cache_miss_total`
  - `cassandra_lookup_latency_ms`
  - `click_event_publish_failures_total`
- Simple redirect load tests.

## Phase 4 - Event Writer

**Goal:** consume click events and store raw events.

**Runtime:**

- `Statistics.EventWriter.Worker`

**Flow:**

```text
Redpanda topic: click-events
  -> Statistics.EventWriter.Worker
  -> MinIO
  -> Parquet partitioned by date/hour
```

**Suggested layout:**

```text
raw-events/
  date=2026-04-26/
    hour=10/
      part-000.parquet
```

**Minimal ClickEvent:**

```json
{
  "shortCode": "abc123",
  "timestamp": "2026-04-26T10:00:00Z",
  "referrerSource": "linkedin.com",
  "country": "BR",
  "deviceType": "mobile",
  "browser": "chrome"
}
```

**Deliverables:**

- Redpanda consumer.
- Parquet writes to MinIO.
- Basic retry behavior.
- Structured logs.
- Integration tests.

## Phase 5 - Batch Processor with DuckDB

**Goal:** generate hourly aggregates from raw events.

**Runtime:**

- `Statistics.BatchProcessor.Worker`

**Flow:**

```text
MinIO raw Parquet
  -> DuckDB
  -> hourly aggregations
  -> ClickHouse
```

**Initial aggregations:**

- `clicks_by_url_hour`
- `clicks_by_country_hour`
- `clicks_by_referrer_hour`
- `clicks_by_device_hour`
- `clicks_by_browser_hour`

**Rules:**

- Start with a manual job.
- Evolve to hourly scheduling.
- Guarantee idempotency by `date/hour`.
- Document the selected scheduling strategy.

**Deliverables:**

- Manual processing for one `date/hour` window.
- Hourly scheduling.
- Aggregate writes to ClickHouse.
- Batch logs and metrics:
  - `batch_duration_seconds`
  - `events_processed_total`
  - generated aggregates

## Phase 6 - Statistics.Api / Dashboard API

**Goal:** query analytical aggregates.

**Suggested endpoints:**

- `GET /api/dashboard/urls/{shortCode}/clicks`
- `GET /api/dashboard/urls/{shortCode}/countries`
- `GET /api/dashboard/urls/{shortCode}/referrers`
- `GET /api/dashboard/urls/{shortCode}/devices`
- `GET /api/dashboard/urls/{shortCode}/browsers`

**Rules:**

- Only Pro users can access dashboard data.
- Query ClickHouse only.
- Never query raw events.
- Always access this API through the API Gateway.

**Deliverables:**

- Dashboard endpoints.
- Pro authorization.
- ClickHouse queries.
- Integration tests.
- `clickhouse_query_latency_ms` metric.

## Phase 7 - User API and Keycloak

**Goal:** formalize authentication, users, and plans.

**Runtime:**

- User API under `src/User/`, using NestJS and TypeScript.

**Responsibilities:**

- Keycloak integration.
- User profile.
- Free and Pro plans.
- Plan claims or plan lookup.

**Deliverables:**

- Keycloak realm.
- Seed users.
- Free and Pro plans.
- API Gateway integration.
- Login documentation.

## Phase 8 - API Gateway

**Goal:** centralize authenticated routes.

**Technology:**

- Ocelot.

**Routes:**

- `/api/short-urls` -> Shortener API
- `/api/users` -> User API
- `/api/dashboard` -> Statistics.Api

**Critical rule:**

- `GET /{shortCode}` does not go through the API Gateway.

**Deliverables:**

- Ocelot routing.
- JWT validation.
- Keycloak integration.
- Route documentation.

## Phase 9 - Angular Frontend

**Goal:** create a demonstrable user interface.

**Screens:**

- Login.
- Dashboard/Home.
- Create URL.
- List URLs.
- URL detail.
- Analytics Dashboard.

**Features:**

- OIDC/Keycloak authentication.
- Short URL creation.
- URL listing.
- Metrics visualization.
- Loading and error states.

**Rules:**

- The frontend uses only authenticated APIs through the API Gateway.
- The frontend does not access Redis, Cassandra, MinIO, ClickHouse, or Redpanda.
- The frontend does not call Redirect Service directly for authenticated flows.

**Deliverables:**

- Angular app in `src/Front/`.
- API Gateway integration.
- Basic charts.
- Tests for the main components.

## Phase 10 - Complete Observability

**Goal:** make the system demonstrable and debuggable.

**Add:**

- Prometheus.
- Grafana.
- OpenTelemetry.
- Jaeger or Tempo.
- OpenSearch or Elasticsearch.
- Fluent Bit.

**Essential metrics:**

- `redirect_requests_total`
- `redirect_latency_ms`
- `redis_cache_hit_total`
- `redis_cache_miss_total`
- `cassandra_lookup_latency_ms`
- `click_event_publish_failures_total`
- `broker_consumer_lag`
- `batch_duration_seconds`
- `events_processed_total`
- `clickhouse_query_latency_ms`

**Deliverables:**

- Grafana dashboards.
- Distributed tracing.
- Searchable structured logs.
- `docs/observability.md`.

## Phase 11 - Tests and Load

**Goal:** prove system behavior.

**Structure:**

```text
tests/
  integration/
  e2e/
  load/
```

**Scenarios:**

- Create URL.
- Redirect URL.
- Publish event.
- Generate Parquet.
- Process batch.
- Query dashboard.

**Load:**

- Redirect tests.
- Cache hit and miss behavior.
- p95 and p99 latency.
- Basic throughput.

**Possible tools:**

- k6
- NBomber
- Locust

**Naming rule:**

- Tests must follow `When_Scenario_Should_ExpectedBehavior`.

## Phase 12 - Final Documentation

**Goal:** turn the project into a showcase.

**Create or update:**

- `docs/architecture.md`
- `docs/redirect-fast-path.md`
- `docs/cassandra-modeling.md`
- `docs/analytics-pipeline.md`
- `docs/observability.md`
- `docs/capacity-estimation.md`
- `docs/tradeoffs.md`

**Include:**

- Diagrams.
- Technical decisions.
- Trade-offs.
- Known limits.
- Next steps.

## Recommended Order

0. Repository, constitution, and README.
1. Local infrastructure.
2. Shortener API.
3. Redirect Service.
4. Event Writer.
5. Batch Processor.
6. Statistics.Api.
7. Keycloak and User API.
8. API Gateway.
9. Angular Frontend.
10. Observability.
11. Tests and load.
12. Final documentation.

## Constitution Compliance Checklist

- Redirect Service remains independent from authentication, API Gateway,
  Shortener API, dashboard, and analytics.
- Shortener API enforces Free and Pro quotas and plan-based TTL.
- Cassandra uses query-driven tables and avoids secondary indexes on high-volume
  paths.
- ClickEvent includes the required minimal schema.
- Analytics does not block redirects.
- Raw events are stored as Parquet in MinIO, and dashboards query ClickHouse
  only.
- APIs, consumers, and batch processors run as separate executables.
- Angular frontend lives in `src/Front/` and consumes APIs through API Gateway.
- Local infrastructure uses Docker Compose, health checks, and idempotent
  scripts.
- Observability, configuration, and tests are first-class work items.

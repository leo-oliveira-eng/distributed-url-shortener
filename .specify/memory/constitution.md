<!--
Sync Impact Report
Version change: 1.2.0 -> 1.2.0
Modified principles:
- II. Redirect Fast Path Independence: clarified Redirect Service independence
  from ApiGateway and Shortener API
- IV. Non-Blocking Analytics Pipeline: added minimal ClickEvent schema
- VI. Separate Runtime Execution Models: clarified Shortener API and Redirect
  Service separation
Added principles:
- None
Added sections:
- None
Removed sections:
- None
Templates requiring updates:
- updated: .specify/templates/plan-template.md
- updated: .specify/templates/spec-template.md
- updated: .specify/templates/tasks-template.md
- updated: .specify/templates/checklist-template.md
- not present: .specify/templates/commands/*.md
Runtime guidance:
- reviewed: README.md; no principle references required an update
Deferred TODOs:
- None
-->
# distributed-url-shortener Constitution

## Core Principles

### I. Portfolio-Grade Distributed System Clarity
This repository MUST remain a portfolio-oriented, production-inspired distributed
URL shortener. Every feature MUST make the system design, operational trade-offs,
and backend engineering practices easier to inspect. Implementations MUST favor
small, explicit services over shared domain abstractions. Shared libraries are
allowed only for truly cross-cutting concerns such as telemetry conventions,
configuration helpers, or protocol contracts.

The monorepo MUST preserve clear service and ownership boundaries. Backend code
belongs under `src/ApiGateway`, `src/User`, `src/Shortener`, and
`src/Statistics`. Frontend code MUST belong under `src/Front/` and MUST use
Angular. Infrastructure configuration belongs under `infra/`, documentation
under `docs/`, scripts under `scripts/`, and load, integration, and end-to-end
tests under `tests/`.

Rationale: the project exists to demonstrate architecture and engineering
judgment. Hidden coupling, generic common layers, or unexplained abstractions
reduce its value as a readable distributed-systems portfolio.

### II. Redirect Fast Path Independence
The public redirect endpoint `GET /{shortCode}` MUST be implemented by Redirect
Service and optimized for low latency and high availability. The redirect flow
MUST remain independent of authentication, ApiGateway, Shortener API, analytics
processing, dashboard queries, and user-management workflows. The required
conceptual path is:

`Client -> Nginx/Ingress -> Redirect Service -> Redis -> Cassandra fallback -> async event publication`

Redirect Service MUST read from Redis first, fall back to Cassandra on cache
miss, and publish click events asynchronously. Redis MUST be the cache for
`shortCode -> longUrl` resolution. Cassandra MUST be the authoritative fallback
store for URL mapping lookup. Redirect Service MUST NEVER block on the analytics
pipeline and MUST NOT call Shortener API to complete a redirect. Redirect logic
MUST fail fast on invalid configuration and MUST expose health checks for its
required dependencies.

Rationale: the redirect path is the core user-facing path and must be isolated
from slower authenticated or analytical workflows.

### III. Query-Driven Distributed Storage
Cassandra data modeling MUST be query-driven. The main redirect lookup table MUST
use `shortCode` as the partition key. Additional Cassandra tables MAY duplicate
URL data to support other access patterns, such as listing URLs by user, but each
table MUST be justified by a concrete query. High-volume paths MUST NOT depend on
secondary indexes or generic relational-style joins.

Large partitions MUST be prevented with explicit bucketing where query patterns
can grow without bound. TTL and expiration strategies MUST align with plan
retention: Free user URLs are retained for 1 month, and Pro user URLs are
retained for 5 years. Capacity reasoning MUST account for the conceptual targets
of 100 million URL creations per day, 1 billion redirects per day, and an average
stored original URL size of about 100 bytes.

Rationale: Cassandra is chosen for distributed write and lookup scale. Its schema
must follow access patterns rather than relational normalization habits.

### IV. Non-Blocking Analytics Pipeline
Redirect Service MUST publish click events asynchronously to a Kafka-compatible
broker such as Redpanda. Analytics MUST NOT block redirection. Raw click events
MUST be retained only briefly at first, with an initial target of 1 day, and raw
events MUST be written to object storage as Parquet files.

Raw events MUST NOT be used for dashboard queries. All dashboard queries MUST use
precomputed aggregates. Aggregation MUST be done in batch using DuckDB over
Parquet files. ClickHouse MUST store only aggregated or query-ready analytical
data. Statistics.Api (Dashboard API) MUST query ClickHouse only.

ClickEvent MUST include at minimum `shortCode`, `timestamp`, `referrerSource`,
`country`, `deviceType`, and `browser`. It MAY include `userAgent`, `ipHash`, and
`requestId`. This schema is required so batch processing and aggregate tables
remain consistent across producers and consumers.

The first dashboard version MUST use hourly summaries, with separate
aggregations by purpose such as clicks by URL, country, referrer, device, and
browser. Unique click detection is out of scope for the first version.

Rationale: analytics exists to demonstrate event-driven processing and OLAP
modeling without putting the redirect experience at risk.

### V. Observable, Testable, Reproducible Services
All runtime applications MUST produce structured logs, expose meaningful metrics,
and emit or propagate distributed traces through OpenTelemetry where practical.
Logs, metrics, and traces MUST include correlation identifiers when available.
Important metrics include redirect request count, redirect latency, Redis
hit/miss rate, Cassandra lookup latency, click event publication failures, broker
consumer lag, raw files processed, batch duration, events processed, aggregates
generated, ClickHouse query latency, and dashboard query errors.

Local development MUST be reproducible with Docker Compose. Runtime applications
MUST use documented environment variables, provide `.env.example` entries for
required configuration, avoid committed secrets, and prefer health checks.
Database and infrastructure initialization scripts MUST be idempotent.

Core business rules MUST have unit tests. Frontend interaction logic and
dashboard behavior MUST have Angular tests when implemented. Infrastructure
boundaries SHOULD have integration tests when practical. End-to-end tests MUST
cover the main flows: create short URL, redirect short URL, publish click event,
process analytics batch, and query dashboard. Load testing scripts MUST be
included for redirect traffic. Test names MUST follow
`When_Scenario_Should_ExpectedBehavior`.

Rationale: observability and reproducibility are core deliverables, not polish.
The project must be demonstrable, debuggable, and safe to evolve.

### VI. Separate Runtime Execution Models
API services, event consumers, and batch processors MUST be implemented as
separate executables and separate runtime processes. They MAY live in the same
bounded context, such as `src/Statistics`, but they MUST NOT be implemented as a
single runtime service. Redirect Service MUST run as a separate process from
Shortener API, MUST be independently scalable and deployable, and MUST NOT depend
on Shortener API at runtime. Shortener API MUST NOT be in the redirect fast path.

Rationale: HTTP APIs, streaming consumers, and scheduled batch jobs have
different scaling, failure, deployment, and lifecycle characteristics.

## Architecture Constraints

The system MUST implement a scalable URL shortener with these user-facing
capabilities:

- Public redirect endpoint: `GET /{shortCode}`.
- Authenticated APIs for users, plans, URL creation, URL listing, and dashboard
  access.
- Free and Pro plans.
- Free users can create up to 5 URLs per month.
- Pro users can create up to 5000 URLs per month.
- Pro users have access to the analytics dashboard.
- Free URLs are retained for 1 month.
- Pro URLs are retained for 5 years.

Authenticated APIs MUST go through `ApiGateway` and IAM/Keycloak. ApiGateway MUST
use Ocelot. Shortener API and Redirect Service MUST be implemented in .NET 8 or
newer under `src/Shortener/`. User API MUST be implemented with NestJS and
TypeScript under `src/User/`. Frontend MUST be implemented with Angular under
`src/Front/`.

Shortener API MUST handle authenticated URL creation, URL management, and quota
enforcement through ApiGateway. URL creation quotas MUST be enforced in Shortener
API: Free users can create at most 5 URLs per month, and Pro users can create at
most 5000 URLs per month. Quota logic MUST NOT be implemented in Redirect
Service.

Redirect Service MUST handle the public `GET /{shortCode}` endpoint, MUST be
optimized for low latency, MUST NOT depend on authentication or ApiGateway, and
MUST follow the fast path `Client -> Nginx/Ingress -> Redirect Service -> Redis
-> Cassandra -> async event`. Although Shortener API and Redirect Service both
live under `src/Shortener/`, they MUST be implemented as separate applications
and separate executables.

The Statistics bounded context MUST be implemented under `src/Statistics/` and
MUST be composed of independent applications:

```text
src/Statistics/
  DistributedUrlShortener.Statistics.Api/
  DistributedUrlShortener.Statistics.EventWriter.Worker/
  DistributedUrlShortener.Statistics.BatchProcessor.Worker/
  DistributedUrlShortener.Statistics.Infrastructure/
```

Statistics.Api (Dashboard API) MUST expose dashboard endpoints, query ClickHouse
only, and MUST NOT access raw events directly. Statistics.EventWriter.Worker MUST
consume click events from the broker, write raw events to object storage as
Parquet files in MinIO/S3, and run continuously. Statistics.BatchProcessor.Worker
MUST run scheduled jobs, initially hourly, use DuckDB to process Parquet files,
and write aggregated data to ClickHouse. Scheduling MAY be implemented using
cron, Quartz, Hangfire, or an equivalent mechanism, and the selected scheduling
strategy MUST be documented in implementation plans.

The Angular frontend MUST use only authenticated APIs through ApiGateway. It MUST
NOT call Redirect Service directly and MUST NOT access ClickHouse, MinIO/S3,
Cassandra, or Redis directly. The redirect endpoint is public and MUST be
accessed directly by clients and browsers. Dashboard views MUST consume
aggregated data exposed by Statistics.Api (Dashboard API). Frontend features MUST
define routes, components, services, API contracts, Keycloak/OIDC authentication
flows, loading states, and error states.

Local infrastructure SHOULD include Nginx or another ingress/reverse proxy,
Redis, Cassandra, Redpanda or another Kafka-compatible broker, MinIO, ClickHouse,
Keycloak, Prometheus, Grafana, an OpenTelemetry collector or Jaeger/Tempo, and
OpenSearch, Elasticsearch, or another structured logging backend when the related
feature is in scope.

The canonical repository structure is:

```text
src/
  ApiGateway/
  User/
  Shortener/
  Statistics/
    DistributedUrlShortener.Statistics.Api/
    DistributedUrlShortener.Statistics.EventWriter.Worker/
    DistributedUrlShortener.Statistics.BatchProcessor.Worker/
    DistributedUrlShortener.Statistics.Infrastructure/
  Front/
infra/
  nginx/
  cassandra/
  redis/
  redpanda/
  minio/
  clickhouse/
  keycloak/
  observability/
docs/
scripts/
tests/
  e2e/
  integration/
  load/
```

Documentation and generated tasks MUST use the names Redirect Service, Shortener
API, User API, Statistics.Api (Dashboard API),
Statistics.EventWriter.Worker, and Statistics.BatchProcessor.Worker when
referring to runtime components.

## Decision Guidelines

Every major architectural choice MUST be documented with trade-offs in `docs/` or
the relevant feature plan. Required documentation topics include high-level
architecture, redirect fast path, Cassandra partition key choices, analytics
pipeline, capacity assumptions, Cassandra versus DynamoDB, ClickHouse versus
MongoDB for analytics, DuckDB's role in batch processing, observability strategy,
and diagrams where useful.

Implementation plans MUST reject coupling that makes the redirect path depend on
authentication, analytics, dashboard, or user-management concerns. Features
touching Cassandra MUST state the query pattern, partition key, clustering
strategy, bucketing strategy when needed, TTL/retention behavior, and why
secondary indexes are not used on high-volume paths.

Features touching analytics MUST define event shape, broker topic, raw Parquet
layout, Statistics.EventWriter.Worker behavior, Statistics.BatchProcessor.Worker
schedule, scheduling mechanism, DuckDB batch responsibilities, ClickHouse
aggregate tables, and Statistics.Api dashboard query behavior. Dashboard work
MUST use aggregate tables unless the constitution is amended.

Features touching URL creation MUST define quota enforcement behavior in
Shortener API. Quota enforcement SHOULD use Redis counters or persistent storage.
Quota logic MUST NOT be implemented in Redirect Service.

Features touching frontend behavior MUST define Angular routes, components,
services, API contracts, Keycloak/OIDC authentication flows, loading states,
error states, and tests. Frontend features MUST use `src/Front/` paths, MUST keep
all authenticated API access behind ApiGateway, MUST NOT call Redirect Service
directly, and MUST consume dashboard analytics from Statistics.Api.

Specs and tasks MUST include observability, configuration, health checks, and
tests as first-class work items. The README MUST explain how to run the system
locally once runnable services exist.

Non-goals for the first version are billing integration, real payment processing,
unique click detection, advanced fraud detection, multi-region deployment,
Kubernetes production deployment, full cloud deployment, perfect cost
optimization, and production-grade security hardening.

## Governance

This constitution supersedes conflicting feature specs, implementation plans, and
task lists. When a conflict is found, the spec or plan MUST be changed unless an
explicit constitution amendment is accepted.

Amendments MUST include:

- The proposed principle or constraint change.
- The reason the current constitution is insufficient.
- Migration notes for affected specs, docs, templates, or implementation work.
- A semantic version bump.

Versioning policy:

- MAJOR for incompatible governance changes, removed principles, or redefined
  architectural direction.
- MINOR for new principles, new required sections, or materially expanded
  guidance.
- PATCH for wording clarifications, typo fixes, or non-semantic refinements.

Compliance review MUST happen during Spec-Kit planning through the Constitution
Check and again before implementation tasks are accepted. Reviewers MUST verify
redirect path independence, separate runtime execution models, frontend/API
Gateway separation, Cassandra modeling, analytics non-blocking behavior,
observability, testing, reproducible local setup, and documentation impacts.

**Version**: 1.0.0 | **Ratified**: 2026-04-26 | **Last Amended**: 2026-04-26

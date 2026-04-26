# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See
`.specify/templates/plan-template.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace this section with the technical details for this
  feature. Use NEEDS CLARIFICATION only when the feature description and
  constitution do not provide enough information.
-->

**Language/Version**: [.NET 8+ for Shortener API / Redirect Service, NestJS/TypeScript for User API, Python for Statistics workers/API, Angular/TypeScript for src/Front, Ocelot for ApiGateway, or N/A]  
**Primary Dependencies**: [Angular, Redis, Cassandra, Redpanda/Kafka, MinIO, DuckDB, ClickHouse, Keycloak, OpenTelemetry, or N/A]  
**Storage**: [Cassandra/Redis/MinIO/ClickHouse data touched by this feature, or N/A]  
**Testing**: [Unit/integration/e2e/load coverage required by the constitution]  
**Target Platform**: [Docker Compose local distributed system]  
**Project Type**: [Monorepo distributed backend system with Angular frontend]  
**Performance Goals**: [Redirect latency, throughput, batch duration, query latency, or N/A]  
**Constraints**: [Redirect fast path independence, query-driven Cassandra model, non-blocking analytics, plan limits, retention]  
**Scale/Scope**: [Conceptual support for 100M URL creations/day, 1B redirects/day, 100-byte average original URL, feature-specific scope]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Service Boundaries**: Plan keeps service ownership explicit under
  `src/ApiGateway`, `src/User`, `src/Shortener`, `src/Statistics`, or `src/Front`,
  and avoids shared domain logic unless truly cross-cutting.
- **Frontend Boundary**: Any frontend work uses Angular under `src/Front/`, consumes
  APIs only through `ApiGateway` and Keycloak/OIDC, does not become part of the
  public redirect fast path, and does not access ClickHouse, MinIO/S3,
  Cassandra, or Redis directly.
- **Runtime Model**: APIs, event consumers, and batch processors are separate
  executables/processes. Statistics work is placed in Statistics.Api,
  Statistics.EventWriter.Worker, Statistics.BatchProcessor.Worker, or
  Statistics.Infrastructure as appropriate. Redirect Service and Shortener API
  are separate executables under `src/Shortener/`.
- **Shortener Context**: URL creation, management, and quota enforcement belong
  in Shortener API. Redirect Service owns public `GET /{shortCode}`, is
  independently scalable/deployable, and is not behind ApiGateway.
- **Redirect Fast Path**: Any redirect work preserves
  `Client -> Nginx/Ingress -> Redirect Service -> Redis -> Cassandra fallback ->
  async event publication`, reads Redis first, falls back to Cassandra, and never
  blocks on authentication, analytics, dashboard, or user-management dependencies.
- **Cassandra Modeling**: Any Cassandra change documents query pattern,
  partition key, clustering, bucketing when needed, TTL/retention, and avoids
  secondary indexes for high-volume paths.
- **Analytics Pipeline**: Any analytics change keeps event publication
  asynchronous, raw events short-lived in Parquet object storage through
  Statistics.EventWriter.Worker, DuckDB batch aggregation in
  Statistics.BatchProcessor.Worker, ClickHouse as the aggregate/query-ready
  dashboard store, dashboard queries on aggregates rather than raw events, and a
  ClickEvent schema with required fields.
- **Observability and Operations**: Plan includes structured logs, metrics,
  tracing/correlation IDs, health checks, invalid configuration failure behavior,
  `.env.example` impact, and Docker Compose reproducibility.
- **Testing and Load**: Plan includes unit tests for business rules, practical
  integration tests for infrastructure boundaries, required e2e coverage for
  main flows, Angular tests for frontend behavior, and redirect load scripts
  when traffic behavior changes.
- **Documentation**: Plan updates `docs/` and README content when architecture,
  setup, capacity, storage modeling, analytics, or trade-offs change.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
  plan.md              # This file (/speckit.plan command output)
  research.md          # Phase 0 output (/speckit.plan command)
  data-model.md        # Phase 1 output (/speckit.plan command)
  quickstart.md        # Phase 1 output (/speckit.plan command)
  contracts/           # Phase 1 output (/speckit.plan command)
  tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

<!--
  ACTION REQUIRED: Replace or narrow this tree to the concrete paths touched by
  the feature. Keep real repository paths; do not invent service names that do
  not exist.
-->

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

**Structure Decision**: [Document the selected paths and why the feature belongs
there. Note any new service or directory introduced by the plan.]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., new runtime outside canonical structure] | [current need] | [why existing service boundary is insufficient] |
| [e.g., dashboard query against raw events] | [current need] | [why aggregate table cannot satisfy the requirement] |

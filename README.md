# distributed-url-shortener

![Status](https://img.shields.io/badge/status-in%20progress-yellow)
![Architecture](https://img.shields.io/badge/architecture-distributed-blue)
![Stack](https://img.shields.io/badge/stack-.NET%208%20%7C%20NestJS%20%7C%20ClickHouse-green)

---

> A distributed, event-driven URL shortener designed for high-scale systems, low-latency redirects, and analytics pipelines.

## Key Features

- URL shortening for Free and Pro plans, including quota and retention rules.
- Public redirect endpoint optimized for low latency and high availability.
- High-scale architecture target: 100M writes/day and 1B reads/day, conceptually.
- Event-driven click analytics pipeline decoupled from redirects.
- Dashboard API backed by aggregated analytical data.
- Observability through structured logs, metrics, and distributed tracing.

## Why This Project?

This project focuses on:

- High-scale system design
- Event-driven architecture
- Analytics pipeline with batch processing
- Clear separation between operational and analytical workloads
- Observability as a first-class concern

## Key Design Highlight: Redirect Fast Path

The redirect endpoint is designed as an independent, low-latency path:

- No authentication
- No API Gateway dependency
- Redis-first lookup
- Cassandra fallback
- Async analytics

This design mirrors real-world large-scale systems where the read path must remain isolated from operational complexity.

## Architecture Overview

The system is split into three main flows:

- **Fast path:** resolves `shortCode -> longUrl` through Redis, with Cassandra as the authoritative fallback, then publishes analytics events asynchronously.
- **Write path:** authenticated users create and manage URLs through the API Gateway and Shortener API.
- **Analytics path:** click events are consumed, written as Parquet, processed in batches, aggregated in ClickHouse, and exposed to the dashboard.
- **Frontend flow:** browser clients use the Angular app, which calls authenticated backend APIs through the API Gateway.

```text
Client (Browser)
  |
  +-- Angular App -> API Gateway
  |                 |
  |                 +-- Shortener API
  |                 +-- User API
  |                 +-- Dashboard API
  |
  +-- Redirect Requests -> Nginx -> Redirect Service
                              |
                              +-- Redis
                              +-- Cassandra
                              +-- Event Broker
                                    |
                                    +-- Event Writer -> MinIO (Parquet)
                                          |
                                          +-- Batch Processor (DuckDB)
                                                |
                                                +-- ClickHouse
```

## Roadmap

The implementation roadmap lives in [`docs/roadmap.md`](docs/roadmap.md). It
breaks the project into incremental phases, from repository foundation and local
infrastructure through Shortener API, Redirect Service, analytics workers,
dashboard APIs, Angular frontend, observability, load testing, and final
showcase documentation.

The roadmap is aligned with the project constitution, especially the
independence of the redirect fast path, separate runtime execution models,
query-driven Cassandra modeling and non-blocking analytics.

## Core Design Principles

- **Redirect fast path independence:** redirects do not depend on authentication, the API Gateway, dashboard queries, or analytics processing.
- **Query-driven Cassandra modeling:** tables are designed around access patterns, starting with `shortCode` lookup for redirects.
- **Event-driven analytics:** click events are emitted asynchronously so analytics failures do not affect redirects.
- **Batch processing with DuckDB:** raw Parquet files are processed in scheduled batches before loading analytical aggregates.
- **Aggregated analytics with ClickHouse:** dashboard reads use query-ready aggregate tables, not raw events.
- **Separate runtime execution models:** APIs, event consumers, and batch processors run as independent processes.

## Scalability Model

The system is designed to scale horizontally:

- Redirect Service scales independently for read-heavy workloads.
- Cassandra handles high write throughput with partitioned data.
- Redis absorbs read pressure with high cache hit rates.
- Event pipeline decouples analytics from user-facing flows.
- ClickHouse supports fast analytical queries over large datasets.

## Technology Stack

### Backend

- .NET 8 for Shortener API and Redirect Service
- NestJS for User Service
- Python for Statistics workers

### Data & Storage

- Cassandra for authoritative URL mappings
- Redis for redirect-path caching
- ClickHouse for analytical aggregates
- MinIO as S3-compatible object storage
- DuckDB for batch processing over Parquet

### Messaging

- Redpanda as the Kafka-compatible event broker

### Observability

- Prometheus for metrics
- Grafana for dashboards
- OpenTelemetry for traces
- OpenSearch or Elasticsearch for structured log search

### Frontend

- Angular for the authenticated web application and analytics dashboard

## Repository Structure

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
docs/
scripts/
tests/
```

- `src/ApiGateway/`: API gateway configuration and edge routing for authenticated APIs.
- `src/User/`: NestJS user, plan, and identity-facing service.
- `src/Shortener/`: .NET URL creation APIs and independent redirect runtime.
- `src/Statistics/`: dashboard API, event writer, batch processor, and analytics infrastructure code.
- `src/Front/`: Angular frontend for authenticated user flows and dashboards.
- `infra/`: Docker Compose, service configuration, database initialization, and observability setup.
- `docs/`: architecture decisions, trade-offs, diagrams, and capacity notes.
- `scripts/`: local automation and repeatable operational tasks.
- `tests/`: end-to-end, integration, and load tests.

## Services Overview

- **Redirect Service:** public `GET /{shortCode}` endpoint. Reads Redis first, falls back to Cassandra, and publishes click events asynchronously.
- **Shortener API:** authenticated URL creation and management API. Enforces Free and Pro quotas and writes URL mappings.
- **User API:** user, plan, and identity-related service built with NestJS.
- **Statistics.EventWriter.Worker:** consumes click events and writes raw event files to MinIO in Parquet format.
- **Statistics.BatchProcessor.Worker:** scheduled worker that uses DuckDB to process Parquet batches and write aggregates to ClickHouse.
- **Statistics.Api:** dashboard-facing API that reads aggregate analytics from ClickHouse.
- **Frontend:** Angular application for URL management, account flows, and analytics dashboards.

## Analytics Pipeline

Redirects publish click events asynchronously to Redpanda. Each event includes at minimum:

- `shortCode`
- `timestamp`
- `referrerSource`
- `country`
- `deviceType`
- `browser`

The event writer persists raw events to MinIO as Parquet files. Batch jobs use DuckDB to scan those files, compute hourly aggregates, and load query-ready data into ClickHouse. The dashboard never queries raw events directly; it reads aggregated metrics through Statistics.Api.

## Quick Start

The intended local entry point is Docker Compose:

```bash
cp .env.example .env
docker compose up --build
```

Then open:

```text
API Gateway: http://localhost:5000
Frontend:    http://localhost:4200
Grafana:     http://localhost:3000
```

Local infrastructure is expected to include:

- Cassandra
- Redis
- Redpanda
- MinIO
- ClickHouse
- Keycloak
- Nginx
- Prometheus, Grafana, OpenTelemetry, and OpenSearch or Elasticsearch

Configuration should be documented through `.env.example`. Runtime secrets must stay out of version control.

> Note: this repository is currently scaffolded. The README documents the target local workflow; compose files and service implementations will be added as the system is built.

## Observability

Each runtime service should emit structured logs, metrics, and traces with correlation identifiers where possible.

Example metrics:

- Redirect request count and latency
- Redis hit/miss rate
- Cassandra lookup latency
- Click event publication failures
- Broker consumer lag
- Raw files processed
- Batch processing duration
- Events processed and aggregates generated
- ClickHouse query latency
- Dashboard query errors

## Trade-offs

### Cassandra vs DynamoDB

Cassandra keeps the storage layer cloud-portable and makes partitioning, replication, and data modeling explicit for portfolio purposes. DynamoDB would reduce operational burden in AWS, but it would make the project more cloud-specific and hide some distributed-storage trade-offs.

### ClickHouse vs MongoDB

ClickHouse is purpose-built for analytical scans and aggregate queries over high-volume event data. MongoDB is flexible for document storage, but it is not the strongest fit for dashboard analytics at this scale when the read model is metric-oriented.

### Batch vs Real-time Analytics

Batch analytics keeps the first version simpler, cheaper, and easier to reason about. It also protects the redirect path from analytical backpressure. Real-time analytics can be added later for fresher dashboards, but it increases operational complexity and requires stricter stream-processing guarantees.

## Status

🚧 Work in progress

This repository is currently focused on architecture and scaffolding.
Core services and infrastructure will be implemented incrementally.

## Future Improvements

- Unique click tracking
- Fraud and bot detection
- Multi-region deployment model
- Kubernetes manifests or Helm charts
- Real billing and payment integration
- Real-time analytics path for near-live dashboard updates
- Automated load testing for redirect throughput

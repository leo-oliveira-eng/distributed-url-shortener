# Research: Phase 1 Minimal Local Infrastructure

## Decision: Extend the Phase 0 Compose baseline in place

**Rationale**: The approved specification requires Phase 1 to extend the
existing `docker-compose.yml`. One Compose file keeps local startup simple,
preserves the existing `app` network, and makes the seven active
infrastructure services visible in one place.

**Alternatives considered**:

- Add a separate Compose override file: rejected because one local-only
  environment does not need layered orchestration yet.
- Add application shells to Compose: rejected because Phase 1 is
  infrastructure-only and must not change application readiness semantics.

## Decision: Use one shared bridge network and loopback-only published ports

**Rationale**: Containers need stable service discovery names for later phases,
while host tools need predictable local ports for diagnostics. Binding
published ports to `127.0.0.1` reduces accidental exposure of development-mode
services and example credentials.

**Alternatives considered**:

- Bind host ports on all interfaces: rejected because Redis and Keycloak
  development defaults are unsafe for remote exposure.
- Create multiple infrastructure networks now: rejected because Phase 1 has no
  application routing or trust-boundary behavior that benefits from extra
  network segmentation.

## Decision: Pin exact service image versions

**Rationale**: Reproducible local setup should not drift when upstream `latest`
tags move. The implementation should use:

| Service | Selected version |
|---------|------------------|
| Nginx | `nginx:1.30.2-alpine` |
| Redis | `redis:8.6.3-alpine` |
| Cassandra | `cassandra:5.0.8-bookworm` |
| Redpanda | `redpandadata/redpanda:v26.1.4` |
| ClickHouse | `clickhouse:26.5.1.882-jammy` |
| Keycloak | `quay.io/keycloak/keycloak:26.6.2` |

These are local-development selections, not a production platform support
matrix. Any upgrade should be intentional and validated through the documented
Compose and readiness checks.

**Alternatives considered**:

- Use floating major or `latest` tags: rejected because fresh checkouts could
  render materially different environments.
- Pin image digests immediately: deferred because exact tags keep the plan
  readable across supported developer architectures. Implementation may record
  digests later if the repository adopts automated image update tooling.

## Decision: Build MinIO Community Edition locally from the last official security source release

**Rationale**: The architecture and approved specification require MinIO.
However, MinIO Community Edition is now source-only, the upstream repository is
archived, and the last prebuilt `minio/minio` server image predates the final
official security release. Use a small multi-stage `infra/minio/Dockerfile` to
build the server from official source tag
`RELEASE.2025-10-15T17-29-55Z`. Use the archived official
`minio/mc:RELEASE.2025-08-13T08-35-41Z` image only as an initialization client.

This keeps the required local dependency usable without selecting a known-stale
prebuilt server image. The upstream archive status must be documented as a
trade-off and revisited before production-oriented work.

**Alternatives considered**:

- Use `minio/minio:RELEASE.2025-09-07T16-13-09Z`: rejected because the final
  official source release contains a security fix not present in that prebuilt
  image.
- Use an unofficial third-party MinIO image: rejected because the project
  should not silently adopt a new image maintainer.
- Replace MinIO with another S3-compatible service: rejected because that
  changes the approved Phase 1 specification and later-phase architecture.

## Decision: Keep initialization explicit and idempotent

**Rationale**: Four opt-in Compose helper services under the `init` profile make
resource creation discoverable, rerunnable, and independently diagnosable.
Scripts create only the approved foundations:

- Cassandra keyspace
- Redpanda `click-events` topic
- MinIO `raw-events` bucket
- ClickHouse database

**Alternatives considered**:

- Hide initialization inside long-running service startup: rejected because
  startup ordering failures become harder to diagnose and rerun.
- Create Cassandra and ClickHouse tables now: rejected because readiness does
  not require tables and later schema must be driven by concrete queries.

## Decision: Defer Keycloak realm import

**Rationale**: Keycloak infrastructure readiness is proven by starting the
service and checking `/health/ready`. A realm, client, user, login flow, or JWT
configuration would not improve Phase 1 readiness and risks introducing Phase 7
behavior prematurely.

**Alternatives considered**:

- Import a minimal local realm: rejected because it adds no infrastructure-only
  validation value in this phase.

## Decision: Use readiness checks without adding an observability stack

**Rationale**: The constitution requires reproducible services and practical
health checks while deferring the complete local observability stack to Phase
10. Each Phase 1 service gets a Compose health check where practical and a
documented host-side readiness command.

**Alternatives considered**:

- Add Prometheus or Grafana for service status: rejected because full
  observability is explicitly out of scope.
- Treat running containers as ready: rejected because Cassandra, Redpanda, and
  Keycloak may accept process startup before they can serve useful requests.

## Official References

- Nginx downloads: https://nginx.org/en/download.html
- Nginx Docker Official Image: https://hub.docker.com/_/nginx
- Redis Docker Official Image: https://hub.docker.com/_/redis
- Cassandra downloads: https://cassandra.apache.org/_/download.html
- Cassandra Docker Official Image: https://hub.docker.com/_/cassandra
- Redpanda current release documentation: https://docs.redpanda.com/current/get-started/release-notes/
- Redpanda Docker image: https://hub.docker.com/r/redpandadata/redpanda
- Redpanda `rpk cluster health`: https://docs.redpanda.com/current/reference/rpk/rpk-cluster/rpk-cluster-health/
- MinIO source-only repository notice: https://github.com/minio/minio
- MinIO final official security source release: https://github.com/minio/minio/releases/tag/RELEASE.2025-10-15T17-29-55Z
- MinIO Client image tags: https://hub.docker.com/r/minio/mc/tags
- ClickHouse Docker Official Image: https://hub.docker.com/_/clickhouse
- Keycloak releases: https://github.com/keycloak/keycloak/releases
- Keycloak container guide: https://www.keycloak.org/server/containers
- Keycloak health checks: https://www.keycloak.org/observability/health

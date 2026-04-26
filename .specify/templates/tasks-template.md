---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include test tasks required by the constitution. Unit tests are
required for core business rules. Angular tests are required for frontend
interaction logic and dashboard behavior when implemented. Integration tests are
required where practical for infrastructure boundaries. E2E tasks are required
for main URL creation, redirect, event publication, batch processing, and
dashboard query flows when the feature touches those flows. Redirect load scripts
are required when redirect traffic behavior changes.

**Organization**: Tasks are grouped by user story to enable independent
implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files and has no
  dependency on another incomplete task.
- **[Story]**: Which user story this task belongs to, such as US1, US2, or US3.
- Include exact file paths in descriptions.

## Path Conventions

- ApiGateway: `src/ApiGateway/`
- User service: `src/User/`
- Shortener API / Redirect Service: `src/Shortener/`
- Statistics.Api (Dashboard API): `src/Statistics/DistributedUrlShortener.Statistics.Api/`
- Statistics.EventWriter.Worker: `src/Statistics/DistributedUrlShortener.Statistics.EventWriter.Worker/`
- Statistics.BatchProcessor.Worker: `src/Statistics/DistributedUrlShortener.Statistics.BatchProcessor.Worker/`
- Statistics.Infrastructure: `src/Statistics/DistributedUrlShortener.Statistics.Infrastructure/`
- Frontend: `src/Front/`
- Infrastructure: `infra/`
- Documentation: `docs/`
- Scripts: `scripts/`
- Tests: `tests/e2e/`, `tests/integration/`, `tests/load/`

<!--
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.

  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md, with priorities P1, P2, P3...
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/
  - Constitution checks from plan.md

  Tasks MUST be organized by user story so each story can be implemented,
  tested, and demonstrated independently.

  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, service scaffolding, and local reproducibility

- [ ] T001 Create or update project structure per implementation plan
- [ ] T002 Configure required service dependencies for [feature] in Docker Compose
- [ ] T003 [P] Configure linting and formatting for touched service(s)
- [ ] T004 [P] Update `.env.example` for new required configuration
- [ ] T005 [P] Add or update health checks for touched service(s)
- [ ] T006 [P] Configure Angular workspace dependencies in src/Front/ if frontend is touched

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story can
be implemented

**CRITICAL**: No user story work can begin until this phase is complete.

Examples of foundational tasks; adjust based on the plan:

- [ ] T007 Define Cassandra table/query model and TTL behavior in docs or migrations
- [ ] T008 Configure Redis-first and Cassandra-fallback shortCode lookup if redirect path is touched
- [ ] T009 Configure Shortener API quota enforcement if URL creation is touched
- [ ] T010 Configure Redpanda/Kafka topic, ClickEvent schema, and Statistics.EventWriter.Worker consumer contract if analytics is touched
- [ ] T011 Configure MinIO/S3 Parquet layout and Statistics.BatchProcessor.Worker DuckDB batch entry point and schedule if batch processing is touched
- [ ] T012 Configure ClickHouse aggregate schema and Statistics.Api dashboard query contract if dashboard analytics is touched
- [ ] T013 Configure Keycloak/Ocelot route and auth policy if authenticated API is touched
- [ ] T014 Define Angular routes, guards, API clients, API contracts, loading states, and error states if frontend is touched
- [ ] T015 Add structured logging, metrics, tracing, and correlation ID plumbing
- [ ] T016 Add invalid configuration validation and fail-fast startup behavior

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - [Title] (Priority: P1) MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1

> **NOTE**: Write tests first for business rules and externally visible contracts.

- [ ] T017 [P] [US1] Unit test for [business rule] in tests/[path]/When_[Scenario]_Should_[ExpectedBehavior].[ext]
- [ ] T018 [P] [US1] Angular component/service test in src/Front/ for [frontend behavior]
- [ ] T019 [P] [US1] Integration test for [infrastructure boundary] in tests/integration/[path]
- [ ] T020 [P] [US1] Contract or API test for [endpoint] in tests/[path]

### Implementation for User Story 1

- [ ] T021 [P] [US1] Create or update [model/entity] in [exact path]
- [ ] T022 [US1] Implement [service/use case] in [exact path]
- [ ] T023 [US1] Implement [endpoint/handler/worker/component] in [exact path] as the correct separate executable/process
- [ ] T024 [US1] Add validation and error handling
- [ ] T025 [US1] Add structured logs, metrics, traces, and correlation IDs
- [ ] T026 [US1] Update documentation for user story behavior in docs/ or README.md

**Checkpoint**: User Story 1 is functional and testable independently.

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2

- [ ] T027 [P] [US2] Unit test for [business rule] in tests/[path]/When_[Scenario]_Should_[ExpectedBehavior].[ext]
- [ ] T028 [P] [US2] Angular component/service test in src/Front/ for [frontend behavior]
- [ ] T029 [P] [US2] Integration test for [infrastructure boundary] in tests/integration/[path]
- [ ] T030 [P] [US2] E2E test for [journey] in tests/e2e/[path]

### Implementation for User Story 2

- [ ] T031 [P] [US2] Create or update [model/entity] in [exact path]
- [ ] T032 [US2] Implement [service/use case] in [exact path]
- [ ] T033 [US2] Implement [endpoint/handler/worker/component] in [exact path] as the correct separate executable/process
- [ ] T034 [US2] Add observability and health check coverage
- [ ] T035 [US2] Update documentation for user story behavior in docs/ or README.md

**Checkpoint**: User Stories 1 and 2 work independently.

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3

- [ ] T036 [P] [US3] Unit test for [business rule] in tests/[path]/When_[Scenario]_Should_[ExpectedBehavior].[ext]
- [ ] T037 [P] [US3] Angular component/service test in src/Front/ for [frontend behavior]
- [ ] T038 [P] [US3] Integration test for [infrastructure boundary] in tests/integration/[path]
- [ ] T039 [P] [US3] E2E test for [journey] in tests/e2e/[path]

### Implementation for User Story 3

- [ ] T040 [P] [US3] Create or update [model/entity] in [exact path]
- [ ] T041 [US3] Implement [service/use case] in [exact path]
- [ ] T042 [US3] Implement [endpoint/handler/worker/component] in [exact path] as the correct separate executable/process
- [ ] T043 [US3] Add observability and health check coverage
- [ ] T044 [US3] Update documentation for user story behavior in docs/ or README.md

**Checkpoint**: All selected user stories are independently functional.

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Constitution & Polish

**Purpose**: Cross-cutting quality gates that affect multiple user stories

- [ ] TXXX [P] Update architecture, capacity, database, analytics, observability, or trade-off docs in docs/
- [ ] TXXX [P] Update README local run instructions when runnable behavior changes
- [ ] TXXX [P] Add or update redirect load test scripts in tests/load/
- [ ] TXXX Verify Docker Compose local setup from a clean environment
- [ ] TXXX Verify no secrets are committed and `.env.example` is complete
- [ ] TXXX Run all required unit, integration, e2e, and load checks
- [ ] TXXX Code cleanup and refactoring within established service boundaries

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phase 3+)**: Depend on Foundational phase completion
- **Constitution & Polish**: Depends on all selected user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational phase
- **User Story 2 (P2)**: Can start after Foundational phase and may integrate with US1
- **User Story 3 (P3)**: Can start after Foundational phase and may integrate with US1/US2

### Within Each User Story

- Tests for business rules and public contracts before implementation
- Data models and contracts before services
- Services before endpoints, handlers, or workers
- Core implementation before observability verification
- Story complete before moving to the next priority unless parallel ownership is explicit

### Parallel Opportunities

- Setup tasks marked [P] can run in parallel
- Foundational tasks marked [P] can run in parallel when they touch different files
- Tests for a user story marked [P] can run in parallel
- Different user stories can proceed in parallel after foundational tasks if file ownership is clear

---

## Parallel Example: User Story 1

```text
Task: "Unit test for plan limit in tests/..."
Task: "Integration test for Redis/Cassandra lookup in tests/integration/..."
Task: "Contract test for POST /urls in tests/..."
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Stop and validate User Story 1 independently
5. Demo locally through Docker Compose when applicable

### Incremental Delivery

1. Complete Setup and Foundational phases
2. Add User Story 1, test independently, then demo
3. Add User Story 2, test independently, then demo
4. Add User Story 3, test independently, then demo
5. Preserve prior story behavior at each increment

### Parallel Team Strategy

1. Team completes Setup and Foundational phases together
2. Once Foundational is done, assign clear file ownership per service/story
3. Integrate completed stories through contracts and e2e checks

---

## Notes

- [P] tasks must touch different files and have no dependency on incomplete work.
- [Story] labels map tasks to user stories for traceability.
- Use real repository paths in every task.
- Keep redirect, analytics, dashboard, and user-management concerns decoupled.
- Keep APIs, event consumers, and batch processors as separate runtime applications.
- Avoid vague tasks, same-file conflicts, and cross-story dependencies that block independent validation.

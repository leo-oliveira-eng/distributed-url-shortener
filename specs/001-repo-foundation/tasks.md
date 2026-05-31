# Tasks: Phase 0 Repository Foundation

**Input**: Design documents from `/specs/001-repo-foundation/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/phase0-runtime-contracts.md, quickstart.md

**Tests**: Phase 0 has no business rules, frontend behavior, infrastructure boundaries, e2e flows, redirect traffic behavior, cache lookup, event publishing, event consumption, scheduling, or batch processing to test. Required verification is limited to Shortener solution build, Python Statistics shell install/run checks, API shell `/health` checks, Compose configuration rendering, documentation review, and out-of-scope behavior checks.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files and has no dependency on another incomplete task.
- **[Story]**: Which user story this task belongs to, such as US1, US2, US3, or US4.
- Include exact file paths in descriptions.

## Path Conventions

- ApiGateway: `src/ApiGateway/`
- User service: `src/User/`
- Shortener API / Redirect Service: `src/Shortener/`
- Statistics.Api (Python FastAPI shell): `src/Statistics/statistics_api/`
- Statistics.EventWriter.Worker (Python worker shell): `src/Statistics/statistics_event_writer/`
- Statistics.BatchProcessor.Worker (Python worker shell): `src/Statistics/statistics_batch_processor/`
- Statistics shared package/module: `src/Statistics/shared/`
- Frontend: `src/Front/`
- Infrastructure: `infra/`
- Documentation: `docs/`
- Scripts: `scripts/`
- Tests: `tests/e2e/`, `tests/integration/`, `tests/load/`

## Phase 1: Setup (Shared Foundation)

**Purpose**: Prepare shared repository scaffolding and avoid trampling existing user work.

- [x] T001 Inspect existing files and placeholders in `src/`, `infra/`, `docs/`, `scripts/`, `tests/`, `CHANGELOG.md`, `.gitignore`, and `.windsurf/` before editing.
- [x] T002 [P] Create or validate root foundation folders `src/`, `infra/`, `docs/`, `scripts/`, and `tests/`.
- [x] T003 [P] Create or validate bounded-context placeholders `src/ApiGateway/`, `src/User/`, `src/Shortener/`, `src/Statistics/`, and `src/Front/`.
- [x] T004 [P] Create or validate test category placeholders `tests/e2e/`, `tests/integration/`, and `tests/load/`.
- [x] T005 Add `.gitkeep` placeholders only where needed in empty folders under `src/ApiGateway/`, `src/User/`, `src/Front/`, `infra/`, `scripts/`, `tests/e2e/`, `tests/integration/`, and `tests/load/`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the solution and shared constraints that all user-story work depends on.

**CRITICAL**: No user story work can begin until this phase is complete.

- [x] T006 Create `DistributedUrlShortener.sln` at the repository root.
- [x] T007 Create or update `.env.example` with safe Phase 0 variables for compose project name, environment, and API shell ports.
- [x] T008 Create initial `docker-compose.yml` at the repository root using `.env.example` variables without adding Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, or observability services.
- [x] T009 Create `docs/architecture.md` sections for bounded contexts, request flow, redirect independence, authenticated API flow, analytics flow, and Phase 0 limitations.
- [x] T010 Create `docs/tradeoffs.md` sections for Redirect Service separation, Statistics runtime separation, shell-first implementation, Compose baseline, and known Phase 0 limitations.
- [x] T011 Update `CHANGELOG.md` under `Unreleased` to reserve entries for Phase 0 foundation files, solution, project shells, documentation, and validation.

**Checkpoint**: Foundation prerequisites are ready; user story implementation can begin.

---

## Phase 3: User Story 1 - Validate Repository Foundation (Priority: P1) MVP

**Goal**: A maintainer can inspect the repository and immediately understand where each bounded context, runtime, infrastructure asset, script, document, and test category belongs before later phases add business behavior.

**Independent Test**: Review the repository tree and confirm that all required top-level folders and bounded-context folders exist with names that match the constitution.

### Implementation for User Story 1

- [x] T012 [P] [US1] Create `src/Shortener/Shortener.Api/` for the Shortener API runtime shell.
- [x] T013 [P] [US1] Create `src/Shortener/Redirect.Service/` for the Redirect Service runtime shell.
- [x] T014 [P] [US1] Create `src/Statistics/statistics_api/` for the Statistics.Api Python FastAPI shell.
- [x] T015 [P] [US1] Create `src/Statistics/statistics_event_writer/` for the Statistics.EventWriter.Worker Python worker shell.
- [x] T016 [P] [US1] Create `src/Statistics/statistics_batch_processor/` for the Statistics.BatchProcessor.Worker Python worker shell.
- [x] T017 [P] [US1] Create `src/Statistics/shared/` for shared Statistics Python package/module code with no business logic.
- [x] T018 [US1] Remove obsolete `.gitkeep` placeholders from folders that now contain real project files under `src/Shortener/` and `src/Statistics/`.
- [x] T019 [US1] Verify repository structure against `specs/001-repo-foundation/spec.md` and `specs/001-repo-foundation/data-model.md`.

**Checkpoint**: User Story 1 is complete when all required structure exists and no runtime boundary is ambiguous.

---

## Phase 4: User Story 2 - Build Initial Runtime Shells (Priority: P1)

**Goal**: A developer can build the Shortener .NET solution and verify that every Statistics Python shell is represented as a separate runnable application without any later-phase business behavior.

**Independent Test**: Open `DistributedUrlShortener.sln`, run the documented .NET build command for Shortener projects, then run the documented Python install/start checks for Statistics shells.

### Implementation for User Story 2

- [x] T020 [P] [US2] Create minimal web project file `src/Shortener/Shortener.Api/Shortener.Api.csproj`.
- [x] T021 [P] [US2] Create minimal web project file `src/Shortener/Redirect.Service/Redirect.Service.csproj`.
- [x] T022 [P] [US2] Create minimal Python project configuration `src/Statistics/pyproject.toml` for Statistics shells using Python 3.12+, including FastAPI and the minimal server dependency for Statistics.Api.
- [x] T023 [P] [US2] Create minimal Python package marker `src/Statistics/statistics_api/__init__.py`.
- [x] T024 [P] [US2] Create minimal Python package marker `src/Statistics/statistics_event_writer/__init__.py`.
- [x] T025 [P] [US2] Create minimal Python package marker `src/Statistics/statistics_batch_processor/__init__.py`.
- [x] T026 [P] [US2] Create minimal health-only startup in `src/Shortener/Shortener.Api/Program.cs`.
- [x] T027 [P] [US2] Create minimal `/health`-only startup in `src/Shortener/Redirect.Service/Program.cs` without `GET /{shortCode}`, redirect logic, cache lookup, or event publishing.
- [x] T028 [P] [US2] Create minimal health-only FastAPI startup in `src/Statistics/statistics_api/main.py`.
- [x] T029 [P] [US2] Create minimal Python worker startup in `src/Statistics/statistics_event_writer/main.py` that starts without consuming events.
- [x] T030 [P] [US2] Create minimal Python worker startup in `src/Statistics/statistics_batch_processor/main.py` that starts without scheduling jobs or processing batches.
- [x] T031 [P] [US2] Create minimal package marker `src/Statistics/shared/__init__.py` with no business logic.
- [x] T032 [US2] Add only the Shortener API and Redirect Service .NET project files to `DistributedUrlShortener.sln`.
- [x] T033 [US2] Ensure Shortener API and Redirect Service project files use environment-based configuration and nullable reference types where applicable.
- [x] T034 [US2] Run `dotnet restore DistributedUrlShortener.sln` from the repository root.
- [x] T035 [US2] Run `dotnet build DistributedUrlShortener.sln --no-restore` from the repository root.
- [x] T036 [US2] Manually verify `/health` for `src/Shortener/Shortener.Api/Shortener.Api.csproj`.
- [x] T037 [US2] Manually verify `/health` for `src/Shortener/Redirect.Service/Redirect.Service.csproj`.
- [x] T038 [US2] Install or run the Python Statistics shells according to `src/Statistics/pyproject.toml` and manually verify `/health` for `src/Statistics/statistics_api/main.py`.
- [x] T039 [US2] Verify worker shells start without performing business work in `src/Statistics/statistics_event_writer/main.py` and `src/Statistics/statistics_batch_processor/main.py`.

**Checkpoint**: User Story 2 is complete when the Shortener solution builds, the Statistics Python shells install or start, and all runtime shells satisfy `specs/001-repo-foundation/contracts/phase0-runtime-contracts.md`.

---

## Phase 5: User Story 3 - Use Local Configuration Baseline (Priority: P2)

**Goal**: A developer can understand the expected local configuration and Docker Compose baseline without relying on committed secrets or manually inventing environment variable names.

**Independent Test**: Review `.env.example` and `docker-compose.yml`; confirm that they are aligned, secret-free, and structured so Phase 1 infrastructure services can be added without replacing the baseline.

### Implementation for User Story 3

- [x] T040 [US3] Align `.env.example` variables with API shell ports and application environment used by `docker-compose.yml`.
- [x] T041 [US3] Define only minimal application shell services or a minimal baseline in `docker-compose.yml` without adding Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, or observability services.
- [x] T042 [US3] Add comments in `docker-compose.yml` that require Phase 1 infrastructure to extend this file rather than replace it, without embedding secrets or fake infrastructure behavior.
- [x] T043 [US3] Run `docker compose --env-file .env.example config` from the repository root.
- [x] T044 [US3] Verify `.env.example` and `docker-compose.yml` contain no real secrets, tokens, private endpoints, or production credentials.

**Checkpoint**: User Story 3 is complete when Compose renders successfully and the local configuration baseline is safe and easy to extend.

---

## Phase 6: User Story 4 - Understand Architecture and Trade-offs (Priority: P2)

**Goal**: A reviewer can read the documentation baseline and understand the intended architecture, runtime boundaries, redirect-path independence, authenticated API routing, and known limitations of Phase 0.

**Independent Test**: Review `docs/architecture.md` and `docs/tradeoffs.md`; confirm that both are written in English and explicitly cover the required Phase 0 architectural decisions.

### Implementation for User Story 4

- [x] T045 [US4] Write high-level architecture overview in `docs/architecture.md`.
- [x] T046 [US4] Document bounded contexts ApiGateway, User, Shortener, Statistics, and Front in `docs/architecture.md`.
- [x] T047 [US4] Document future public `GET /{shortCode}` ownership by Redirect Service outside ApiGateway in `docs/architecture.md`, and state that Phase 0 allows only `/health`.
- [x] T048 [US4] Document authenticated API flow through ApiGateway in `docs/architecture.md`.
- [x] T049 [US4] Document planned high-level analytics flow in `docs/architecture.md` without claiming analytics implementation.
- [x] T050 [US4] Document why Redirect Service is separate from Shortener API in `docs/tradeoffs.md`.
- [x] T051 [US4] Document why Statistics.Api, Statistics.EventWriter.Worker, and Statistics.BatchProcessor.Worker are separate Python runtimes, and why shared Statistics code is a Python package/module with no business logic, in `docs/tradeoffs.md`.
- [x] T052 [US4] Document why Phase 0 creates buildable shells before business behavior in `docs/tradeoffs.md`.
- [x] T053 [US4] Document known Phase 0 limitations and out-of-scope behavior in `docs/tradeoffs.md`.
- [x] T054 [US4] Update `README.md` only if needed to point to `docs/architecture.md`, `docs/tradeoffs.md`, and the Phase 0 local verification flow.
- [x] T055 [US4] Update `CHANGELOG.md` with completed Phase 0 foundation documentation changes.

**Checkpoint**: User Story 4 is complete when a reviewer can identify runtime boundaries and Phase 0 limitations from documentation in under 5 minutes.

---

## Phase 7: Constitution & Polish

**Purpose**: Cross-cutting quality gates that affect multiple user stories.

- [x] T056 [P] Verify all generated project docs and project-doc comments are in English in `README.md`, `docs/architecture.md`, `docs/tradeoffs.md`, `CHANGELOG.md`, and `specs/001-repo-foundation/tasks.md`.
- [x] T057 [P] Verify no later-phase business behavior exists in `src/Shortener/Shortener.Api/Program.cs`, `src/Shortener/Redirect.Service/Program.cs`, `src/Statistics/statistics_api/main.py`, `src/Statistics/statistics_event_writer/main.py`, `src/Statistics/statistics_batch_processor/main.py`, and `src/Statistics/shared/__init__.py`.
- [x] T058 Verify quickstart steps in `specs/001-repo-foundation/quickstart.md` from a clean terminal session.
- [x] T059 Verify acceptance criteria from `specs/001-repo-foundation/spec.md` against the final repository state.
- [x] T060 Run `git status --short` and review changed files to confirm no unrelated user work was reverted.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion; blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational completion.
- **User Story 2 (Phase 4)**: Depends on User Story 1 structure being present.
- **User Story 3 (Phase 5)**: Depends on Foundational completion and benefits from User Story 2 port/project names.
- **User Story 4 (Phase 6)**: Depends on Foundational completion and can run alongside User Story 2 or User Story 3 after paths are stable.
- **Constitution & Polish (Phase 7)**: Depends on all selected user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: MVP; validates the repository foundation structure.
- **User Story 2 (P1)**: Requires US1 runtime folders and completes buildable runtime shells.
- **User Story 3 (P2)**: Can start after Phase 2, but final alignment should happen after US2 confirms project names and ports.
- **User Story 4 (P2)**: Can start after Phase 2, but final wording should reflect US1-US3 implementation decisions.

### Within Each User Story

- Create or validate structure before removing placeholders.
- Create Shortener project files before adding Shortener projects to the solution.
- Create startup files before build and run verification.
- Align `.env.example` before validating `docker-compose.yml`.
- Write architecture and trade-off docs before final changelog completion.

### Parallel Opportunities

- T002, T003, and T004 can run in parallel because they touch different folder groups.
- T012 through T017 can run in parallel because each task owns a different runtime folder.
- T020 through T031 can run in parallel after US1 because each task owns a different project or startup file.
- T045 through T049 can run in parallel with T050 through T053 because they touch different documentation files.
- T056 and T057 can run in parallel during polish because they inspect different quality dimensions.

---

## Parallel Example: User Story 2

```text
Task: "Create minimal web project file src/Shortener/Shortener.Api/Shortener.Api.csproj"
Task: "Create minimal web project file src/Shortener/Redirect.Service/Redirect.Service.csproj"
Task: "Create minimal Python project configuration src/Statistics/pyproject.toml"
Task: "Create minimal Python health shell src/Statistics/statistics_api/main.py"
Task: "Create minimal Python worker shell src/Statistics/statistics_event_writer/main.py"
Task: "Create minimal Python worker shell src/Statistics/statistics_batch_processor/main.py"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate the repository tree against the constitution.

### Incremental Delivery

1. Complete Setup and Foundational phases.
2. Add User Story 1 structure and validate boundaries.
3. Add User Story 2 runtime shells and build verification.
4. Add User Story 3 local configuration baseline.
5. Add User Story 4 documentation and changelog updates.
6. Run Constitution & Polish checks.

### Parallel Team Strategy

1. One owner prepares shared setup and solution baseline.
2. Separate owners scaffold independent runtime folders and project files.
3. Documentation owner writes architecture and trade-off docs once paths are stable.
4. Final owner runs build, Compose, health, and constitution verification.

---

## Notes

- Keep every task implementation in English when writing documentation or comments.
- Do not implement URL creation, redirect resolution, event publishing, event writing, batch processing, dashboard queries, authentication, Angular frontend behavior, observability stack, or load tests.
- Redirect Service must expose only `/health` in Phase 0; do not implement `GET /{shortCode}`, cache lookup, or event publishing.
- Everything inside `src/Statistics/` must be Python project/package shells only, and shared Statistics code must contain no business logic in Phase 0.
- `docker-compose.yml` must not include Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, or observability services in Phase 0; Phase 1 must extend, not replace, the baseline.
- Preserve existing user changes in `.gitignore` and `.windsurf/`.
- Use `apply_patch` for manual edits, non-interactive `dotnet` commands for Shortener solution verification, and non-interactive Python commands for Statistics shell verification.

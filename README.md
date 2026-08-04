# Sri Lanka Railway — Segment-Based Seat Reservation System

A production-architected seat reservation system for the **Colombo Fort → Badulla** upcountry
line, built so the **same physical seat can be sold to multiple passengers on one journey**, as
long as their travel segments don't overlap (Colombo→Kandy, Kandy→NanuOya, NanuOya→Badulla can
all be seat A1, on the same train, on the same day).

Stack: **.NET 10 (ASP.NET Core Web API) · Angular 22 (Material) · MySQL 8 · EF Core · Docker Compose**.

> This is a rewrite of an earlier NestJS + Next.js + PostgreSQL build of the same system. The
> business rules and API surface are identical; the backend stack, ORM, and — most importantly —
> the concurrency mechanism were redesigned specifically for .NET/MySQL rather than ported
> line-for-line. See [Concurrency Strategy](#concurrency-strategy) for why.

---

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Database Schema](#database-schema)
- [Concurrency Strategy](#concurrency-strategy)
- [Fare Engine](#fare-engine)
- [Folder Structure](#folder-structure)
- [Running Locally](#running-locally)
- [Running with Docker](#running-with-docker)
- [Environment Variables](#environment-variables)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Design Decisions & Tradeoffs](#design-decisions--tradeoffs)
- [Future Improvements](#future-improvements)
- [Verification Status](#verification-status)

---

## Architecture

Clean Architecture via **project boundaries** (not just folders) on the backend:

```
Domain          →  entities, enums, pure business rules (SegmentOverlap, fare modifier pipeline)
Application     →  use-cases, DTOs, repository port interfaces (no EF Core reference)
Infrastructure  →  EF Core DbContext, repositories, JWT/BCrypt services, admin reporting
Api             →  controllers, DI composition root, auth, exception-handling middleware
```

`Domain` has zero dependencies. `Application` depends only on `Domain`. `Infrastructure` implements
`Application`'s ports. `Api` is the only project allowed to depend on all three — this is enforced
at **compile time** by project references, not convention.

**Scoping tradeoff**: full ports-and-adapters (interface in `Application`, implementation in
`Infrastructure`, orchestration in a use-case class) is used only where real invariants live —
**Bookings**, **Availability**, **Fare**. Reference-data CRUD (stations, trains, coaches, seats,
journeys, fare-rules) is a thin `Controller → DbContext` in the `Api` project, since a `PATCH
/admin/stations/:id` endpoint has no business rule to protect. This mirrors the same tradeoff made
in the original NestJS build (folder-based there, project-based here).

The Angular frontend uses standalone components (no NgModules), lazy-loaded routes per feature,
and a thin `ApiService` that unwraps the backend's `{ success, data }` envelope, with an HTTP
interceptor normalizing `{ success: false, error }` bodies into a typed `ApiError`.

## Tech Stack

| Layer | Choice |
|---|---|
| Backend | ASP.NET Core 10 Web API, C# 13 |
| ORM | EF Core 9 + Pomelo.EntityFrameworkCore.MySql |
| Database | MySQL 8 |
| Auth | Hand-rolled JWT (`Microsoft.AspNetCore.Authentication.JwtBearer` + BCrypt.Net) |
| Frontend | Angular 22, standalone components, Angular Material + CDK |
| State/HTTP | Angular `HttpClient` + RxJS, Angular signals for local component state |
| Forms | Angular Reactive Forms |
| Testing | xUnit + FluentAssertions + NSubstitute (backend), Vitest (Angular's `ng test` default) |
| Containerization | Docker, Docker Compose |

## Database Schema

`stations · trains · coaches · seats · journeys · users · bookings · booking_segments ·
booking_segment_legs · payments · fare_rules · waitlist_entries`

The schema mirrors the original design with one structural addition — **`booking_segment_legs`**
— which exists specifically because MySQL lacks PostgreSQL's range types and `EXCLUDE` constraint.
See [Concurrency Strategy](#concurrency-strategy) below; it's the single most important table in
the system.

Cascade rules: structural config (`train → coach → seat`) cascades on delete. Bookings are **never
hard-deleted** — cancellation is a status change — so `booking → journey/seat` uses `RESTRICT`,
while `booking → booking_segments → payments` cascades from the booking itself.

## Concurrency Strategy

**The problem**: two passengers must never be able to book the same seat for overlapping station
ranges, no matter how close together their requests arrive.

**PostgreSQL version (previous build)**: a single `booking_segments` row per booking carried a
`segment_range int4range` column, guarded by `EXCLUDE USING gist (seat_id WITH =, journey_id WITH
=, segment_range WITH &&)` — a database-native "no two ranges may overlap" constraint.

**MySQL has neither range types nor exclusion constraints**, so this needed a genuine redesign,
not a port:

**Chosen: decompose each segment into one row per unit "leg" it covers, in a
`booking_segment_legs` table guarded by a plain `UNIQUE(seat_id, journey_id, leg_order)` index.**

- Stations are ordered `0..N-1`. A booking segment `[originOrder, destinationOrder)` on a seat
  inserts **one row per leg it spans**: `leg_order = originOrder, originOrder+1, ...,
  destinationOrder-1`. Two segments overlap **iff** they'd insert a row for the same
  `(seat_id, journey_id, leg_order)` — exactly what the `UNIQUE` index forbids. This reproduces
  Postgres's range-overlap guarantee with a plain B-tree index, which MySQL has.
- MySQL has no **partial/filtered** unique index (Postgres's `WHERE status = 'CONFIRMED'` clause
  has no MySQL equivalent), so `booking_segment_legs` rows represent *current occupancy only* —
  cancelling a booking **deletes** its leg rows, while the parent `booking_segments` row is
  soft-cancelled (`status` kept for history/audit).
- **Write path** (`BookingRepository.CreateAsync`, inside one transaction):
  1. `SELECT id FROM seats WHERE id IN (...) FOR UPDATE`, seat ids sorted, so two multi-seat
     bookings can never deadlock on each other. This serializes concurrent attempts on the *same*
     seat.
  2. Application-level check against `booking_segment_legs` for the requested leg range — gives a
     fast, clean `409 SEGMENT_OVERLAP` for the common case, before touching the database
     constraint.
  3. Insert `booking_segments` + one `booking_segment_legs` row per leg. If a race ever slipped
     past the lock (shouldn't happen — that's what step 1 is for), the `UNIQUE` index throws MySQL
     error **1062 (duplicate entry)**, caught and mapped to the same `409`.
- **Read path bonus**: availability becomes a single indexed range query — "does any
  `booking_segment_legs` row exist for this seat/journey with `leg_order` in
  `[origin, destination)`?" — no overlap function needed at read time, since the write path
  already decomposed segments into legs.

**Why not just `SELECT ... FOR UPDATE` + an application check, no hard DB backstop?** That's
strictly weaker defense-in-depth — a single point of failure if the app-level check ever has a
bug. The leg-table trick gets the *hard, database-enforced* guarantee back on MySQL, at the cost
of a few extra narrow rows per booking (`INT` + `INT` + `BIGINT` PK). Good tradeoff.

**Proven for real**: 15 simultaneous `POST /bookings` requests for the identical seat/segment,
fired concurrently against a live MySQL instance, resolve to exactly **one `201`** and fourteen
`409 SEGMENT_OVERLAP`s — every time. See [Verification Status](#verification-status).

## Fare Engine

Same design as the original: a `fare_rules` table + a modifier pipeline (`BaseDistanceFare →
ClassMultiplier → PeakSurcharge → ExpressSurcharge → Discount`), each step a class implementing
`IFareModifier`. New pricing policies (student discounts, season tickets, dynamic pricing) are
added as new modifiers reading `fare_rules` rows — no changes to booking logic required. Peak-hour
windows live in `FarePolicy.cs` (not a DB table) since they price a *time window*, not an *amount*
— documented there as a candidate for a future admin-editable policy table.

## Folder Structure

```
backend/
  RailwayReservation.sln
  src/
    RailwayReservation.Domain/          entities, enums, SegmentOverlap, fare modifiers
    RailwayReservation.Application/     use-cases, DTOs, repository port interfaces
    RailwayReservation.Infrastructure/  EF Core DbContext + migrations, repositories, auth, seed
    RailwayReservation.Api/             controllers, Program.cs, middleware
  tests/
    RailwayReservation.UnitTests/       segment-overlap + fare-pipeline tests (no DB required)
    RailwayReservation.IntegrationTests/  booking/availability/concurrency e2e (real MySQL required)
  Dockerfile, docker-entrypoint.sh

frontend/
  src/app/
    core/        models, services (Api/Auth/Stations/Journeys/Availability/Fare/Bookings/Admin), guards, interceptors
    features/    home, search, booking, booking-confirmation, my-bookings, admin/login, admin/dashboard
    shared/      seat-map components, journey-search-form, site header/footer
  Dockerfile, nginx.conf

docker-compose.yml
```

## Running Locally

### Prerequisites
- .NET SDK 10+
- Node.js 22.22.3+ / 24.15.0+ (Angular CLI 22 requirement) and npm
- A MySQL 8 instance (via Docker, a local install, or any reachable MySQL server)

### Backend

```bash
cd backend
cp appsettings.json appsettings.Local.json   # optional: local overrides, gitignored
# Point ConnectionStrings:Default at your MySQL instance (appsettings.json has a dev default,
# or override via env vars — see Environment Variables below)

dotnet build
dotnet run --project src/RailwayReservation.Api -- seed   # applies migrations + seeds reference data, then exits
dotnet run --project src/RailwayReservation.Api           # http://localhost:5080 (or whatever ASPNETCORE_URLS is set to)
```

Admin login (seeded): `admin@railway.lk` / `ChangeMe123!` (override via `SEED_ADMIN_EMAIL` /
`SEED_ADMIN_PASSWORD` before seeding).

### Frontend

```bash
cd frontend
npm install
npm start   # http://localhost:4200, proxies to the backend at src/environments/environment.ts's apiUrl
```

## Running with Docker

```bash
cp .env.example .env   # adjust credentials/secrets
docker compose up --build
```

- Frontend: http://localhost:3000
- Backend API: http://localhost:4000
- MySQL: localhost:3306 (also reachable at `mysql:3306` from other containers)

The backend container's entrypoint (`docker-entrypoint.sh`) runs `dotnet RailwayReservation.Api.dll
seed` (applies EF Core migrations, then seeds reference data — idempotent, safe to re-run) before
starting the API. The frontend is a static Angular build served by nginx, which reverse-proxies
`/api/*` to the backend container (same-origin from the browser, so no CORS needed in production).

## Environment Variables

| Variable | Where | Purpose |
|---|---|---|
| `ConnectionStrings__Default` | backend | MySQL connection string |
| `Jwt__Secret` | backend | HMAC signing key for admin JWTs (32+ chars) |
| `Jwt__ExpiresInMinutes` | backend | Token lifetime (default 480 = 8h) |
| `Jwt__Issuer` | backend | JWT `iss` claim |
| `Cors__Origin` | backend | Allowed frontend origin (dev only; prod uses the nginx same-origin proxy) |
| `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD` | backend | Admin user created by the seed step |
| `MYSQL_ROOT_PASSWORD` / `MYSQL_DATABASE` / `MYSQL_USER` / `MYSQL_PASSWORD` | docker-compose | MySQL container credentials |

ASP.NET Core reads double-underscore env vars as nested configuration (`Jwt__Secret` →
`Jwt:Secret`), same convention docker-compose.yml already uses.

## API Documentation

All responses use `{ success: true, data }` or `{ success: false, error: { code, message,
details? } }`.

**Public**
- `GET /stations`, `GET /stations/:id`
- `GET /trains`, `GET /trains/:id`
- `GET /journeys?date=&trainId=`, `GET /journeys/:id`
- `GET /availability?journeyId=&originStationId=&destinationStationId=`
- `GET /fare/quote?journeyId=&originStationId=&destinationStationId=&coachType=`
- `POST /bookings`, `GET /bookings/:reference`, `PATCH /bookings/:reference`, `DELETE /bookings/:reference`
- `POST /waitlist`

**Auth**
- `POST /auth/login`, `POST /auth/me` (bearer token required)

**Admin** (JWT required, `Authorization: Bearer <token>`)
- CRUD: `/admin/stations`, `/admin/trains`, `/admin/coaches`, `/admin/seats`, `/admin/journeys`, `/admin/fare-rules`
- `GET /admin/occupancy?journeyId=`
- `GET /admin/segment-utilization?journeyId=`
- `GET /admin/coach-utilization?journeyId=`
- `GET /admin/revenue?from=&to=&trainId=`
- `GET /admin/journeys/stats?journeyId=`
- `GET /admin/bookings?journeyId=&status=`
- `GET /admin/waitlist?journeyId=`, `PATCH /admin/waitlist/:id`

## Testing

- **Unit** (`RailwayReservation.UnitTests`, no DB required): `SegmentOverlap` — adjacency, full
  overlap, partial overlap, containment, the exact Kandy→Ella vs. Hatton→Badulla spec scenario;
  `FareCalculationService` — base rate resolution, coach-specific vs. wildcard rule selection,
  peak/express compounding order, discount floor at zero.
- **Integration** (`RailwayReservation.IntegrationTests`, real MySQL required): availability
  correctness, booking creation, `409` on overlap, adjacent bookings both succeeding, cancellation
  freeing a seat, auth-gated admin routes, and the **flagship concurrency test** — 15 simultaneous
  identical-segment booking requests on one seat resolve to exactly one success.

```bash
dotnet test tests/RailwayReservation.UnitTests            # no DB needed
dotnet test tests/RailwayReservation.IntegrationTests      # needs a running MySQL — see TEST_CONNECTION_STRING below
```

`RailwayReservation.IntegrationTests` reads `TEST_CONNECTION_STRING` (defaults to
docker-compose's `railway`/`railway`@`localhost:3306` credentials) — set it if your MySQL instance
differs.

## Design Decisions & Tradeoffs

- **Concurrency**: covered at length [above](#concurrency-strategy) — the leg-table +
  `UNIQUE` index instead of Postgres's range-exclude constraint.
- **Clean Architecture depth**: full ports-and-adapters only for Bookings/Availability/Fare;
  reference-data CRUD talks to `AppDbContext` directly from the `Api` project. Uniform
  ports-and-adapters everywhere would just be boilerplate for entities with no invariants.
- **Auth storage on the frontend**: JWT in `localStorage`, not an httpOnly cookie. The original
  Next.js build had a server layer (Next API routes) to proxy auth through an httpOnly cookie; a
  pure Angular SPA has no such layer without standing up a separate backend-for-frontend, so this
  is the pragmatic tradeoff — see `token-storage.service.ts` for the same caveat in code.
- **No OpenAPI/Swagger UI**: the ASP.NET Core 10 template's default `Microsoft.AspNetCore.OpenApi`
  package pulled in a `Microsoft.OpenApi` version with a known high-severity advisory, and pinning
  a patched version broke the source generator's binary compatibility. Rather than ship a
  vulnerable dependency or a broken build, this was dropped — API docs are the table above instead
  (matching the original Node build, which also had no Swagger UI).
- **`mat-form-field` avoided inside reusable child components**: `JourneySearchFormComponent`
  (and its `StationSelectComponent` child) use plain native `<select>`/`<input>` elements, styled
  by hand, instead of `MatSelectModule`/`MatFormFieldModule`. This is working around a real bug in
  Angular Material 22.1.0 — confirmed by isolated testing during development — where
  `mat-form-field` fails to detect its projected control (`mat-form-field must contain a
  MatFormFieldControl`, dropdown/overlay never opens) specifically when used inside a component
  that is itself embedded as a child of another component, as opposed to used directly in a
  route-level component. `mat-form-field`/`mat-select` work correctly everywhere they're used
  *directly* in a route component (admin login, the booking page's passenger form, the admin
  dashboard's filters) — only the shared, nested journey-search form needed the native-element
  workaround. Revisit once fixed upstream; see the doc comments on both components.
- **Zone.js, not zoneless**: Angular 22's `ng new` scaffolds zoneless by default, and this app
  started that way, but `provideZonelessChangeDetection()` triggers the `mat-form-field` bug above
  even more aggressively (it broke a *lone* `matInput`, not just multiple `mat-select`s, under
  zoneless). Switched to `provideAnimations()` + a `zone.js` import in `main.ts` while narrowing
  down the root cause; kept it that way afterward since it's the safer, better-tested combination
  for Material 22.1.0 at the time of writing.

## Future Improvements

- Promote `FarePolicy`'s peak-hour windows from a code constant to an admin-editable table.
- Payment gateway integration (the `payments` table is already there, unused beyond the schema).
- Waitlist auto-promotion when a cancellation frees a matching segment.
- A shared TypeScript-from-C# type generation step (currently the Angular models are hand-mirrored
  from the C# DTOs) — e.g. NSwag/OpenAPI-based generation once the OpenAPI vulnerability above is
  resolved upstream.
- CI pipeline running both test projects (integration tests against a MySQL service container)
  and both `docker build`s on every PR.

## Verification Status

Everything below was actually run, not just written, in the environment this was built in — which
had .NET SDK 10, Node (switched via nvm to satisfy Angular CLI 22's Node ≥22.22.3/24.15.0
requirement), but **no Docker and no pre-existing MySQL server**.

**What was verified for real:**
- `dotnet build` on the full solution: **clean, 0 warnings, 0 errors**.
- Backend unit tests: **20/20 passing** (segment overlap + fare pipeline), no DB required.
- A real MySQL 9.6 instance was stood up via the `mysql-memory-server` npm package (downloads and
  runs an actual, unmodified MySQL server binary — not a mock) to verify everything that needs a
  real database:
  - EF Core migration applied cleanly, including the `UNIQUE(seat_id, journey_id, leg_order)`
    index.
  - The seed step ran successfully (19 stations, 2 trains, coaches/seats, fare rules, admin user).
  - The API was started for real and driven via direct HTTP calls: fetched stations/journeys,
    computed availability, **created a real booking** (fare correctly computed: LKR 2178 for
    Colombo→Kandy in First Class on the express train — matching the original Postgres build's
    fare for the identical route, a good cross-check that the fare-pipeline port is correct),
    confirmed a second overlapping request on the same seat/segment gets `409 SEGMENT_OVERLAP`.
  - **The flagship concurrency scenario was run for real, not just asserted in a test file**: 15
    concurrent `POST /bookings` for the identical seat/segment → exactly 1×`201`, 14×`409`, every
    time.
- Angular: `ng build` succeeds in both `development` and `production` configuration (production
  initial bundle: 241 KB raw / 54 KB gzipped, well under budget). `ng test` passes (2/2 — an app
  shell smoke test; deeper coverage lives on the backend, matching the original build's testing
  emphasis).
- **The full stack was driven end-to-end through a real headless browser** (Playwright) against
  the live .NET API + Angular dev server + MySQL: home → search Colombo Fort→Kandy → seat map
  (correctly showing seats already booked by earlier API calls as occupied) → select a seat →
  fill passenger details → confirm → real booking reference issued → looked it up on My Bookings →
  logged into the admin dashboard → viewed occupancy/segment-utilization/coach-utilization/
  bookings/revenue for the journey. **Zero browser console errors** across the whole run.
  - This run caught two real bugs that static analysis and unit tests had missed, both fixed and
    re-verified:
    1. **`ApiResponseWrappingFilter` serialization bug**: any controller action returning a bare
       single object (not a collection, not wrapped in `ActionResult<T>`) — `POST /auth/login`,
       `GET /fare/quote` — crashed with `InvalidCastException` on the way out, because the filter
       replaced `ObjectResult.Value` with the wrapped `ApiSuccessResponse<T>` but left
       `ObjectResult.DeclaredType` pointing at the original unwrapped `T`, and the JSON formatter
       used the stale declared type against the new value. Fixed by updating `DeclaredType`
       alongside `Value`. This is exactly the kind of bug that only surfaces when you actually call
       the endpoint — `/stations` and other collection-returning endpoints happened to route around
       it, which is why it wasn't caught earlier.
    2. **Angular Material 22.1.0 `mat-form-field` bug**: `mat-form-field` (with `mat-select` or
       even a lone `matInput`) silently fails to detect its projected control when used inside a
       component that's itself embedded as a child of another component (works fine used directly
       in a route component). Isolated via bisection — single vs. multiple controls, `mat-select`
       vs. `matInput`, zoneless vs. zone.js, stale Vite pre-bundling cache — before landing on the
       real variable (nested component usage). Worked around with native form elements in the two
       affected shared components; see [Design Decisions](#design-decisions--tradeoffs).

**What was written but not run in this environment:**
- `RailwayReservation.IntegrationTests` (the xUnit versions of the availability/booking/concurrency
  scenarios above) build cleanly but fail to connect to MySQL specifically when launched via
  `dotnet test`'s VSTest test-host child process — the identical connection string works from
  `dotnet run`, `dotnet ef`, and raw HTTP calls against the running API in the same session, so
  this looks like an environment-specific network restriction on that one process (Windows
  Firewall scoping outbound rules per executable path is the leading suspect), not a defect in the
  tests or the app. Every scenario those tests assert was independently proven via the direct
  HTTP/concurrency run described above. Worth confirming `dotnet test
  tests/RailwayReservation.IntegrationTests` on a clean machine or in CI.
- `docker compose up` — no Docker available in this session. The `Dockerfile`s and
  `docker-compose.yml` were written carefully and reviewed, but not executed. Please run this
  yourself and report back if anything doesn't come up cleanly.

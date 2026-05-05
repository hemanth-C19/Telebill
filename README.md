# TeleBill — Telehealth Billing & Insurance Claim Portal

> A full-stack, role-based telehealth billing platform covering the complete revenue cycle: charge capture → medical coding → claim building → scrubbing → submission → payment posting → denials & AR management.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Portals & Roles](#portals--roles)
- [Module Breakdown](#module-breakdown)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
  - [Backend](#backend-setup)
  - [Frontend](#frontend-setup)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Telehealth Compliance](#telehealth-compliance)
- [Security](#security)

---

## Overview

TeleBill is a purpose-built **Telehealth Billing & Insurance Claim Submission Portal** designed for clinics and group practices that deliver care through remote, synchronous, and asynchronous telehealth encounters.

Standard billing systems don't natively handle the nuances of telehealth — Place of Service codes (`02`/`10`), synchronous/async modifiers (`95`, `GT`, `FQ`, `93`), and payor-specific telehealth plan rules. TeleBill fills that gap by providing a **complete, end-to-end billing workflow** purpose-built for the remote-care era.

**Phase 1 Scope:**
- X12 837P claim payloads, 835 ERA remittance, and 270/271 eligibility transactions are generated and stored in object storage — no live external clearinghouse transmission.
- In-app notifications only (no email/SMS).
- No payment gateway integration.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         TeleBill System                             │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    React 19 Frontend                         │   │
│  │   Admin │ FrontDesk │ Provider │ Coder │ AR Portals          │   │
│  │         React Router · React Hook Form · Axios               │   │
│  │                      TailwindCSS                             │   │
│  └──────────────────────┬───────────────────────────────────────┘   │
│                         │ REST / JSON                               │
│  ┌──────────────────────▼───────────────────────────────────────┐   │
│  │               ASP.NET Core Web API (.NET 10)                 │   │
│  │                                                              │   │
│  │  Identity │ MasterData │ Encounters │ Coding │ Claims        │   │
│  │  Submissions │ AR & Denials │ Reports │ Notifications        │   │
│  │                                                              │   │
│  │         JWT Auth · FluentValidation · Swagger                │   │
│  └──────────────────────┬───────────────────────────────────────┘   │
│                         │ EF Core 10                                │
│  ┌──────────────────────▼───────────────────────────────────────┐   │
│  │                    SQL Server                                │   │
│  │              32 Tables · Soft Deletes · UTC Timestamps       │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

**Backend pattern:** Controller → Service → Repository → EF Core DbContext  
**Frontend pattern:** Page → Component → API Layer → React Context/Hooks

---

## Tech Stack

### Backend
| Layer | Technology |
|---|---|
| Runtime | .NET 10.0 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 10.0.3 |
| Database | SQL Server (LocalDB / full instance) |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Validation | FluentValidation 11.3.1 |
| API Docs | Swashbuckle / Swagger 10.1.4 |
| OpenAPI | Microsoft.AspNetCore.OpenApi 10.0.3 |

### Frontend
| Layer | Technology |
|---|---|
| UI Framework | React 19.2 |
| Language | TypeScript 6.0 |
| Build Tool | Vite 8.0 |
| Routing | React Router DOM 7.14 |
| Forms | React Hook Form 7.72 |
| HTTP Client | Axios 1.15 |
| Styling | TailwindCSS 4.2 |
| Linting | ESLint 9 + Prettier 3.8 |

---

## Portals & Roles

TeleBill implements **5 role-based portals**, each with a dedicated UI and API surface scoped to that role's responsibilities:

| Portal | Role | Core Responsibilities |
|---|---|---|
| **Admin** | `Admin` | User management, master data (payers, plans, providers, fee schedules), system configuration |
| **FrontDesk** | `FrontDesk` | Patient registration, coverage verification, eligibility checks (270/271), encounter scheduling |
| **Provider** | `Provider` | Encounter documentation, charge capture, attestation, encounter finalization |
| **Coder** | `Coder` | ICD-10-CM diagnosis coding, CPT/HCPCS procedure coding, coding lock management |
| **AR** | `AR` | Claim scrubbing, submission batching, payment posting (835 ERA), denial management, appeal tracking, AR worklist |

---

## Module Breakdown

```
 1. Identity & Access Management   — JWT auth, RBAC, user lifecycle
 2. Master Data Management         — Payers, plans, fee schedules, providers
 3. Patient & Coverage             — Patient registration, insurance, eligibility
 4. Encounters & Charge Capture    — Telehealth encounter records, charge lines
 5. Medical Coding                 — ICD-10-CM / CPT / HCPCS assignment, coding locks
 6. Claim Building                 — Claim assembly from encounter + coding data
 7. Claim Scrubbing                — Rule-based validation engine (ScrubRule / ScrubIssue)
 8. Submission                     — X12 837P generation, batch management, 999/277CA ack tracking
 9. Payment Posting                — 835 ERA parsing, CARC/RARC adjustment posting
10. Denials & Appeals             — Denial capture, appeal workflow
11. AR Management                 — AR worklist, aging buckets, patient statements
12. Reports & Analytics           — CCR, FPAR, DSO, denial rate, TAT metrics
```

---

## Database Schema

32 tables across 7 domains with strict conventions:

- `DECIMAL(10,2)` for all monetary values — never `FLOAT`
- `DATETIME` UTC for all timestamps
- `INT AUTO_INCREMENT` PKs (`BIGINT` for `AuditLog`)
- Soft deletes only — no hard deletes anywhere in the schema
- `AuditLog` is append-only with no `UPDATE`/`DELETE` privileges at the DB level

**Domain map:**

```
Identity & Access    →  User, AuditLog
Master Data          →  Provider, Payer, PayerPlan, FeeSchedule
Patient & Coverage   →  Patient (ePHI), Coverage, EligibilityRef
Encounters           →  Encounter, ChargeLine, Attestation, Diagnosis, CodingLock
Claims               →  Claim, ClaimLine, ScrubRule, ScrubIssue
                        X12_837P_Ref, PriorAuth, AttachmentRef
Submission & Remit   →  SubmissionBatch, SubmissionRef, RemitRef
                        PaymentPost, PatientBalance, Statement
AR & Denials         →  Denial, Appeal, ARWorkitem
Reporting            →  BillingReport, Notification
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- SQL Server (LocalDB ships with Visual Studio, or use SQL Server Express)

---

### Backend Setup

```bash
# Navigate to backend
cd Telebill-Backend

# Restore NuGet packages
dotnet restore

# Apply EF Core migrations to create the database
dotnet ef database update

# Run the API (default: https://localhost:7001)
dotnet run
```

The Swagger UI is available at `https://localhost:7001/swagger` when running in Development mode.

**Connection string** is configured in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Telebill;Integrated Security=true;"
}
```

**JWT configuration** — set your secret key in `appsettings.Development.json`:
```json
"Jwt": {
  "Key": "<your-secret-key-min-32-chars>",
  "Issuer": "TeleBill",
  "Audience": "TeleBillUsers",
  "ExpiryMinutes": 60
}
```

---

### Frontend Setup

```bash
# Navigate to frontend
cd Telebill-Frontend

# Install dependencies
npm install

# Start development server (default: http://localhost:5173)
npm run dev

# Build for production
npm run build
```

**API base URL** — configure in your environment or `src/api/` layer to point to the running backend.

---

## API Documentation

Swagger / OpenAPI documentation is auto-generated and available at runtime:

```
https://localhost:7001/swagger
```

Endpoints are organized by domain controller:

| Controller Group | Base Path |
|---|---|
| Auth | `/api/auth` |
| Users / Identity | `/api/identity` |
| Master Data | `/api/masterdata` |
| Patients & Coverage | `/api/patientcoverage` |
| Encounters | `/api/encounter` |
| Coding | `/api/coding` |
| Claims | `/api/claim` |
| Batch / Submission | `/api/batch` |
| Payment Posting | `/api/posting` |
| AR & Denials | `/api/ar` |
| Reports | `/api/reports` |
| Notifications | `/api/notifications` |

---

## Project Structure

```
Telebill/
├── Telebill-Backend/
│   ├── Controllers/            # API endpoints (12 domain groups)
│   │   ├── AR/
│   │   ├── Batch/
│   │   ├── Claim/
│   │   ├── Coding/
│   │   ├── Encounter/
│   │   ├── IdentityAccess/
│   │   ├── MasterData/
│   │   ├── Notifications/
│   │   ├── PatientCoverage/
│   │   ├── Posting/
│   │   ├── PreCert/
│   │   ├── Reports/
│   │   └── AuthController.cs
│   ├── Services/               # Business logic layer
│   ├── Repositories/          # Data access layer
│   ├── Dto/                   # Request/Response DTOs
│   ├── Models/                # EF Core entity models
│   ├── Data/                  # DbContext
│   ├── Migrations/            # EF Core migrations
│   ├── Validations/           # FluentValidation rule sets
│   ├── Middlewares/
│   ├── Extensions/
│   ├── Program.cs
│   └── Telebill.csproj
│
└── Telebill-Frontend/
    └── src/
        ├── pages/
        │   ├── admin-portal/
        │   ├── AR-portal/
        │   ├── coding-portal/
        │   ├── frontdesk-portal/
        │   ├── provider-portal/
        │   ├── auth/
        │   └── shared/
        ├── components/         # Reusable components per portal
        ├── api/               # Axios API integration layer
        ├── types/             # TypeScript type definitions
        ├── context/           # React Context (auth, portal state)
        ├── hooks/             # Custom React hooks
        └── utils/
```

---

## Telehealth Compliance

TeleBill is built around the specific billing rules that govern telehealth:

**Place of Service codes**
| Code | Description |
|---|---|
| `02` | Telehealth — not patient's home |
| `10` | Telehealth — patient's home |

**Telehealth modifiers**
| Modifier | Description |
|---|---|
| `95` | Synchronous telemedicine via interactive audio/video |
| `GT` | Interactive audio and video telecommunications |
| `FQ` | Audio-only (telephone) telehealth service |
| `93` | Telephone evaluation and management |

**Claim lifecycle**

```
Draft → ScrubError → Ready → Batched → Submitted
      ↓                                     ↓
   (fix issues)              Accepted / Rejected / Denied
                                             ↓
                                    Paid / PartiallyPaid
```

**Encounter lifecycle**
```
Open → ReadyForCoding → Finalized
```

**EDI transactions (generated, reference-only in Phase 1)**
- `837P` — Professional claim submission
- `835` — Electronic Remittance Advice (ERA)
- `270/271` — Eligibility inquiry / response
- `999/277CA` — Acknowledgement / claim status

---

## Security

- **Authentication:** JWT Bearer tokens with configurable expiry
- **Authorization:** Role-based access control — each endpoint is scoped to one or more portal roles
- **PHI Protection:** `Patient` table contains ePHI — encryption at rest required; all access is logged to `AuditLog`
- **AuditLog:** Append-only (`BIGINT` PK), no `UPDATE`/`DELETE` at the DB level; archived after 12 months
- **Input Validation:** FluentValidation on all request DTOs; no raw SQL — all queries via EF Core parameterized queries
- **Soft Deletes:** No hard deletes anywhere — data is always recoverable

---

*TeleBill — Built for the era of remote care.*

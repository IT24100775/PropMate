# PropMate

**Intelligent Property Rental, Marketplace and Verification System**

PropMate is an integrated property management platform that supports
the complete property journey, from listing and verification to
discovery, viewing, transactions, and maintenance. The system combines a
React web application, a Flutter mobile application, an ASP.NET Core Web
API, PostgreSQL, and four Agentic AI services.

------------------------------------------------------------------------

## 1. Project Overview

PropMate provides a single digital platform for property owners,
agents, buyers/renters, administrators, and property managers. It
connects listing management, property discovery, viewing reservations,
rental/purchase transactions, and post-transaction maintenance.

The system contains four main functional components:

1.  **Property Listing & Approval**
2.  **Property Discovery & Viewing**
3.  **Applications, Offers & Transactions**
4.  **Maintenance Management**

Each component is supported by an AI-assisted workflow while important
business decisions remain controlled by human users where appropriate.

## 2. Business Problem

Property rental and sales workflows are often distributed across
separate systems and manual communication channels. This can cause
incomplete or suspicious listings, difficulty finding suitable
properties, disconnected viewing and transaction processes, limited
transaction visibility, and manual maintenance coordination.

PropMate addresses these issues by integrating the property lifecycle
into one system and using Agentic AI to assist with verification,
discovery, negotiation, and maintenance decisions.

## 3. Project Objectives

-   Centralize property rental and sales activities.
-   Support controlled property listing, verification, approval, and
    publication.
-   Improve discovery through search, filtering, maps, and AI-assisted
    recommendations.
-   Provide an integrated viewing reservation workflow.
-   Support rental applications, purchase offers, negotiation,
    agreements, and transaction completion.
-   Assist transaction negotiations while retaining human approval for
    important actions.
-   Support maintenance requests, technician scheduling, expense
    tracking, status updates, and history.
-   Provide secure role-based access.
-   Demonstrate Agentic AI through planning, tool use, decision support,
    and human-in-the-loop execution.

## 4. User Roles

| Role | Primary Application | Responsibilities |
|---|---|---|
| **Buyer/Renter** | Flutter Mobile | Discover properties, view details/maps, save favourites, book viewings, submit applications/offers, negotiate, and confirm agreements. |
| **Owner/Agent** | React Web | Create/manage listings, submit for verification, manage viewings, review applications/offers, negotiate, and confirm agreements. |
| **Admin** | React Web | Review AI verification evidence, approve/reject/request revisions, publish/unpublish listings, and oversee relevant transaction functions. |
| **Property Manager** | React Web | Manage maintenance requests, technicians, AI recommendations, scheduling, status changes, expenses, and history. |

## 5. Core Features

### 5.1 Property Listing & Approval

-   Listing creation and draft management.
-   Sale and rental listings.
-   Property details, location, coordinates, bedrooms, bathrooms, and
    images.
-   Listing submission and lifecycle tracking.
-   AI-assisted property verification.
-   Admin review, approval, rejection, and revision requests.
-   Publish and unpublish approved listings.

### 5.2 Property Discovery & Viewing

-   Browse/search published properties.
-   Property filtering.
-   AI-assisted natural-language discovery.
-   Property details and images.
-   Favourites.
-   Property-specific map/location display.
-   Owner viewing-slot management.
-   Buyer/Renter viewing booking and tracking.

### 5.3 Applications, Offers & Transactions

-   Rental applications and purchase offers.
-   Shared negotiation history.
-   Manual counter-offers.
-   AI-assisted transaction negotiation.
-   Human approval before execution of AI-proposed actions.
-   Agreement generation and confirmation.
-   Transaction completion.
-   Removal of completed rental/sale properties from active discovery
    where applicable.

### 5.4 Maintenance Management

-   Maintenance requests associated with actual properties.
-   Technician management and availability.
-   AI-assisted issue classification.
-   Technician recommendation and scheduling.
-   Human approval of AI recommendations.
-   Status updates, expenses, history, and resolution tracking.

## 6. Technology Stack and Justification

| Technology | Usage | Justification |
|---|---|---|
| React + Vite | Web frontend | Component-based development and fast modern web tooling. |
| Flutter / Dart | Buyer/Renter mobile app | Cross-platform mobile development from one codebase. |
| ASP.NET Core Web API / C# | Main backend | Strongly typed APIs, DI, authentication, validation, and EF Core integration. |
| Entity Framework Core | Data access | ORM support, migrations, and maintainable database access. |
| PostgreSQL / Supabase | Cloud database | Reliable managed relational storage. |
| Python + FastAPI | AI services | Lightweight independent APIs suitable for agent/tool orchestration. |
| Google Gemini | AI reasoning | Generative reasoning, interpretation, classification, and recommendations where configured. |
| JWT | Authentication | Stateless authentication and role-based authorization. |
| Swagger / OpenAPI | API docs/testing | Interactive endpoint documentation and testing. |
| Render | Backend/AI hosting | Deployment of the API and independent Python services. |
| Vercel | React hosting | Production hosting for the Vite/React frontend. |

## 7. System Architecture

``` mermaid
flowchart TB
    Mobile["Flutter Mobile - Buyer/Renter"]
    Web["React Web - Owner/Agent, Admin, Property Manager"]
    API["ASP.NET Core Web API"]
    DB[("PostgreSQL / Supabase")]
    VAgent["Property Verification Agent"]
    DAgent["Property Discovery Agent"]
    TAgent["Transaction Negotiation Agent"]
    MAgent["Maintenance Management Agent"]

    Mobile --> API
    Web --> API
    Web --> MAgent
    API --> DB
    API --> VAgent
    API --> DAgent
    API --> TAgent
    MAgent --> API
```

## 8. Agentic AI Architecture

| Component | AI Service | Main Purpose |
|---|---|---|
| Property Listing & Approval | Property Listing Verification Agent | Assess completeness, legitimacy, risk, and supporting evidence; provide a recommendation for human review. |
| Property Discovery & Viewing | Property Discovery Agent | Interpret natural-language requirements and find suitable published properties. |
| Applications, Offers & Transactions | Transaction Negotiation Agent | Inspect transaction context/history and propose negotiation actions requiring human approval before execution. |
| Maintenance Management | Maintenance Management Agent | Classify issues, identify specialization, find technicians, validate scheduling, and request human approval. |

General pattern:

``` mermaid
flowchart LR
    O[Observe] --> P[Plan]
    P --> T[Execute Tools]
    T --> A[Analyze Evidence]
    A --> D[Recommend / Decide]
    D --> H{Human Approval?}
    H -->|Yes| R[Human Review]
    R --> E[Execute Approved Action]
    H -->|No| E
```

## 9. Database Design

PropMate uses PostgreSQL, hosted with Supabase in the deployed
environment. Entity Framework Core is used by the ASP.NET Core backend
for data access and migrations.

Major data areas include users/roles, property listings/images,
favourites, viewing slots/bookings, rental applications, purchase
offers, transactions, negotiations, agreements, maintenance requests,
technicians, maintenance expenses, maintenance status history, and
workflow-related data where persistence is required.

The exact schema is represented by the EF Core entity models and
migrations under `backend/PropMate.Api`.

## 10. Repository Structure

``` text
PropMate/
├── ai/
│   ├── property-verification-agent/
│   ├── property-discovery-agent/
│   ├── transaction-negotiation-agent/
│   └── maintenance-management-agent/
├── backend/
│   └── PropMate.Api/
├── frontend/
│   └── propmate-web/
├── mobile/
│   └── propmate_mobile/
├── .gitignore
└── README.md
```

## 11. Prerequisites

-   Git
-   .NET SDK compatible with the backend project
-   Node.js and npm
-   Flutter SDK and Android development tools
-   Python 3 and pip
-   PostgreSQL access (local or cloud, e.g. Supabase)

## 12. Installation and Startup

Clone the repository:

``` bash
git clone https://github.com/IT24100775/PropMate.git
cd PropMate
```

### Backend

``` bash
cd backend/PropMate.Api
dotnet restore
dotnet build
dotnet run
```

### React Web

``` bash
cd frontend/propmate-web
npm install
npm run dev
```

Production build:

``` bash
npm run build
```

### Flutter Mobile

``` bash
cd mobile/propmate_mobile
flutter pub get
flutter run --dart-define=API_BASE_URL=<API_BASE_URL>
```

Using the deployed API:

``` bash
flutter run --dart-define=API_BASE_URL=https://propmate-api-m5rt.onrender.com/api
```

Release APK:

``` bash
flutter build apk --release --dart-define=API_BASE_URL=https://propmate-api-m5rt.onrender.com/api
```

Output:

``` text
mobile/propmate_mobile/build/app/outputs/flutter-apk/app-release.apk
```

### Property Verification Agent

``` bash
cd ai/property-verification-agent
pip install -r requirements.txt
uvicorn app.main:app --reload --port 8001
```

### Property Discovery Agent

``` bash
cd ai/property-discovery-agent
pip install -r requirements.txt
uvicorn main:app --reload
```

### Transaction Negotiation Agent

``` bash
cd ai/transaction-negotiation-agent
pip install -r requirements.txt
uvicorn app.main:app --reload
```

### Maintenance Management Agent

``` bash
cd ai/maintenance-management-agent
pip install -r requirements.txt
uvicorn main:app --reload
```

> When running all services locally, use non-conflicting ports and
> configure the corresponding service URLs accordingly.

## 13. Environment Variables

**Never commit real secrets to Git.**

### ASP.NET Core Backend

``` text
ConnectionStrings__DefaultConnection=<POSTGRESQL_CONNECTION_STRING>
Jwt__Key=<JWT_SIGNING_KEY>
PropertyVerificationService__BaseUrl=<PROPERTY_VERIFICATION_AGENT_URL>
PropertyDiscoveryService__BaseUrl=<PROPERTY_DISCOVERY_AGENT_URL>
AgenticAiService__BaseUrl=<TRANSACTION_NEGOTIATION_AGENT_URL>
AgenticAiService__InternalApiKey=<INTERNAL_SERVICE_KEY>
GEMINI_API_KEY=<GEMINI_API_KEY>
```

### React Web

``` text
VITE_API_URL=<BACKEND_API_BASE_URL>
VITE_MAINTENANCE_AI_URL=<MAINTENANCE_AGENT_URL>
```

### Property Verification Agent

``` text
GOOGLE_API_KEY=<GOOGLE_GEMINI_API_KEY>
```

### Property Discovery Agent

``` text
BACKEND_API_URL=<BACKEND_API_BASE_URL>
GEMINI_API_KEY=<GOOGLE_GEMINI_API_KEY>
```

### Transaction Negotiation Agent

``` text
GOOGLE_API_KEY=<GOOGLE_GEMINI_API_KEY>
GEMINI_MODEL=<SUPPORTED_GEMINI_MODEL>
INTERNAL_API_KEY=<INTERNAL_SERVICE_KEY>
BACKEND_INTERNAL_URL=<BACKEND_ROOT_URL>
DATABASE_URL=<POSTGRESQL_CONNECTION_STRING>
```

### Maintenance Management Agent

``` text
BACKEND_API_URL=<BACKEND_API_BASE_URL>
FRONTEND_ORIGIN=<FRONTEND_ORIGIN>
GEMINI_API_KEY=<GOOGLE_GEMINI_API_KEY>
```

Use .NET User Secrets and/or platform environment-variable settings for
sensitive values.

## 14. Database Setup

1.  Create or obtain access to PostgreSQL.
2.  Configure `ConnectionStrings__DefaultConnection`.
3.  From `backend/PropMate.Api`, restore/build the project.
4.  Apply EF Core migrations.
5.  Start the backend and verify database connectivity.

``` bash
dotnet ef database update
```

If required:

``` bash
dotnet tool install --global dotnet-ef
```

## 15. Recommended Local Startup Order

1.  PostgreSQL
2.  Property Verification Agent
3.  Property Discovery Agent
4.  Transaction Negotiation Agent
5.  Maintenance Management Agent
6.  ASP.NET Core API
7.  React Web
8.  Flutter Mobile

## 16. API Documentation

Swagger/OpenAPI is used for interactive backend API documentation.

``` bash
cd backend/PropMate.Api
dotnet run
```

Open the Swagger URL configured/shown for the local API, commonly:

``` text
http://localhost:<PORT>/swagger
```

Important API groups include:

-   Authentication
-   Property Listings
-   Favourites
-   Viewing Management
-   Applications / Offers / Transactions
-   AI Workflows
-   Maintenance
-   Technicians

Swagger should be used as the authoritative interactive reference rather
than duplicating every endpoint in this README.

## 17. Testing Instructions

### Component 1

Owner creates listing -\> submits -\> AI verification -\> Admin review
-\> approve -\> publish -\> verify Buyer/Renter visibility.

### Component 2

Buyer/Renter discovers property -\> AI search -\> details/images/map -\>
Owner creates viewing -\> Buyer books -\> Owner verifies booking.

### Component 3

Buyer/Renter submits application/offer -\> Owner reviews -\> AI-assisted
negotiation -\> human approval -\> Buyer accepts -\> agreement -\> both
confirmations -\> transaction completion.

### Component 4

Create maintenance request -\> AI analysis -\> technician recommendation
-\> human approval -\> assignment/scheduling -\> `IN_PROGRESS` -\>
expense -\> `RESOLVED` -\> verify History.

### Integrated Workflow

``` text
Property Listing
    ↓
AI Verification
    ↓
Admin Approval & Publication
    ↓
Property Discovery
    ↓
Viewing Booking
    ↓
Application / Offer
    ↓
AI-Assisted Negotiation
    ↓
Agreement & Transaction Completion
    ↓
Maintenance Management
```

Both rental and purchase transaction paths should be tested where
required.

## 18. Deployment

### Vercel

-   Application: React web
-   Root directory: `frontend/propmate-web`
-   Production branch: `main`
-   Configure frontend environment variables in Vercel.

### Render

-   ASP.NET Core API uses `backend/PropMate.Api/Dockerfile`.
-   Each AI service is deployed independently from its directory under
    `ai/`.
-   Configure secrets through Render environment variables.
-   Production services use the `main` branch.

Automatic deployment/build settings may be disabled for a stable
demonstration environment. Deploy manually when a new version is
intentionally ready for production.

## 19. Live URLs

| Service | URL |
|---|---|
| React Web | https://prop-mate-two.vercel.app |
| ASP.NET Core API | https://propmate-api-m5rt.onrender.com |
| API Base | https://propmate-api-m5rt.onrender.com/api |
| Property Verification Agent | https://propmate-property-verification-agent.onrender.com |
| Property Discovery Agent | https://propmate-property-discovery-agent.onrender.com |
| Transaction Negotiation Agent | https://propmate-transaction-negotiation-agent.onrender.com |
| Maintenance Management Agent | https://propmate-maintenance-management-agent.onrender.com |
| Mobile Application | Android APK - available from the project release/submission package. |

> Render free-tier services may require a short warm-up period after
> inactivity.

## 20. Test Accounts

| Role | Email | Password |
|---|---|---|
| Buyer/Renter | `buyer@propmate.com` | `Buyer123!` |
| Owner/Agent | `owner@propmate.com` | `Owner123!` |
| Admin | `admin@propmate.com` | `Admin123!` |
| Property Manager | `propertymanager@propmate.com` | `PropertyManager123!` |

These credentials are dedicated demonstration accounts and must not be
reused as real user credentials.

## 21. Individual Contributions

| Member | IT Number | Contribution |
|---|---|---|
| **Riwaz F. N. M.** | IT24100775 | Property Listing & Approval + Property Listing Verification Agent |
| **Gunasekara R. P. I. M** | IT24100795 | Property Discovery & Viewing + Property Discovery Agent |
| **Rosayro De M. C. J** | IT24101618 | Applications, Offers & Transactions + Transaction Negotiation Agent |
| **Ilma M. S. F** | IT24103987 | Maintenance Management + Maintenance Management Agent |

## 22. Key Challenges and Solutions

### Multi-component integration

Four independently developed components had to operate as one system. A
shared ASP.NET Core API and PostgreSQL database, with REST-based
communication and clear component boundaries, were used.

### AI-service communication

Independent services required correct authentication, payload formats,
workflow identifiers, serialization, and timeout handling. Service
clients and DTOs were aligned and tested end-to-end.

### Cloud deployment and warm-up

Free-tier services can sleep after inactivity. Health endpoints,
appropriate timeouts, and deployment-aware testing were used.

### Cross-platform integration

React supports Owner/Agent, Admin, and Property Manager workflows, while
Flutter provides the Buyer/Renter experience through the same backend
and JWT authentication.

### AI reliability and human control

AI output can vary. Deterministic validation/fallback logic and human
approval are used for important workflows and actions.

## 23. Security Considerations

-   JWT-based authentication.
-   Role-based authorization.
-   Secrets stored in environment variables/local secret stores.
-   Never commit database passwords, Gemini/API keys, JWT keys, internal
    service keys, or bearer tokens.
-   Server-side validation and controlled status transitions.
-   Human approval before selected AI-generated actions are executed.
-   CORS configured for required deployed frontend origins.
-   Published test credentials are dedicated demonstration accounts
    only.

## 24. AI Usage Declaration

### AI within PropMate

PropMate integrates Agentic AI into Property Listing Verification,
Property Discovery, Transaction Negotiation, and Maintenance Management.
Google Gemini is used where configured for reasoning, interpretation,
classification, or recommendation tasks. AI workflows are combined with
application data, deterministic tools/validation, and human approval
where appropriate.

### AI-assisted development

The team used: - **ChatGPT** - **GitHub Copilot** - **Antigravity**

These tools assisted with brainstorming, code development, debugging,
troubleshooting, documentation, and refinement. AI-generated suggestions
were reviewed, adapted, integrated, and tested by the project team. The
team remains responsible for the final implementation and submitted
work.

## 25. Repository

https://github.com/IT24100775/PropMate

## 26. Academic Use

This repository was developed as an academic software engineering
project. Unless a separate license states otherwise, it should be
treated as coursework and not assumed to grant a general open-source
license.

# PropMate Component 3 Backend

This folder follows the current repository structure:

```text
PropMate/
  ai/                    # AI services live here in the full repository
  backend/
    PropMate.Api/        # this package
  frontend/              # ignored for this backend package
  mobile/                # ignored for this backend package
```

## Important AI structure change

The Gemini Agentic AI implementation is **not inside `backend/`**. The ASP.NET Core backend only contains an HTTP integration layer (`AgentWorkflowClient`) and the API gateway controller (`AiWorkflowsController`).

The separate AI service is expected to live under the repository-level `ai/` folder, alongside the existing `property-verification-agent`. That service will contain the four assessed agents:

- `PlannerAgent`
- `DomainAnalysisAgent`
- `ActionAgent`
- `ValidationAgent`

It should persist its workflow/audit state and expose the workflow API described below. Gemini credentials should stay in that AI service, not in ASP.NET Core.

## Component 3 domain backend included

- Rental applications: employment, income, occupants, move-in date, duration, message
- Purchase offers: offer amount and conditions
- Separate rental and purchase negotiation flows
- Immutable counter-offer history
- Separate negotiation messages
- Either party can make a counter-offer
- Accepting an application/offer or counter-offer closes negotiation and generates an agreement
- Buyer/tenant and seller/owner may confirm in either order
- Listing becomes `Sold` or `Rented` after both confirmations
- OwnerAgent access is restricted to their own listings
- BuyerRenter access is restricted to their own applications/offers
- Admin access is view-only for Component 3

## External Agentic AI service contract expected by the backend

The backend calls the external AI service at `AgenticAiService:BaseUrl`.

Expected routes:

```text
POST /workflows
GET  /workflows
GET  /workflows/{workflowId}
POST /workflows/{workflowId}/approvals/{approvalId}/decision
```

The backend forwards authenticated identity through trusted service-to-service headers:

```text
X-User-Id
X-User-Role
X-Internal-Api-Key   # optional/configured secret
```

The AI service should accept/return the DTO shape in `DTOs/AgenticAI/AgentWorkflowDtos.cs` and enforce its own allow-listed tools, validation, approval gate, retries/timeouts, and audit persistence.

## Local service ports used in development configuration

```text
Property verification agent: http://127.0.0.1:8001
Component 3 agent service:    http://127.0.0.1:8002
React web app:                http://localhost:5173
```

## Database

`Database/COMPONENT3_SCHEMA.sql` contains only the Component 3 **transaction** tables. AI workflow persistence belongs to the separate AI service because its implementation is outside `backend/`.

The existing EF migration files are left unchanged. Generate a new migration locally after reviewing the model:

```bash
dotnet ef migrations add AddComponent3Transactions
dotnet ef database update
```

## Secrets

Do not commit JWT keys, PostgreSQL credentials, Gemini API keys, or the internal AI service key. Gemini credentials belong to the separate AI service.

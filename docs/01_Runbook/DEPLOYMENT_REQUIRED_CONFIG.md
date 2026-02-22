# Deployment Required Configuration

This document describes the configuration required to deploy TikQ in production (intranet) and, when used, to connect it to the organization’s Company/Directory database. It is intended for system administrators and infrastructure teams.

**SonarQube scanning** is optional and not required for build or deploy. The repository may include or omit SonarQube/SonarScanner artifacts; they do not affect runtime behavior.

---

## Dual User Stores

TikQ supports two categories of users:

1. **Local users** — Stored in the TikQ database (email, password hash, role, profile). Used for offline/testing and as the system-of-record for roles and authorization.
2. **Server users** — Sourced from **CompanyDirectory** (Boss DB) as a **read-only** user directory. The Company DB provides profile fields only: Email, FullName, IsActive/IsDisabled. **No passwords are stored or read from the Company DB.** Authentication for these users is done with passwords stored in the TikQ DB.

**Shadow user creation:** When a user logs in and is not found in the TikQ DB, and CompanyDirectory is enabled, TikQ looks up the user by email in the Company Directory. If found and active, it creates or updates a **shadow user** in the TikQ DB with default role (e.g. Client) unless a role mapping already exists. TikQ **never writes** to the Company DB. Server users must have their password set in TikQ (e.g. by an admin via **pre-provision**). Admins can set a password for any user by email using the Admin-only endpoint `POST /api/admin/roles/set-password` (body: `{ "email": "...", "newPassword": "..." }`).

---

## Break-Glass Emergency Admin

When the main server or directory is unavailable, an **emergency admin** (break-glass) login can be enabled. It mirrors server Admin privileges and uses a **separate login route** and an **emergency key** so use is explicit and auditable.

- **Frontend:** Dedicated route `/login-emergency`. Form requires **Email**, **Password**, and **Emergency key**.
- **Backend:** Endpoint `POST /api/auth/emergency-login` (body: `{ "email", "password", "emergencyKey" }`). Only active when `EmergencyAdmin:Enabled=true`. Validates the emergency key and credentials, ensures an Admin user exists in the TikQ DB with the same profile, and signs the user in. In Production, **no default passwords** are allowed; if Emergency Admin is enabled but secrets are missing, startup fails.

**Required environment variables for Emergency Admin** (when `EmergencyAdmin:Enabled=true` in Production or ProductionHandoffMode):

| Variable | Description |
|----------|-------------|
| `EmergencyAdmin__Enabled` | Set to `true` to enable emergency login. |
| `EmergencyAdmin__Email` | Email that must be used for emergency login. |
| `EmergencyAdmin__FullName` | Display name for the emergency admin user (e.g. "Break-Glass Admin"). |
| `EmergencyAdmin__Password` | Password for emergency login. **Must be at least 8 characters.** Set via environment in Production (e.g. `EmergencyAdmin__Password`). |
| `EmergencyAdmin__Key` | Extra secret key required in the login form. **Set via environment only in Production** (e.g. `EmergencyAdmin__Key`). |

---

## Required Environment Variables

**JWT secret (mandatory in production)**  
- **Key**: `Jwt:Secret` (appsettings or env `Jwt__Secret`), or `JWT_SECRET` (environment variable).  
- **Required**: In Production (or when `ProductionHandoffMode=true`), startup fails if no JWT secret is set.  
- **Recommendation**: Use a strong secret (e.g. 32+ characters). Set via environment variable so it is not committed (e.g. `JWT_SECRET` or `Jwt__Secret` in IIS or host configuration).

**Company Directory connection (when enabled)**  
- **When**: Only when `CompanyDirectory:Enabled=true`.  
- **Required**:  
  - `CompanyDirectory:ConnectionString` must be non-empty (SQL Server connection to the read-only Company/Directory DB).  
  - `CompanyDirectory:Mode` must be one of: `Enforce`, `Optional`, `Friendly`.  
- **Important**: The Company DB is used **read-only** (identity lookup only). No schema changes or writes are performed by TikQ against this database.

**Production database**  
- **Default**: In Production, using **SQLite** as the main app database causes startup to **fail** unless explicitly allowed.  
- **To use SQL Server**: Set `ConnectionStrings:DefaultConnection` to your SQL Server connection string.  
- **To allow SQLite in Production** (e.g. for testing only): Set `AllowSqliteInProduction=true` in config.

**CORS (mandatory in production)**  
- **Production requires** `Cors:AllowedOrigins=["<frontend-origin>"]` (e.g. your frontend URL). If empty or missing in Production or when `ProductionHandoffMode=true`, startup fails with: "Cors:AllowedOrigins must be configured in production."

**Production flags**  
- **ASPNETCORE_ENVIRONMENT**: Set to `Production` on the production host so that production validation and SQLite rejection run.  
- **ProductionHandoffMode** (optional): When set to `true`, the app applies production-style validation and disables debug/maintenance endpoints even if environment is not Production.

**Bootstrap admin (when DB is empty)**  
- **BootstrapAdmin__Password** must be provided securely in production when the database has no users (e.g. via environment variable or `appsettings.Production.json`). Do not commit this value to source control.

---

## Database Responsibilities

**TikQ database (primary)**  
- All application data: users, roles, tickets, categories, custom fields, assignments, settings.  
- This is the only database on which TikQ runs migrations and performs writes.  
- In production, use SQL Server (or another supported provider) via `ConnectionStrings:DefaultConnection`. SQLite is blocked unless `AllowSqliteInProduction=true`.

**Company DB (read-only user directory)**  
- Used only when `CompanyDirectory:Enabled=true`.  
- **Read-only**: Identity lookup only (e.g. email, display name, IsActive/IsDisabled). **No passwords** are read from or stored in the Company DB; server users authenticate with passwords stored in TikQ.  
- **No migrations**: TikQ does not run Entity Framework migrations or any schema changes against the Company DB.  
- **No writes**: No INSERT, UPDATE, DELETE, or DDL against the Company DB. Application-level guards enforce this; the organization should also grant the connection user read-only permissions at the database level.

Roles and landing paths are stored and managed **only in the TikQ database**. The Company DB does not supply or override roles. Shadow users created from the directory get a default role (e.g. Client) in TikQ until an admin assigns a different role or pre-provisions a password.

---

## Security Requirements

**HTTPS**  
Use HTTPS in production for the backend and frontend. Configure certificates and bindings on the host (IIS or Kestrel reverse proxy).

**Cookie flags**  
When using cookie-based auth, ensure appropriate flags (e.g. Secure, SameSite) for your environment. Cookie domain and path must match how the frontend is served (e.g. subpath or different subdomain may require configuration).

**Secret storage**  
Do not commit JWT secrets or connection strings to source control. Use environment variables, a secure vault, or host-specific configuration (e.g. IIS environment variables, Azure Key Vault).

---

## Deployment Modes

**Intranet deployment**  
TikQ is designed to run on the organization’s internal network. Backend and frontend are hosted on internal servers; the frontend’s API base URL (e.g. `NEXT_PUBLIC_API_BASE_URL`) must point to the backend’s intranet URL.

**Hybrid auth (Company Directory + TikQ)**  
When Company Directory is enabled, users not found in TikQ are looked up in the directory by email. If found and active, a shadow user is created or updated in TikQ with a default role (e.g. Client). **Authentication is always with the password stored in TikQ** (local users or admin pre-provisioned passwords for server users). Roles are assigned only in TikQ.

**Email/password fallback**  
TikQ supports email/password login against the TikQ database. This can be used as the sole auth method or as fallback when Company Directory is optional or unavailable.

---

## Failure Scenarios

| Scenario | Cause | Action |
|----------|--------|--------|
| **Missing role** | User exists in Company DB but has no user or role in TikQ (e.g. in Enforce mode). | Provision the user in TikQ and assign a role (Admin, Technician, or Client). |
| **Missing config** | JWT secret not set in production; or Company Directory enabled but connection string or Mode missing; or CORS origins empty in production. | Set `JWT_SECRET` (or `Jwt:Secret`); if using Company Directory, set `CompanyDirectory:ConnectionString` and `CompanyDirectory:Mode`, or set `CompanyDirectory:Enabled=false`; set `Cors:AllowedOrigins` to your frontend origin(s). |
| **Company DB unavailable** | Company Directory is enabled but the directory database is down or unreachable. | Restore directory availability, or temporarily disable Company Directory; ensure timeouts and monitoring are in place. |
| **SQLite in production** | Main app database is SQLite and `AllowSqliteInProduction` is not set. | Set `ConnectionStrings:DefaultConnection` to SQL Server (or intended DB), or set `AllowSqliteInProduction=true` only if acceptable for the environment. |
| **Bootstrap admin password** | No users in DB and bootstrap would run, but `BootstrapAdmin:Password` is missing or shorter than 8 characters. | Set `BootstrapAdmin:Email`, `BootstrapAdmin:Password` (min 8 chars), and `BootstrapAdmin:FullName` when using bootstrap for first user. |

**Common startup messages**  
- *"JWT secret is not configured for production"* — Set `Jwt:Secret` or `JWT_SECRET`.  
- *"CompanyDirectory:ConnectionString is empty"* — Set the connection string or set `CompanyDirectory:Enabled=false`.  
- *"CompanyDirectory:Mode must be one of: Enforce, Optional, Friendly"* — Set `CompanyDirectory:Mode` accordingly.  
- *"SQLite is not allowed as the main app database in Production"* — Use SQL Server (or set `AllowSqliteInProduction=true` if acceptable).  
- *"BootstrapAdmin:Password is missing or too short"* — Set bootstrap admin config when the database has no users.  
- *"Cors:AllowedOrigins must be configured in production"* — Set `Cors:AllowedOrigins` in appsettings (e.g. `["https://your-frontend"]`).

---

## First Run Behavior

**Bootstrap rules**  
If the Users table is empty, the application can create a first admin user from configuration (`BootstrapAdmin:Email`, `BootstrapAdmin:Password`, `BootstrapAdmin:FullName`). In Production or when `ProductionHandoffMode=true`, the bootstrap password must be set and at least 8 characters; no default password (e.g. `Admin123!`) is used. If missing or too short, startup fails when no users exist.

**Seeding disabled in production**  
Demo seed data (e.g. test users with known passwords like `Test123!`) runs only in **Development** or when `EnableDevSeeding=true`. In production, `EnableDevSeeding` is false by default; do not set it to true unless you understand the security impact.

**Health endpoint**  
- **URL**: `/api/health` (and `/health` for compatibility).  
- **Auth**: Unauthenticated so load balancers and monitors can check app health without credentials.

---

## Checklist Before Go-Live

- [ ] JWT secret set and not a default or development value.  
- [ ] Production database connection string points to SQL Server (or intended DB); SQLite not used unless explicitly allowed.  
- [ ] If Company Directory is enabled: connection string and Mode are set and valid; directory user has read-only access.  
- [ ] Bootstrap admin password (if used) is strong and from config/env, not a default.  
- [ ] If Emergency Admin is enabled: `EmergencyAdmin:Email`, `EmergencyAdmin:Password` (min 8 chars), and `EmergencyAdmin:Key` are set (via env in Production).  
- [ ] Dev seeding and debug endpoints are disabled (default when not in Development and not `EnableDevSeeding`).  
- [ ] Startup log shows `[HANDOFF] Production validation passed` when running in Production or with `ProductionHandoffMode=true`.  
- [ ] `Cors:AllowedOrigins` is set to your frontend origin(s) (required in production).

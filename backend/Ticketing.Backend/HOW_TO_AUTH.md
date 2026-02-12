# TikQ Hybrid Authentication (Intranet + External)

## Summary
- Intranet/LAN users: Windows Integrated Authentication (Kerberos/Negotiate), no login button required.
- External/mobile users: Company SSO via ADFS OIDC (`/api/auth/adfs/login`) or ADFS JWT bearer.
- TikQ authorization remains internal (`Admin`, `Supervisor`, `Technician`, `Client`) and is mapped after authentication.

## Flows

### 1) Intranet flow (automatic)
1. Frontend calls `GET /api/auth/me`.
2. API challenges with `401 WWW-Authenticate: Negotiate` when needed.
3. Browser completes Windows Integrated login automatically (domain-joined + browser policy).
4. API resolves identity to Boss DB by email/UPN.
5. If found and enabled: local TikQ user is synced and TikQ role claims are emitted.
6. If disabled or missing from Boss DB: API returns `403` with clear reason.

### 2) External flow (Company SSO)
1. Frontend login page calls `GET /api/auth/adfs/login?returnUrl=/`.
2. API starts OIDC challenge (when `Authentication:Oidc:Enabled=true`).
3. User signs in at ADFS, returns to app with authenticated external cookie session.
4. API still enforces Boss DB enabled/disabled checks and TikQ role mapping.

### 3) Bearer token flow (ADFS JWT)
1. Client sends `Authorization: Bearer <token>`.
2. Policy scheme selects JWT bearer validation.
3. Identity claims are normalized (email/upn/preferred_username/unique_name order).
4. Boss DB + TikQ role sync is applied.

## Scheme Selection Rules
- If request has `Authorization: Bearer ...` -> `AdfsBearer`.
- Else if external auth cookie exists -> external cookie scheme.
- Else if Development + `DevHeaderAuth.Enabled=true` + `X-Dev-User` header -> `DevHeader`.
- Else -> `Negotiate`.

## Required Configuration
`appsettings.json`:

- `Authentication.Mode` (`Hybrid` default)
- `Authentication.Windows.Enabled`
- `Authentication.Jwt.Authority`
- `Authentication.Jwt.MetadataAddress`
- `Authentication.Jwt.Audience`
- `Authentication.DevHeaderAuth.Enabled` (`false` by default, keep off in production)
- `Authentication.Oidc.*` (for backend-initiated external login)
- `Authentication.RoleMapping.*`
- `BossDb.ConnectionString` and table/column mappings

Frontend:
- `NEXT_PUBLIC_AUTH_MODE=auto|lan|external`
  - `lan`: only automatic intranet login.
  - `external`: only Company SSO button.
  - `auto`: try intranet first, then show Company SSO fallback.
- `NEXT_PUBLIC_API_BASE_URL` should point to the API origin (prefer same-origin reverse proxy).

## Boss DB Provisioning Rules
- User lookup by email/UPN.
- If not found: deny with `403` (`No access to TikQ`).
- If disabled: deny with `403`.
- If valid:
  - sync/create local TikQ user profile,
  - map role via configurable role-mapping lists,
  - emit TikQ role claim used by `[Authorize]`.

## Dev Mode (safe local testing)
- `Authentication.DevHeaderAuth.Enabled=true` only in `Development`.
- Send:
  - `X-Dev-User: someone@company.local`
  - `X-Dev-Roles: Admin` (optional)
- Disabled automatically outside development.

## Deployment Recommendation
- Prefer same-origin reverse proxy (Next.js + API under one host) for smooth Negotiate.
- Use HTTPS in production.
- For Kerberos:
  - set SPN on service account / app pool identity,
  - verify browser integrated-auth allowlist (intranet zone / trusted URIs).

## Troubleshooting
- `401` usually means authentication challenge (Negotiate/Bearer) not completed.
- `403` means authenticated identity is blocked by authorization or Boss directory checks.
- If external login returns `503` on `/api/auth/adfs/login`, OIDC is disabled or incomplete.
- Cross-origin + Windows auth is unreliable by design; same-origin is strongly recommended.

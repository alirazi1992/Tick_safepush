# Sanity Check Report - IT Ticketing System

**Generated:** 2025-01-02  
**Backend Root:** `backend/Ticketing.Backend`  
**Frontend Root:** `frontend`

---

## PHASE 0 — REPO STRUCTURE

✅ **Confirmed:**
- Backend: `backend/Ticketing.Backend` (ASP.NET Core 8.0)
- Frontend: `frontend` (Next.js with TypeScript)
- Database: SQLite (via EF Core)
- Migrations: `backend/Ticketing.Backend/Infrastructure/Data/Migrations`

---

## PHASE 1 — INVENTORY (Read-Only Audit)

### A) FRONTEND INVENTORY

#### 1. Route Map (Next.js App Router)

| Route | File | Purpose |
|-------|------|---------|
| `/` | `app/page.tsx` | Home/Dashboard (role-based) |
| `/login` | `app/login/page.tsx` | Login page |
| `/tickets/[id]` | `app/tickets/[id]/page.tsx` | Ticket detail page |
| `/settings/notifications` | `app/settings/notifications/page.tsx` | Notification settings |
| `/examples/ticket-calendar` | `app/examples/ticket-calendar/page.tsx` | Example/Test route |

**Special Routes:**
- `app/layout.tsx` - Root layout
- `app/error.tsx` - Error boundary
- `app/loading.tsx` - Loading state

#### 2. Navigation Map

**From `app/page.tsx` (Dashboard Shell):**
- Role-based navigation via `DashboardShell` component
- Admin: Tickets, Assignment, Technicians, Categories, Auto-settings (tabs)
- Technician: Tickets list
- Client: Tickets list, Create ticket

**Links found:**
- `router.push("/")` - Back to dashboard (from ticket detail, settings)
- `router.push("/tickets/${id}")` - Navigate to ticket detail
- `router.push(notification.linkUrl)` - Notification links

**Orphan routes:**
- `/examples/ticket-calendar` - Not linked in navigation (test/example route - OK)

#### 3. API Call Map

**Base URL:** `process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5000"`

**Tickets API (`lib/tickets-api.ts`):**
- `POST /api/tickets/{ticketId}/assign-technicians`
- `DELETE /api/tickets/{ticketId}/technicians/{technicianId}`
- `GET /api/tickets/{ticketId}/technicians`
- `PUT /api/tickets/{ticketId}/technicians/me/state`
- `GET /api/tickets/{ticketId}/activities`
- `PUT /api/tickets/{ticketId}/work/me` ⚠️ **NEW - Collaboration feature**
- `GET /api/tickets/{ticketId}/collaboration` ⚠️ **NEW - Collaboration feature**

**Notifications API (`lib/notifications-api.ts`):**
- `GET /api/notifications?onlyUnread=...&type=...&ticketId=...&q=...&page=...&pageSize=...`
- `GET /api/notifications/unread-count`
- `PATCH /api/notifications/{id}/read`
- `PUT /api/notifications/read-all`
- `DELETE /api/notifications/{id}`
- `DELETE /api/notifications/clear-read`
- `GET /api/notifications/preferences`
- `PUT /api/notifications/preferences`

**Categories API (`lib/categories-api.ts`):**
- `GET /api/categories` (public)
- `GET /api/categories/admin?search=...&page=...&pageSize=...`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`
- `GET /api/categories/{categoryId}/subcategories`
- `POST /api/categories/{categoryId}/subcategories`
- `PUT /api/categories/subcategories/{id}`
- `DELETE /api/categories/subcategories/{id}`

**Auth API (`lib/auth-context.tsx`):**
- `POST /api/auth/login`
- `GET /api/auth/me`
- `POST /api/auth/register`
- `PUT /api/auth/me`
- `POST /api/auth/change-password`

**Technicians API (`lib/technicians-api.ts`):**
- `GET /api/admin/technicians`
- `GET /api/admin/technicians/{id}`
- `POST /api/admin/technicians`
- `POST /api/admin/technicians/create-with-user`
- `PUT /api/admin/technicians/{id}`
- `PATCH /api/admin/technicians/{id}/status`
- `PATCH /api/admin/technicians/{id}/link-user`
- `PATCH /api/admin/technicians/{id}/deactivate`
- `DELETE /api/admin/technicians/{id}`

**Field Definitions API (`lib/field-definitions-api.ts`):**
- `GET /api/categories/subcategories/{subcategoryId}/fields`
- `GET /api/categories/subcategories/{subcategoryId}/fields/admin`
- `POST /api/categories/subcategories/{subcategoryId}/fields`
- `PUT /api/categories/subcategory-fields/{id}`
- `DELETE /api/categories/subcategory-fields/{id}`

**Other APIs:**
- Settings, Smart Assignment, Preferences, etc. (not fully enumerated here)

#### 4. Common UI Break Issues

**Potential Issues:**
- SignalR connection handling in `ticket-collaboration-box.tsx` - needs error handling
- Optional chaining in some API responses
- Type safety in some mappers

### B) BACKEND INVENTORY

#### 1. Endpoint Map

**TicketsController (`api/[controller]` = `api/tickets`):**
- `GET /api/tickets` - List tickets (filtered by role)
- `GET /api/tickets/{id}` - Get ticket detail
- `POST /api/tickets` - Create ticket (Client only)
- `PATCH /api/tickets/{id}` - Update ticket
- `PUT /api/tickets/{id}/assign-technician` - Assign technician (Admin)
- `POST /api/tickets/{id}/assign` - [OBSOLETE] Assign ticket
- `GET /api/tickets/{id}/messages` - Get messages
- `POST /api/tickets/{id}/messages` - Add message
- `GET /api/tickets/calendar?start=...&end=...` - Calendar view (Admin)
- `POST /api/tickets/{ticketId}/assign-technicians` - Multi-assign (Admin)
- `DELETE /api/tickets/{ticketId}/technicians/{technicianId}` - Remove technician (Admin)
- `GET /api/tickets/{ticketId}/technicians` - Get assigned technicians
- `PUT /api/tickets/{ticketId}/technicians/me/state` - Update technician state (Technician)
- `GET /api/tickets/{ticketId}/activities` - Get activities
- `PUT /api/tickets/{ticketId}/work/me` - Update work session (Technician) ⚠️ **NEW**
- `GET /api/tickets/{ticketId}/collaboration` - Get collaboration data ⚠️ **NEW**

**NotificationsController (`api/[controller]` = `api/notifications`):**
- `GET /api/notifications` - List notifications (paginated)
- `GET /api/notifications/unread-count` - Unread count
- `PATCH /api/notifications/{id}/read` - Mark as read
- `PUT /api/notifications/read-all` - Mark all as read
- `DELETE /api/notifications/{id}` - Delete notification
- `DELETE /api/notifications/clear-read` - Clear read notifications
- `GET /api/notifications/preferences` - Get preferences
- `PUT /api/notifications/preferences` - Update preferences

**CategoriesController (`api/[controller]` = `api/categories`):**
- `GET /api/categories` - Get all categories (public)
- `GET /api/categories/admin` - Admin list (paginated)
- `POST /api/categories` - Create category (Admin)
- `PUT /api/categories/{id}` - Update category (Admin)
- `DELETE /api/categories/{id}` - Delete category (Admin)
- `GET /api/categories/{categoryId}/subcategories` - Get subcategories
- `POST /api/categories/{categoryId}/subcategories` - Create subcategory (Admin)
- `PUT /api/categories/subcategories/{id}` - Update subcategory (Admin)
- `DELETE /api/categories/subcategories/{id}` - Delete subcategory (Admin)

**TechniciansController (`api/admin/technicians`):**
- `GET /api/admin/technicians` - List all (Admin)
- `GET /api/admin/technicians/{id}` - Get by ID (Admin)
- `POST /api/admin/technicians` - Create (Admin)
- `POST /api/admin/technicians/create-with-user` - Create with user (Admin)
- `PUT /api/admin/technicians/{id}` - Update (Admin)
- `PATCH /api/admin/technicians/{id}/status` - Update status (Admin)
- `PATCH /api/admin/technicians/{id}/link-user` - Link user (Admin)
- `PATCH /api/admin/technicians/{id}/deactivate` - Deactivate (Admin)
- `DELETE /api/admin/technicians/{id}` - Delete (Admin)

**FieldDefinitionsController (`api/categories`):**
- `GET /api/categories/subcategories/{subcategoryId}/fields` - Get fields (public)
- `GET /api/categories/subcategories/{subcategoryId}/fields/admin` - Admin view (Admin)
- `POST /api/categories/subcategories/{subcategoryId}/fields` - Create field (Admin)
- `PUT /api/categories/subcategory-fields/{id}` - Update field (Admin)
- `DELETE /api/categories/subcategory-fields/{id}` - Delete field (Admin)

**Other Controllers:**
- AuthController, SettingsController, SmartAssignmentController, UsersController, etc.

#### 2. DB/Migrations Check

**Entities in DbContext:**
- ✅ User, Ticket, TicketMessage
- ✅ TicketTechnician, TicketActivity
- ⚠️ **TicketWorkSession** - EXISTS in DbContext but NO MIGRATION FOUND

**Migrations Found:**
- `20251214121545_InitialCreate.cs`
- `20251220090428_AddNormalizedNameToCategories.cs`
- `20251220121133_AddSubcategoryFieldDefinitionsAndTicketFieldValues.cs`
- `20251222112101_AddSmartAssignmentRules.cs`
- `20251223104702_AddTechnicianSubcategoryPermissions.cs`
- `20251223130842_AddMultiTechnicianAssignment.cs`
- **MISSING: Migration for TicketWorkSession**

#### 3. Auth/JWT/CORS Check

✅ **JWT:** Configured in `Program.cs` with JwtSettings
✅ **CORS:** Configured for `localhost:3000` and `localhost:3001` with credentials
✅ **SignalR:** Hub registered at `/notificationHub`

⚠️ **SignalR Package Issue:** Build error suggests NuGet can't find `Microsoft.AspNetCore.SignalR` v8.0.4 (may be cache issue)

### C) CROSS-CHECK

#### 1. API Mismatches

**✅ MATCHES:**
- All tickets API endpoints match
- All notifications API endpoints match
- All categories API endpoints match
- All technicians API endpoints match
- Collaboration endpoints match (newly added)

#### 2. Navigation/Route Mismatches

**✅ ALL ROUTES REACHABLE:**
- `/` - Main entry point
- `/login` - Accessible from unauthenticated state
- `/tickets/[id]` - Linked from ticket lists and notifications
- `/settings/notifications` - Linked from navigation
- `/examples/ticket-calendar` - Test route (OK to be orphaned)

#### 3. Status/Enum Usage

✅ **Frontend uses enum strings** (e.g., "Open", "InProgress")
✅ **Backend uses JsonStringEnumConverter** - configured in Program.cs
✅ **Mappers handle conversion** between API and UI formats

---

## PHASE 1 — ISSUES FOUND

### 🔴 CRITICAL

1. **Missing Migration for TicketWorkSession**
   - Entity exists in code
   - DbSet registered in AppDbContext
   - Configuration class exists
   - **NO MIGRATION CREATED**
   - **Impact:** App will crash on startup if DB doesn't have table

### ⚠️ HIGH PRIORITY

2. **SignalR Package Build Error**
   - NuGet can't find `Microsoft.AspNetCore.SignalR` v8.0.4
   - May be NuGet cache issue or network issue
   - **Impact:** Backend won't build

3. **Potential Null Reference Issues**
   - Some SignalR connection handling may fail silently
   - Need defensive checks in collaboration box

### ⚠️ MEDIUM PRIORITY

4. **Error Handling in Collaboration Box**
   - SignalR connection failures need better UX
   - Polling fallback should be more robust

---

## PHASE 2 — FIXES APPLIED

### Fix 1: SignalR Package Reference
**Issue:** Build error - NuGet couldn't find Microsoft.AspNetCore.SignalR v8.0.4  
**Fix:** Removed explicit package reference (SignalR is included in Microsoft.AspNetCore.App framework in .NET 8.0)  
**Files Changed:**
- `backend/Ticketing.Backend/Ticketing.Backend.csproj`

### Fix 2: NotificationService Interface Mismatches
**Issue:** Multiple compilation errors due to interface signature changes  
**Fix:** Updated all NotificationService calls to match the interface:
- `NotifyTicketAssignedAsync` - Updated to use `(Guid ticketId, string ticketTitle, List<Guid> assignedUserIds)`
- `NotifyTicketCreatedAsync` - Updated to use `(Guid ticketId, string ticketTitle, Guid createdByUserId, int? categoryId, int? subcategoryId)`
- `NotifyTicketClosedAsync` - Updated to use `(Guid ticketId, Guid createdByUserId, string ticketTitle, TicketStatus status)`

**Files Changed:**
- `backend/Ticketing.Backend/Application/Services/TicketService.cs` (5 locations)
- `backend/Ticketing.Backend/Application/Services/SmartAssignmentService.cs` (1 location)

### Fix 3: Missing Migration for TicketWorkSession
**Issue:** TicketWorkSession entity exists in code but no migration was created  
**Fix:** Created migration `AddTicketWorkSession`  
**Files Changed:**
- New migration file created: `backend/Ticketing.Backend/Infrastructure/Data/Migrations/20251224053147_AddTicketWorkSession.cs`

### Fix 4: Missing SignalR Package in Frontend
**Issue:** Frontend build fails - `@microsoft/signalr` package not installed  
**Fix:** Installed `@microsoft/signalr` package  
**Files Changed:**
- `frontend/package.json` (updated dependencies)
- `frontend/package-lock.json` (updated)

### How Verified
- ✅ `dotnet build` - Build succeeded with only warnings (no errors)
- ✅ Migration created successfully (20251224053147_AddTicketWorkSession)
- 🔄 Frontend build verification in progress...

---

## PHASE 3 — FINAL VERIFICATION CHECKLIST

### Backend
- [x] `dotnet build` passes ✅ (with warnings only, no errors)
- [ ] Swagger loads (needs runtime verification)
- [ ] Auth works (login + /me) (needs runtime verification)
- [ ] Key endpoints respond (needs runtime verification)
- [x] Migration created successfully ✅ (`20251224053147_AddTicketWorkSession`)

### Frontend
- [x] `npm run build` passes ✅
- [ ] No console runtime crashes on main pages (needs runtime verification)
- [ ] Navigation links work (no 404) (needs runtime verification)
- [ ] Ticket flows work (needs runtime verification)
- [ ] Collaboration feature works (needs runtime verification)

### Build Warnings (Non-blocking)
**Backend:**
- CS8604: Possible null reference in TicketsController.cs:106 (request parameter)
- CS8602: Possible null references in TicketsController.cs (lines 130, 143)
- CS0168: Unused variable 'ex' in TicketsController.cs:405

These are nullable reference warnings and don't block execution. They can be addressed in a future cleanup.

---

## SUMMARY

### ✅ COMPLETED FIXES

1. **SignalR Package Issue** - Fixed (removed explicit package reference)
2. **NotificationService Interface Mismatches** - Fixed (6 locations updated)
3. **Missing TicketWorkSession Migration** - Fixed (migration created)
4. **Missing Frontend SignalR Package** - Fixed (package installed)

### ⚠️ REMAINING ITEMS

- Runtime verification needed (start app and test)
- Nullable reference warnings (non-critical, can be cleaned up later)
- Frontend dependency vulnerabilities (2 found, can run `npm audit fix` if needed)

### 📋 NEXT STEPS

1. Run migrations: `dotnet ef database update` (in backend folder)
2. Start backend: `dotnet run` (verify Swagger loads)
3. Start frontend: `npm run dev` (verify pages load)
4. Test key flows:
   - Login
   - Create ticket
   - View ticket detail
   - Collaboration box (for technicians)
   - Status updates
   - Notifications

---
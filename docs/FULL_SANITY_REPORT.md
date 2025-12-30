# TikQ Full Sanity Report

**Date:** 2025-12-30  
**Branch:** `fix/full-sanity-cleanarch`  
**Status:** In Progress

---

## Phase 0 — Safety + Baseline

### Git Status
- ✅ Branch created: `fix/full-sanity-cleanarch`
- ⚠️ Uncommitted changes present (will stash if needed)

### Inventory

#### Backend Projects + Entrypoints
- **Main Project:** `backend/Ticketing.Backend/Ticketing.Backend.csproj`
  - Entry Point: `Program.cs` (root level)
  - Type: Web SDK (`Microsoft.NET.Sdk.Web`)
  - Excludes: `src/**` folder (old structure)
  
- **Old Structure (Excluded):**
  - `src/Ticketing.Api/Ticketing.Api.csproj` - Excluded from build
  - `src/Ticketing.Application/` - Excluded
  - `src/Ticketing.Domain/` - Excluded
  - `src/Ticketing.Infrastructure/` - Excluded

**Conclusion:** Entry point is `Ticketing.Backend.csproj` with `Program.cs` in root.

#### EF Core Migrations + DB Location
- **Migrations Location:** `backend/Ticketing.Backend/Infrastructure/Data/Migrations/`
- **Migrations:**
  1. `20251214121545_InitialCreate.cs`
  2. `20251228103000_UpdateTicketStatusEnum.cs`
  3. `20251230000000_AddSubcategoryFieldDefinitions.cs`
  4. `20251230120000_AddMissingColumnsToSubcategoryFieldDefinitions.cs`

- **DB Location:** `backend/Ticketing.Backend/App_Data/ticketing.db`
  - Resolved at runtime via `Program.cs` using `ContentRoot` path
  - Absolute path: `{ContentRoot}/App_Data/ticketing.db`

#### Frontend Next.js
- **Version:** Next.js 15.2.4
- **Build Script:** `npm run build`
- **TypeScript:** Enabled
- **Package Manager:** npm

---

## Phase 1 — Reproduce and List ALL Failures

### Backend Diagnostics

#### 1.1 .NET Info
```powershell
dotnet --info
```
**Status:** ⏳ Pending

#### 1.2 Backend Clean
```powershell
cd backend/Ticketing.Backend
dotnet clean
```
**Status:** ⏳ Pending

#### 1.3 Backend Build
```powershell
dotnet build
```
**Status:** ⏳ Pending

#### 1.4 Backend Run
```powershell
dotnet run
```
**Status:** ⏳ Pending

#### 1.5 Endpoint Tests
- GET `/swagger` - ⏳ Pending
- GET `/api/categories` - ⏳ Pending
- GET `/api/admin/subcategories/{id}/fields` - ⏳ Pending
- POST `/api/admin/subcategories/{id}/fields` - ⏳ Pending

### Frontend Diagnostics

#### 2.1 Node/npm Versions
```powershell
node -v
npm -v
```
**Status:** ⏳ Pending

#### 2.2 Frontend Install
```powershell
cd frontend
npm ci
```
**Status:** ⏳ Pending

#### 2.3 Frontend Build
```powershell
npm run build
```
**Status:** ⏳ Pending

#### 2.4 Frontend TypeCheck
```powershell
npm run typecheck
```
**Status:** ⏳ Pending

#### 2.5 Known Issues to Check
- ⏳ TSX syntax error in `components/subcategory-field-designer-dialog.tsx`
- ⏳ TS7053 indexing error in `components/admin-ticket-management.tsx`

---

## Phase 2 — Fix Build Blockers

### 2A) Backend Compile/Run Blockers

#### CS5001 (No Main Entry Point)
**Status:** ⏳ Pending Investigation

#### Duplicate Assembly Attributes (CS0579)
**Status:** ⏳ Pending Investigation

#### File Locking (MSB4025)
**Status:** ⏳ Pending Investigation

### 2B) Frontend Compile Blockers

#### TSX Syntax Error
**File:** `components/subcategory-field-designer-dialog.tsx`
**Status:** ⏳ Pending Fix

#### TS7053 Indexing Error
**File:** `components/admin-ticket-management.tsx`
**Status:** ⏳ Pending Fix

---

## Phase 3 — Fix Subcategory Fields Feature

### Root Cause: DB Schema Mismatch
**Error:** `SQLite Error 1: 'no such column: s.DefaultValue'`

**Status:** ⏳ Pending Investigation

### Actions Required
1. ⏳ Inspect `SubcategoryFieldDefinition` model
2. ⏳ Compare with actual SQLite schema
3. ⏳ Create/fix migration
4. ⏳ Ensure migrations apply on startup
5. ⏳ Verify API endpoints work

---

## Phase 4 — Redesign Popup UI

**Status:** ⏳ Pending

---

## Phase 5 — Repo-wide Sanity Scan

**Status:** ⏳ Pending

---

## Phase 6 — Clean Architecture Verification

**Status:** ⏳ Pending

---

## Deliverables

- [ ] Working code: backend + frontend build and run
- [ ] `tools/sanity.ps1` script
- [ ] This report with all findings and fixes
- [ ] Final verification output

---

**Last Updated:** 2025-12-30 (Phase 0 Complete)



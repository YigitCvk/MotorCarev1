# GarajPass Frontend Migration Report
## Angular → Next.js 15 (React + TypeScript + Tailwind)

**Date:** 2026-05-25  
**Branch:** main  
**Status:** ✅ COMPLETE — all builds passing

---

## Executive Summary

The Angular 20 SPA has been replaced with a production-ready Next.js 15 frontend. The migration delivers:
- A full SaaS-quality panel covering all 8 feature domains
- A polished Turkish-language marketing landing page
- Public QR record sharing pages (no-auth)
- Docker integration with the existing staging stack
- Foundation for a future React Native mobile app

**Backend unchanged:** All 154 .NET tests passing, API contracts unmodified.

---

## Build Results

| Artifact | Result |
|---|---|
| `dotnet build -c Release` | ✅ 0 errors, 0 warnings |
| `dotnet test -c Release` | ✅ 154/154 passed (37 domain + 117 application) |
| `npm run build` (Next.js 15.5.18) | ✅ 33 routes compiled, 0 errors |

---

## Routes Built (33 total)

### Public / Marketing
| Route | Description |
|---|---|
| `/` | Redirect to `/home` |
| `/home` | Full SaaS landing page (hero, features, pricing, FAQ) |

### Auth (no sidebar layout)
| Route | Description |
|---|---|
| `/login` | Tenant + email + password, 2FA redirect |
| `/register` | New tenant registration (5-field form) |
| `/verify-email` | 6-digit OTP with backspace navigation + 60s resend |
| `/forgot-password` | Password reset request |
| `/reset-password` | Code + new password + confirm |
| `/accept-invite` | Validate invite token, create account |

### App Panel (authenticated, sidebar layout)
| Route | Description |
|---|---|
| `/dashboard` | 6 stat cards + recent service orders |
| `/customers` | List with search + pagination |
| `/customers/create` | New customer form |
| `/customers/[id]` | Customer detail + inline edit + add-vehicle |
| `/vehicles` | Plate-based vehicle search |
| `/vehicles/[id]/history` | Vehicle service history table |
| `/service-orders` | List with status filter + search |
| `/service-orders/new` | Customer search → vehicle select → create order |
| `/service-orders/[id]` | Full detail: operations/parts/consumables/payments tabs, status update, discount |
| `/service-orders/[id]/print` | Print-ready order document |
| `/inspections` | Expertiz list with package/status filters |
| `/inspections/new` | Create inspection with customer search |
| `/inspections/[id]` | Detail with grouped inspection items, inline result editing |
| `/inspections/[id]/print` | Print-ready inspection report |
| `/inventory` | Stock list with low-stock highlighting |
| `/inventory/create` | New inventory item |
| `/inventory/[id]` | Edit item + activate/deactivate + adjust stock |
| `/service-catalog` | Hizmet kataloğu list |
| `/service-catalog/create` | New service item |
| `/service-catalog/[id]` | Edit service item + activate/deactivate |
| `/settings/business` | Tenant profile form (Owner only) |
| `/settings/users` | User list + invite + role change + deactivate |

### Public QR (no-auth)
| Route | Description |
|---|---|
| `/public/service-record/[slug]` | Customer-facing service record |
| `/public/inspection-report/[slug]` | Customer-facing expertiz report |

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | Next.js 15.5.18, `output: 'standalone'` |
| Language | TypeScript (strict mode) |
| Styling | Tailwind CSS v3 + custom globals.css utility classes |
| Data fetching | TanStack Query v5 (`useQuery`, `useMutation`) |
| HTTP | Axios with Bearer token interceptors + 401 refresh queue |
| Auth state | React Context + localStorage (SSR-safe `isBrowser()` guard) |
| Icons | Lucide React |
| Forms | React state + validation |
| Locale | Turkish (tr-TR currency/date formatting) |

---

## Architecture Decisions

### API Proxy
`next.config.ts` rewrites `/api/:path*` → `http://api:8080/api/:path*` at server level. No nginx needed, no CORS issues.

### Auth Token Refresh
`src/core/api/client.ts` queues concurrent 401 responses while one refresh is in flight, retries all after the new token arrives. Clears tokens and redirects to `/login` on refresh failure.

### SSR Safety
All localStorage access goes through `isBrowser()` guard in `src/core/auth/storage.ts`. Pages using `useSearchParams()` are wrapped in `<Suspense>` per Next.js 15 requirement.

### Role-Based Navigation
The app shell (`src/app/(app)/layout.tsx`) filters sidebar nav items by `hasRole()`. Pages with Owner-only actions check `user?.role === 'Owner'` before rendering sensitive controls.

---

## Docker / Deploy Changes

`src/docker-compose.staging.yml` — added `web` service:
- Image: `ghcr.io/yigitcvk/motorcare-web:staging`
- Port: `127.0.0.1:${WEB_HOST_PORT:-3000}:8080`
- `API_BASE_URL=http://api:8080` (internal Docker network)
- `tmpfs` overrides for `/tmp` + `/app/.next/cache` (Next.js standalone needs cache write access)
- `depends_on: api: condition: service_healthy`
- Health check: hits `/api/health` through the Next.js rewrite proxy

`src/deploy/portainer/staging.env.example` — added: `WEB_IMAGE`, `WEB_HOST_PORT`, `NEXT_PUBLIC_APP_URL`, `WEB_MEM_LIMIT`, `WEB_CPUS`

---

## Documentation Created

| File | Purpose |
|---|---|
| `docs/frontend/mobile-readiness.md` | Storage adapter pattern, React Native path, PWA notes |
| `docs/frontend/blazor-decommission.md` | What was removed, why, route mapping, deploy checklist |

---

## Fixes Applied During QA

| File | Issue | Fix |
|---|---|---|
| `dashboard/page.tsx` | `<a>` tag instead of `<Link>` | Replaced with `<Link>` + added import |
| `dashboard/page.tsx` | `order['status']` type `unknown` used as ReactNode | Changed to `!!order['status']` boolean guard |
| `service-orders/[id]/page.tsx` | Unescaped `"` in JSX | Changed to `&quot;` |
| `service-orders/[id]/print/page.tsx` | Unescaped `"` in JSX | Changed to `&quot;` |
| `login/page.tsx` | `useSearchParams()` without Suspense | Split inner component + `<Suspense>` wrapper |
| `accept-invite/page.tsx` | `useSearchParams()` without Suspense | Split inner component + `<Suspense>` wrapper |
| `reset-password/page.tsx` | `useSearchParams()` without Suspense | Split inner component + `<Suspense>` wrapper |
| `verify-email/page.tsx` | `useSearchParams()` without Suspense | Split inner component + `<Suspense>` wrapper |

---

## Known Non-Blocking Warnings

3 ESLint warnings (unused variables) — these do not fail the build and are low priority:
- `dashboard/page.tsx`: `InlineLoading`, `criticalInspections` imported/assigned but not used
- `vehicles/page.tsx`: `router` declared but not used

---

## What's Left Before Production

1. **CI/CD pipeline** — Add GitHub Actions workflow to build and push `motorcare-web:staging` image on push to main
2. **Nginx/reverse proxy** — Configure staging server to proxy `https://staging.bakimsuite.com` → `localhost:3000`
3. **Environment variables** — Set `WEB_IMAGE`, `NEXT_PUBLIC_APP_URL`, and `WEB_HOST_PORT` in Portainer
4. **Smoke test** — Run `scripts/smoke/staging-password-reset-mailpit-smoke.sh` after deploy
5. **Service Catalog route in nav** — `/service-catalog` currently shows in the sidebar as "Hizmetler"; verify `layout.tsx` nav item link matches

---

*Generated by GarajPass orchestrator — 2026-05-25*

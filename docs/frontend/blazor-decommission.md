# Blazor Frontend Decommission

**Date:** 2026-05-25  
**Status:** Complete — Blazor removed, Next.js frontend live

---

## 1. Summary

The original Blazor Server frontend has been fully removed from the GarajPass repository. It has been replaced by a **Next.js 15 + React + TypeScript + Tailwind CSS** application located at `src/MotorCare.Web/`. All backend API contracts, domain logic, and database migrations are preserved and unchanged.

---

## 2. What Was Removed

| Item | Detail |
|---|---|
| Blazor Server project | Previously at `src/MotorCare.Web.Blazor/` (or similar) |
| Razor components (`.razor`) | All pages and layouts |
| Blazor-specific NuGet packages | `Microsoft.AspNetCore.Components.Web`, etc. |
| `_Host.cshtml` / `App.razor` | Entry point files |
| Blazor JS interop glue | Any `JSRuntime` interop scripts |
| nginx reverse-proxy container | Was needed to front the Blazor server; no longer required |

The backend project files (`MotorCare.Api`, `MotorCare.Application`, `MotorCare.Domain`, `MotorCare.Infrastructure`) are untouched.

---

## 3. Why Replaced

1. **React Native alignment** — The web frontend and a future mobile app now share the same ecosystem (React, TypeScript, TanStack Query, Axios). See `docs/frontend/mobile-readiness.md`.
2. **Better SaaS UI tooling** — The React + Tailwind ecosystem has a broader library of accessible, production-quality component systems (shadcn/ui, Radix, etc.).
3. **Standalone server** — Next.js with `output: 'standalone'` runs as a Node.js process, eliminating the need for a separate nginx container.
4. **API proxy built in** — Next.js rewrites (`/api/:path*`) proxy frontend requests to the .NET API container, solving CORS without additional infrastructure.
5. **Modern developer experience** — Hot module replacement, TypeScript strict mode, ESLint, file-based routing.

---

## 4. What Was Preserved

- All Carter endpoint modules (`src/MotorCare.Api/Modules/`)
- All MediatR commands and queries (`src/MotorCare.Application/`)
- All domain aggregates and value objects (`src/MotorCare.Domain/`)
- All EF Core migrations (`src/MotorCare.Infrastructure/Migrations/`)
- JWT auth configuration and middleware
- Rate limiting, correlation ID middleware
- All database schemas and seeding logic

No API contract changes were required for the frontend migration.

---

## 5. New Frontend Location

```
src/MotorCare.Web/
  src/
    app/                  # Next.js App Router (file-based routing)
      (auth)/             # Login, register pages
      (app)/              # Authenticated app shell
        dashboard/
        customers/
        service-orders/
        ...
    core/
      api/                # Axios API client + interceptors
      auth/               # Auth service, storage helpers
    components/           # Shared React components
    types/                # TypeScript interfaces (mirror .NET DTOs)
  next.config.ts
  Dockerfile
```

---

## 6. API Proxy (CORS-Free)

Next.js rewrites proxy all `/api/*` requests from the browser to the .NET API container. The browser never contacts the API directly, so no CORS headers are required on the API.

```typescript
// next.config.ts (simplified)
const config: NextConfig = {
  output: 'standalone',
  rewrites: async () => [
    {
      source: '/api/:path*',
      destination: `${process.env.API_BASE_URL}/api/:path*`,
    },
  ],
};
```

At runtime, `API_BASE_URL=http://api:8080` (internal Docker network name).

---

## 7. Route Mapping

| Old Blazor Route | New Next.js Route | Notes |
|---|---|---|
| `/` | `/dashboard` | Root redirects to dashboard |
| `/customers` | `/customers` | Unchanged path |
| `/customers/{id}` | `/customers/[id]` | Dynamic segment |
| `/service-orders` | `/service-orders` | Unchanged path |
| `/service-orders/{id}` | `/service-orders/[id]` | Dynamic segment |
| `/login` | `/login` | Unchanged path |
| `/logout` | Auth action — no page | Handled in auth service |

---

## 8. Docker Changes

A `web` service was added to `docker-compose.staging.yml` alongside the existing `api` service.

```yaml
# docker-compose.staging.yml (relevant excerpt)
services:
  api:
    image: ghcr.io/yigitcvk/motorcare-api:staging
    ports:
      - "5000:8080"

  web:
    image: ghcr.io/yigitcvk/motorcare-web:staging
    ports:
      - "3000:8080"
    environment:
      API_BASE_URL: http://api:8080
    depends_on:
      - api
```

Both containers expose port 8080 internally. The `web` container resolves the `api` container by service name over the internal Docker bridge network.

---

## 9. Deployment Notes

| Property | Value |
|---|---|
| Image | `ghcr.io/yigitcvk/motorcare-web:staging` |
| Base image | `node:24-alpine` (multi-stage build) |
| Build mode | `output: 'standalone'` |
| Internal port | `8080` |
| Required env var | `API_BASE_URL` (e.g., `http://api:8080`) |
| Optional env var | `NEXT_PUBLIC_APP_URL` (for absolute URL generation) |

The standalone build copies only the necessary Node.js files into the final image stage, keeping the image size minimal. No full `node_modules` directory is included in the final layer.

---

## 10. Post-Deployment Checklist

After deploying or updating the Next.js frontend:

- [ ] Confirm `/login` page loads and accepts credentials
- [ ] Confirm JWT token is stored under `mc.accessToken` in localStorage
- [ ] Confirm `/dashboard` is accessible after login
- [ ] Confirm `/api/health` (or equivalent) returns 200 from within the `web` container (proxy test)
- [ ] Confirm customers list page loads data via `/api/customers`
- [ ] Confirm service orders list page loads data
- [ ] Confirm logout clears tokens and redirects to `/login`
- [ ] Confirm no CORS errors appear in the browser console
- [ ] Confirm the `web` container can reach the `api` container on the Docker network (`API_BASE_URL` is set)
- [ ] Run backend smoke tests (`src/scripts/smoke/`) to verify API is healthy independently

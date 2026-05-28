# Mobile Readiness — GarajPass / GarajPass

**Last updated:** 2026-05-25 (Phase 2 enhancements)  
**Status:** Web frontend live; mobile app not yet started

---

## 1. Overview

The current web frontend (`src/MotorCare.Web/`) is built with **Next.js 15 + TypeScript + Tailwind CSS**. Because React is the rendering layer, the codebase shares the same ecosystem as **React Native / Expo**, making a future mobile app a natural extension rather than a rewrite.

The key insight is the separation of concerns already present in the web app:

- **Core layer** (`src/core/`) — auth logic, API client, storage abstraction, type definitions
- **UI layer** (`src/app/`, `src/components/`) — React/DOM-specific; cannot be shared with React Native

Everything in the core layer can be extracted into a shared package or replicated in a React Native project with minimal changes.

---

## 2. Shared Ecosystem

### What CAN be shared

| Artifact | Location (web) | Shareable? | Notes |
|---|---|---|---|
| TypeScript types/interfaces | `src/types/` | Yes | Plain TS, no DOM dependency |
| API endpoint constants | `src/core/api/endpoints.ts` | Yes | Just strings |
| Axios instance + interceptors | `src/core/api/apiClient.ts` | Yes | Axios runs in React Native |
| Auth service logic | `src/core/auth/authService.ts` | Yes | Swap storage adapter only |
| Format utilities | `src/lib/utils/` | Yes | Pure functions |
| Zod/FluentValidation-mirror schemas | `src/lib/validation/` | Yes | Zod runs in React Native |
| API response DTOs | `src/types/api/` | Yes | Mirror of .NET response models |
| React Query hooks | `src/hooks/` | Yes (with care) | Works in React Native via TanStack Query |

### What CANNOT be shared

| Artifact | Reason |
|---|---|
| React components (`src/components/`) | Import DOM elements (`div`, `input`, etc.) |
| Next.js routing (`useRouter`, `Link`) | Next.js-specific; use Expo Router instead |
| Tailwind CSS classes | No DOM in React Native; use StyleSheet or NativeWind |
| `next/image`, `next/font` | Next.js-only modules |
| Cookie-based logic | Mobile uses token storage, not cookies |

---

## 3. Storage Adapter Pattern

The web app defines and exports a `StorageAdapter` interface in `src/MotorCare.Web/src/core/auth/storage.ts`. The current web implementation is backed by `localStorage` with an `isBrowser()` guard for SSR safety.

### Interface (already in `storage.ts`)

```typescript
// src/core/auth/storage.ts
export interface StorageAdapter {
  get(key: string): string | null;
  set(key: string, value: string): void;
  remove(key: string): void;
  clear(): void;
}
```

The synchronous signature matches the web's `localStorage` API. A React Native implementation would wrap `AsyncStorage` calls behind `async`/`await` and expose an equivalent sync-style API, or the interface can be widened to return `Promise<…>` values when the mobile project is started.

### Web Adapter (current)

The module already exports individual helpers (`getAccessToken`, `setTokens`, `clearTokens`, etc.) built on top of `localStorage`. These are the integration points that a mobile adapter would replace.

### React Native Adapter (future — using `@react-native-async-storage/async-storage`)

```typescript
// mobile: core/auth/storage.native.ts
import AsyncStorage from '@react-native-async-storage/async-storage';
import { StorageAdapter } from './storage';

export const nativeStorageAdapter: StorageAdapter = {
  get: (key) => {
    // AsyncStorage is async; expose via a synchronous facade with a local cache
    // or widen the StorageAdapter interface to Promise<…> when porting.
    throw new Error('Use async helpers directly on mobile');
  },
  set: (key, value) => { AsyncStorage.setItem(key, value); },
  remove: (key) => { AsyncStorage.removeItem(key); },
  clear: () => { AsyncStorage.clear(); },
};
```

Prefer `expo-secure-store` over `@react-native-async-storage/async-storage` for tokens (see Section 6).

### Storage Keys (unchanged on both platforms)

```typescript
// already defined as KEYS inside storage.ts
const KEYS = {
  ACCESS_TOKEN:  'mc.accessToken',
  REFRESH_TOKEN: 'mc.refreshToken',
  CURRENT_USER:  'mc.currentUser',
  CLIENT_KEY:    'mc.clientKey',
} as const;
```

The auth service is then initialized with the appropriate adapter at app startup, keeping all token logic identical.

---

## 4. API Client for React Native

The Axios-based `apiClient` can be reused in React Native without modification. Axios is platform-agnostic and works in the React Native JS runtime.

### Reusable Pattern

```typescript
// shared: core/api/createApiClient.ts
import axios, { AxiosInstance } from 'axios';
import { IStorageAdapter } from '../auth/IStorageAdapter';
import { STORAGE_KEYS } from '../auth/storageKeys';

export function createApiClient(
  baseURL: string,
  storage: IStorageAdapter,
): AxiosInstance {
  const client = axios.create({ baseURL });

  // Request interceptor — attach Bearer token
  client.interceptors.request.use(async (config) => {
    const token = await storage.get(STORAGE_KEYS.ACCESS_TOKEN);
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
  });

  // Response interceptor — handle 401 / token refresh
  client.interceptors.response.use(
    (res) => res,
    async (error) => {
      if (error.response?.status === 401) {
        // trigger refresh or logout
      }
      return Promise.reject(error);
    },
  );

  return client;
}
```

### Base URL Strategy

The web app exposes `appConfig.apiBaseUrl` (from `src/MotorCare.Web/src/shared/config/env.ts`) which reads `NEXT_PUBLIC_API_BASE_URL`. On the Next.js web app this var is typically unused at runtime because API calls go through the internal Next.js proxy (`/api` rewrites). On mobile there is no proxy, so `apiBaseUrl` is used directly.

```typescript
// src/shared/config/env.ts
export const appConfig = {
  appName: process.env.NEXT_PUBLIC_APP_NAME ?? 'GarajPass',
  appUrl:  process.env.NEXT_PUBLIC_APP_URL  ?? 'http://localhost:3000',
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL ?? 'https://api.garajpass.com',
} as const;
```

| Environment | Web (Next.js) | React Native (Expo) |
|---|---|---|
| Local dev | `/api` (Next.js rewrite) | `http://localhost:5000` |
| Staging | `/api` (proxied) | `https://staging-api.bakimsuite.com` |
| Production | `appConfig.apiBaseUrl` → `https://api.garajpass.com` | `appConfig.apiBaseUrl` same value |

Mobile apps bypass the Next.js proxy entirely and pass `appConfig.apiBaseUrl` directly to `createApiClient`. In React Native, use `@env` (react-native-dotenv) or Expo's `extra` config in `app.config.ts` to inject the base URL at build time.

---

## 5. PWA Considerations

Before investing in a native mobile app, the Next.js app can be PWA-enabled to provide an installable, offline-capable experience on mobile browsers.

### Enable PWA with next-pwa

```bash
npm install next-pwa
```

```typescript
// next.config.ts
import withPWA from 'next-pwa';

export default withPWA({
  dest: 'public',
  disable: process.env.NODE_ENV === 'development',
  runtimeCaching: [
    {
      urlPattern: /^\/api\//,
      handler: 'NetworkFirst',
      options: { cacheName: 'api-cache', networkTimeoutSeconds: 10 },
    },
    {
      urlPattern: /\/_next\/static\//,
      handler: 'CacheFirst',
      options: { cacheName: 'static-cache' },
    },
  ],
})({ output: 'standalone' /* existing config */ });
```

### Manifest

Create `public/manifest.json`:

```json
{
  "name": "GarajPass",
  "short_name": "GarajPass",
  "description": "Motorcycle service management",
  "start_url": "/dashboard",
  "display": "standalone",
  "background_color": "#0f172a",
  "theme_color": "#0f172a",
  "icons": [
    { "src": "/icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "/icons/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

Add `<link rel="manifest" href="/manifest.json" />` and `<meta name="theme-color" content="#0f172a" />` to `src/app/layout.tsx`.

> Note: PWA is viable as a quick-win but does not replace a native app for features requiring camera, Bluetooth (OBD-II), or push notifications on iOS.

---

## 6. Recommended React Native Mobile Architecture

When the mobile app is started, the recommended setup is:

### Toolchain

| Concern | Tool |
|---|---|
| Framework | Expo (managed workflow) |
| Language | TypeScript (strict) |
| Routing | Expo Router v3 (file-based, mirrors Next.js) |
| Data fetching | TanStack Query v5 (same as web) |
| Auth state | Zustand or React Context + storage adapter |
| Secure token storage | `expo-secure-store` (replaces localStorage) |
| HTTP client | Axios (shared `createApiClient` factory) |
| Styling | NativeWind v4 (Tailwind for React Native) |

### SecureStore Adapter (Expo)

```typescript
// mobile: core/auth/storage.native.ts
import * as SecureStore from 'expo-secure-store';
import { IStorageAdapter } from './IStorageAdapter';

export const secureStorageAdapter: IStorageAdapter = {
  get: (key) => SecureStore.getItemAsync(key),
  set: (key, value) => SecureStore.setItemAsync(key, value),
  remove: (key) => SecureStore.deleteItemAsync(key),
};
```

Prefer `SecureStore` over `@react-native-async-storage/async-storage` for tokens on mobile — it uses the device Keychain / Keystore.

### Project Structure (Expo)

The web app already organises domain logic under `src/features/` with a consistent `hooks/`, `components/`, and `types/` layout per domain:

```
src/MotorCare.Web/src/features/
  auth/              hooks/, components/, types/
  customers/         hooks/, components/, types/
  vehicles/          hooks/, components/, types/
  service-orders/    hooks/, components/, types/
  inspections/       hooks/, components/, types/
  inventory/         hooks/, components/, types/
  service-catalog/   hooks/, components/, types/
  settings/          hooks/, components/, types/
  public-records/    hooks/, components/, types/
```

The `hooks/` barrel in each feature re-exports React Query hooks (e.g. `useCustomers`, `useServiceOrders`). These are the primary candidates for sharing with a mobile app.

A corresponding Expo project would mirror this structure:

```
garajpass-mobile/
  app/                    # Expo Router file-based routes
    (auth)/
      login.tsx
    (app)/
      dashboard.tsx
      customers/
  src/
    core/
      api/
        createApiClient.ts   # shared factory (copy or monorepo link)
        endpoints.ts         # shared constants
      auth/
        authService.ts       # shared logic
        storage.native.ts    # SecureStore adapter
    features/
      auth/            hooks/, types/  (copy or monorepo link from web)
      customers/       hooks/, types/
      service-orders/  hooks/, types/
    components/              # Native UI components (NOT shared with web)
    types/                   # TypeScript types (copy or monorepo link)
```

### Monorepo Option (future)

If maintenance overhead grows, consider a Turborepo/pnpm monorepo:

```
garajpass/
  apps/
    web/          # Next.js (current src/MotorCare.Web)
    mobile/       # Expo
  packages/
    api-client/   # shared axios factory + endpoint constants
    types/        # shared TypeScript interfaces
    utils/        # shared pure utilities
```

---

## 7. React Native Integration Path

When a React Native / Expo app is started, the following integration points already exist in the web codebase and can be consumed directly.

### Hook imports

Each `features/<domain>/hooks/index.ts` barrel re-exports the React Query hooks for that domain. A mobile app can import these directly (copy or monorepo link):

```typescript
// mobile app
import { useCustomers, useCreateCustomer } from 'features/customers/hooks';
import { useServiceOrders, useServiceOrderDetail } from 'features/service-orders/hooks';
import { useLogin, useLogout } from 'features/auth/hooks';
```

The hooks themselves have no DOM dependency — they call the `apiClient` over HTTP, which works identically in React Native.

### API base URL

Mobile bypasses the Next.js proxy. Pass `appConfig.apiBaseUrl` (from `src/shared/config/env.ts`) to `createApiClient`:

```typescript
import { appConfig } from 'shared/config/env';
import { createApiClient } from 'core/api/createApiClient';
import { secureStorageAdapter } from 'core/auth/storage.native';

export const apiClient = createApiClient(appConfig.apiBaseUrl, secureStorageAdapter);
```

`NEXT_PUBLIC_API_BASE_URL` resolves to the correct environment value (`https://api.garajpass.com` in production) via the `appConfig` object — no extra wiring needed.

### StorageAdapter implementation

Implement the `StorageAdapter` interface exported from `src/core/auth/storage.ts` using `@react-native-async-storage/async-storage`:

```typescript
// mobile: core/auth/storage.native.ts
import AsyncStorage from '@react-native-async-storage/async-storage';
import type { StorageAdapter } from 'core/auth/storage';

export const asyncStorageAdapter: StorageAdapter = {
  get:    (key) => { /* synchronous facade — see note in Section 3 */ return null; },
  set:    (key, value) => { AsyncStorage.setItem(key, value); },
  remove: (key) => { AsyncStorage.removeItem(key); },
  clear:  () => { AsyncStorage.clear(); },
};
```

Prefer `expo-secure-store` (see Section 6) over `AsyncStorage` for token storage on production mobile builds.

---

## 8. Current Readiness Checklist

### Already Done (Phase 1 + Phase 2)

- [x] TypeScript throughout the web app — types are portable
- [x] Axios-based API client with interceptor pattern
- [x] Bearer token auth (JWT) — same on web and mobile
- [x] `StorageAdapter` interface exported from `src/core/auth/storage.ts`
- [x] `isBrowser()` guard abstracts environment differences within storage helpers
- [x] Storage keys are constants (`mc.accessToken`, etc.)
- [x] `appConfig.apiBaseUrl` via `src/shared/config/env.ts` (`NEXT_PUBLIC_API_BASE_URL`) — mobile uses this directly
- [x] Auth flow (login, logout, token refresh) is service-isolated
- [x] `features/` directory structure organises domain hooks, components, and types in isolation
- [x] Feature hook barrels (`features/auth/hooks`, `features/service-orders/hooks`, etc.) ready for mobile import

### Needs Work Before Starting Mobile App

- [ ] Decide whether to widen `StorageAdapter` to async (`Promise<…>`) or keep sync-facade
- [ ] Refactor `authService.ts` to accept an injected storage adapter
- [ ] Extract `createApiClient` factory (remove any remaining Next.js-specific assumptions)
- [ ] Document all API endpoints in `endpoints.ts` (some may still be inline)
- [ ] Decide on monorepo vs. copy-paste for shared packages
- [ ] Add `manifest.json` + PWA config if quick mobile-web is needed first
- [ ] Confirm API CORS policy allows requests from Expo dev client origin

---

## 9. Shared UI/Core Notes Added by Frontend Worker B

### Money and date utilities

`src/MotorCare.Web/src/shared/utils/format.ts` now exposes stable aliases for mobile reuse:

- `formatMoney` / `money`
- `formatDate` / `dateText`
- `formatDateTime` / `dateTimeText`

These helpers are pure TypeScript and do not depend on DOM, Next.js, or browser APIs. They can be copied into a mobile shared package as-is. The money helper formats TRY with the `tr-TR` locale, and the date helpers return `-` for missing or invalid values so list/detail screens do not need repeated fallback logic.

### Storage and auth core reuse

The mobile app should reuse the current auth boundary, not the web UI implementation:

- Keep storage access behind `StorageAdapter`.
- Use the same token key names so logout, refresh, and session restore behavior stays aligned across clients.
- Prefer `expo-secure-store` for access/refresh tokens in production mobile builds.
- If the adapter is widened to async, update auth service methods and React Query auth hooks together so token reads are awaited consistently.

### Mobile-friendly UI components

The web UI kit now includes reusable responsive primitives under `src/components/ui/`:

- Data display: `DataTable`, `MobileCardList`, `DetailItem`, `Timeline`, `DateDisplay`, `StatusBadge`, `StatCard`
- Layout: `ResponsiveGrid`, `SectionHeader`, `FilterBar`, `Drawer`, `PrintLayout`
- Forms/actions: `FormField`, `Checkbox`, `CopyButton`, `QRLinkCard`, `RetryState`

These components are DOM/Tailwind based and should not be imported directly into React Native. Their prop shapes are intentionally simple so equivalent Native components can mirror them without coupling mobile screens to web markup.


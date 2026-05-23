# Angular Frontend Migration

Blazor frontend is deprecated for staging and production. The active MVP frontend is the Angular SPA under `src/MotorCare.Web`.

Deployment now builds the `app` service from `MotorCare.Web/Dockerfile` and serves the SPA with Nginx. API traffic is proxied through `/api/`, and all SPA routes fall back to `index.html`.

The first safe decommission step keeps `MotorCare.App` in the solution for historical/reference builds, but removes it from staging/production compose frontend serving. Full source deletion can be handled in a later cleanup after pilot acceptance.

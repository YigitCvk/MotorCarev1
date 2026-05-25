# Nginx Staging Configuration

Nginx reverse-proxy configuration for the two MotorCare / BakimSuite staging domains.

---

## Setup Instructions

1. **Obtain TLS certificates with certbot** (run once per domain):

   ```bash
   sudo certbot certonly --nginx \
     -d staging.bakimsuite.com \
     -d staging-api.bakimsuite.com \
     --email ops@bakimsuite.com \
     --agree-tos --non-interactive
   ```

2. **Save the config file** to `/etc/nginx/sites-available/bakimsuite-staging`:

   ```bash
   sudo cp nginx-staging.conf /etc/nginx/sites-available/bakimsuite-staging
   ```

3. **Enable the site** by creating a symlink:

   ```bash
   sudo ln -s /etc/nginx/sites-available/bakimsuite-staging \
               /etc/nginx/sites-enabled/bakimsuite-staging
   ```

4. **Test the configuration**:

   ```bash
   sudo nginx -t
   ```

5. **Reload Nginx** to apply changes:

   ```bash
   sudo systemctl reload nginx
   ```

---

## Docker Internal Hostnames

The two application containers run on a shared Docker network called `motorcare-staging`.
Within that network they are addressed by their container names:

| Service | Docker hostname | Internal port |
|---------|----------------|---------------|
| Next.js web frontend | `motorcare-web` | `8080` |
| .NET REST API | `motorcare-api` | `8080` |

Nginx is **not** itself inside the Docker network; it reaches the containers through
the ports they expose on the host (see `proxy_pass` directives below).
If Nginx is later added to the same Docker network, replace `127.0.0.1:PORT` with
`http://motorcare-web:8080` and `http://motorcare-api:8080` respectively.

---

## Nginx Configuration

Paste the block below into `/etc/nginx/sites-available/bakimsuite-staging`.

```nginx
# ============================================================
# Global gzip settings (place in http{} block or nginx.conf)
# ============================================================
# If you manage a separate nginx.conf, move the gzip directives
# there and remove the duplicate gzip_* lines from this file.

# ============================================================
# staging.bakimsuite.com  →  Next.js web container (port 8080)
# ============================================================

# --- HTTP → HTTPS redirect ---
server {
    listen 80;
    listen [::]:80;
    server_name staging.bakimsuite.com;

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 301 https://$host$request_uri;
    }
}

# --- HTTPS termination + proxy ---
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name staging.bakimsuite.com;

    # ---- TLS (Let's Encrypt / certbot) ----
    ssl_certificate     /etc/letsencrypt/live/staging.bakimsuite.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/staging.bakimsuite.com/privkey.pem;
    include             /etc/letsencrypt/options-ssl-nginx.conf;
    ssl_dhparam         /etc/letsencrypt/ssl-dhparams.pem;

    # ---- Security headers ----
    add_header X-Frame-Options          "SAMEORIGIN"                    always;
    add_header X-Content-Type-Options   "nosniff"                       always;
    add_header Referrer-Policy          "strict-origin-when-cross-origin" always;

    # ---- gzip compression ----
    gzip              on;
    gzip_vary         on;
    gzip_proxied      any;
    gzip_comp_level   6;
    gzip_buffers      16 8k;
    gzip_http_version 1.1;
    gzip_types
        text/plain
        text/css
        text/javascript
        application/javascript
        application/json
        application/x-javascript
        image/svg+xml;

    # ---- Upstream proxy defaults ----
    proxy_http_version  1.1;
    proxy_set_header    Host              $host;
    proxy_set_header    X-Real-IP         $remote_addr;
    proxy_set_header    X-Forwarded-For   $proxy_add_x_forwarded_for;
    proxy_set_header    X-Forwarded-Proto $scheme;

    # ---- Next.js static assets (aggressive cache) ----
    location /_next/static/ {
        proxy_pass http://127.0.0.1:8080;
        add_header Cache-Control "public, max-age=31536000, immutable";

        proxy_set_header Host              $host;
        proxy_set_header X-Real-IP         $remote_addr;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # ---- /api/* → .NET API container ----
    # Requests that begin with /api/ are forwarded directly to the API
    # container so the frontend can reach the backend on the same origin.
    location /api/ {
        proxy_pass http://127.0.0.1:8081;   # motorcare-api host port

        proxy_set_header Host              $host;
        proxy_set_header X-Real-IP         $remote_addr;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        proxy_read_timeout    60s;
        proxy_connect_timeout 10s;
    }

    # ---- WebSocket support (Next.js HMR / app routes) ----
    location /_next/webpack-hmr {
        proxy_pass http://127.0.0.1:8080;

        proxy_http_version 1.1;
        proxy_set_header Upgrade    $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host       $host;
    }

    # ---- SPA / Next.js catch-all ----
    location / {
        proxy_pass http://127.0.0.1:8080;

        # WebSocket upgrade headers (required for Next.js streaming / RSC)
        proxy_set_header Upgrade    $http_upgrade;
        proxy_set_header Connection $connection_upgrade;

        proxy_set_header Host              $host;
        proxy_set_header X-Real-IP         $remote_addr;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # SPA fallback: try real file, directory, then hand off to Next.js
        try_files $uri $uri/ @nextjs;
    }

    location @nextjs {
        proxy_pass http://127.0.0.1:8080;

        proxy_set_header Upgrade    $http_upgrade;
        proxy_set_header Connection $connection_upgrade;

        proxy_set_header Host              $host;
        proxy_set_header X-Real-IP         $remote_addr;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}


# ============================================================
# staging-api.bakimsuite.com  →  .NET API container (port 8081)
# ============================================================

# --- HTTP → HTTPS redirect ---
server {
    listen 80;
    listen [::]:80;
    server_name staging-api.bakimsuite.com;

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 301 https://$host$request_uri;
    }
}

# --- HTTPS termination + proxy ---
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name staging-api.bakimsuite.com;

    # ---- TLS (Let's Encrypt / certbot) ----
    ssl_certificate     /etc/letsencrypt/live/staging-api.bakimsuite.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/staging-api.bakimsuite.com/privkey.pem;
    include             /etc/letsencrypt/options-ssl-nginx.conf;
    ssl_dhparam         /etc/letsencrypt/ssl-dhparams.pem;

    # ---- Security headers ----
    # NOTE: CORS headers are intentionally omitted here.
    # The .NET API handles CORS via its own middleware; adding duplicate
    # Access-Control-* headers in Nginx would cause browser rejections.
    add_header X-Frame-Options          "SAMEORIGIN"                    always;
    add_header X-Content-Type-Options   "nosniff"                       always;
    add_header Referrer-Policy          "strict-origin-when-cross-origin" always;

    # ---- gzip compression ----
    gzip              on;
    gzip_vary         on;
    gzip_proxied      any;
    gzip_comp_level   6;
    gzip_buffers      16 8k;
    gzip_http_version 1.1;
    gzip_types
        text/plain
        text/css
        text/javascript
        application/javascript
        application/json
        application/x-javascript
        image/svg+xml;

    # ---- API proxy ----
    location / {
        proxy_pass http://127.0.0.1:8081;   # motorcare-api host port

        proxy_http_version 1.1;
        proxy_set_header Host              $host;
        proxy_set_header X-Real-IP         $remote_addr;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # Timeouts appropriate for long-running API operations
        proxy_read_timeout    60s;
        proxy_connect_timeout 10s;
    }
}
```

### WebSocket connection upgrade map

Add this snippet inside the top-level `http {}` block in `/etc/nginx/nginx.conf`
(or in a dedicated `/etc/nginx/conf.d/upgrade-map.conf` include) so that the
`$connection_upgrade` variable used above is defined:

```nginx
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      close;
}
```

---

## Port mapping reference

| Container | Docker-internal port | Host-exposed port | Nginx `proxy_pass` |
|-----------|---------------------|-------------------|--------------------|
| `motorcare-web` (Next.js) | `8080` | `8080` | `http://127.0.0.1:8080` |
| `motorcare-api` (.NET) | `8080` | `8081` | `http://127.0.0.1:8081` |

> The two containers both listen on port 8080 **inside** Docker but are mapped to
> different host ports (8080 and 8081) to avoid conflicts. Adjust the host-side
> port numbers in your `docker-compose.yml` if they differ in your deployment.

---

## Certbot auto-renewal

certbot installs a systemd timer by default. Verify it is active:

```bash
sudo systemctl status certbot.timer
```

After any certificate renewal Nginx must be reloaded. Add a deploy hook to handle
this automatically:

```bash
echo 'systemctl reload nginx' | \
  sudo tee /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
sudo chmod +x /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
```

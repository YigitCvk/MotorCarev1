# Nginx Staging Configuration

Nginx reverse-proxy configuration for the two GarajPass staging domains.

Staging live blocker: DNS A records must exist before certbot and HTTPS smoke can pass.

| Record | Type | Target |
|---|---|---|
| `staging.bakimsuite.com` | A | `46.225.166.254` |
| `staging-api.bakimsuite.com` | A | `46.225.166.254` |

Verify DNS before running certbot:

```bash
nslookup staging.bakimsuite.com
nslookup staging-api.bakimsuite.com
dig +short staging.bakimsuite.com
dig +short staging-api.bakimsuite.com
```

If DNS has not propagated, certbot can fail and browser HTTPS smoke will not be meaningful.

## Host Ports

Nginx runs on the host and proxies to the host-bound ports exposed by `src/docker-compose.staging.yml`.

| Public host | Upstream |
|---|---|
| `staging.bakimsuite.com` | `http://127.0.0.1:3000` |
| `staging-api.bakimsuite.com` | `http://127.0.0.1:5102` |

## Setup

Issue certificates after DNS resolves:

```bash
sudo certbot certonly --nginx \
  -d staging.bakimsuite.com \
  -d staging-api.bakimsuite.com \
  --email ops@bakimsuite.com \
  --agree-tos --non-interactive
```

Install and reload:

```bash
sudo cp nginx-staging.conf /etc/nginx/sites-available/motorcare-staging
sudo ln -s /etc/nginx/sites-available/motorcare-staging /etc/nginx/sites-enabled/motorcare-staging
sudo nginx -t
sudo systemctl reload nginx
```

Add the WebSocket upgrade map inside the top-level `http {}` block in `/etc/nginx/nginx.conf`
or in `/etc/nginx/conf.d/upgrade-map.conf`:

```nginx
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      close;
}
```

## Nginx Configuration

Paste the block below into `/etc/nginx/sites-available/motorcare-staging`.

```nginx
# staging.bakimsuite.com -> Next.js web app
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

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name staging.bakimsuite.com;

    ssl_certificate     /etc/letsencrypt/live/staging.bakimsuite.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/staging.bakimsuite.com/privkey.pem;
    include             /etc/letsencrypt/options-ssl-nginx.conf;
    ssl_dhparam         /etc/letsencrypt/ssl-dhparams.pem;

    client_max_body_size 25m;

    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    gzip on;
    gzip_vary on;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types text/plain text/css text/javascript application/javascript application/json image/svg+xml;

    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;

    location /_next/static/ {
        proxy_pass http://127.0.0.1:3000;
        add_header Cache-Control "public, max-age=31536000, immutable";
    }

    location /api/ {
        proxy_pass http://127.0.0.1:5102;
        proxy_read_timeout 60s;
        proxy_connect_timeout 10s;
    }

    location /_next/webpack-hmr {
        proxy_pass http://127.0.0.1:3000;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }

    location / {
        proxy_pass http://127.0.0.1:3000;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
    }
}

# staging-api.bakimsuite.com -> .NET API
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

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name staging-api.bakimsuite.com;

    ssl_certificate     /etc/letsencrypt/live/staging-api.bakimsuite.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/staging-api.bakimsuite.com/privkey.pem;
    include             /etc/letsencrypt/options-ssl-nginx.conf;
    ssl_dhparam         /etc/letsencrypt/ssl-dhparams.pem;

    client_max_body_size 25m;

    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    gzip on;
    gzip_vary on;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types text/plain text/css text/javascript application/javascript application/json image/svg+xml;

    location / {
        proxy_pass http://127.0.0.1:5102;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 60s;
        proxy_connect_timeout 10s;
    }
}
```

## Smoke Targets

- `https://staging.bakimsuite.com` should return the Next.js app.
- `https://staging.bakimsuite.com/_next/static/...` chunks should return 200.
- Hard refreshes on app routes should continue to resolve through Next.js.
- `https://staging-api.bakimsuite.com/api/version` should return 200 JSON.
- API upload requests should pass through with `client_max_body_size 25m`.

## Certbot Auto-Renewal

certbot installs a systemd timer by default:

```bash
sudo systemctl status certbot.timer
```

Reload Nginx after renewal:

```bash
echo 'systemctl reload nginx' | \
  sudo tee /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
sudo chmod +x /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
```

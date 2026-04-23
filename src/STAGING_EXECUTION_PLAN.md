# BakimSuite First Staging Deployment Execution Plan

This document turns the staging checklist into an execution-ready rollout plan for the first real staging deployment on a single Linux server.

It assumes:

- one Ubuntu server
- PostgreSQL on the same server for the first staging rollout
- Nginx as reverse proxy
- systemd for service supervision
- optional external Elasticsearch / Kibana

Current applied staging reference:

- provider: Hetzner Cloud
- server IP: `46.225.166.254`
- API bind: `127.0.0.1:5002`
- App bind: `127.0.0.1:5001`
- publish folders:
  - `/var/www/motorcare/publish/api`
  - `/var/www/motorcare/publish/app`
- service names:
  - `motorcare-api.service`
  - `motorcare-app.service`

## 1. Recommended Provider

Recommended first choice:

- Hetzner Cloud

Why:

- better price/performance for a staging environment
- simple Ubuntu VM provisioning
- sufficient networking and disk options
- easy future scale-up to a larger instance

Good alternative:

- DigitalOcean

Choose DigitalOcean if:

- the team already uses it
- managed PostgreSQL or managed networking is preferred

## 2. Recommended Staging Server Size

Minimum recommended size for first real staging:

- 2 vCPU
- 4 GB RAM
- 80 GB SSD

Practical mappings:

- Hetzner: `CPX21`
- DigitalOcean: `Basic 2 vCPU / 4 GB`

Why not smaller:

- API + App + PostgreSQL + Nginx on one server
- print views and Blazor server sessions are more comfortable with 4 GB RAM
- staging should be stable enough for smoke, QA and pre-release checks

If Elastic runs on the same box, increase to:

- 4 vCPU
- 8 GB RAM

Preferred approach:

- keep Elasticsearch/Kibana outside the application server if possible

## 3. Target Hostnames

Recommended:

- UI: `staging.bakimsuite.com`
- API: `staging-api.bakimsuite.com`

Required DNS records:

- `A staging.bakimsuite.com -> 46.225.166.254`
- `A staging-api.bakimsuite.com -> 46.225.166.254`

## 4. Folder Layout

Use:

```text
/var/www/motorcare/
  publish/
    api/
    app/
  releases/
  shared/
    dataprotection/
      api/
      app/
/etc/motorcare/
  api.env
  app.env
/var/log/motorcare/
```

Recommended conventions:

- current publish output goes into:
  - `/var/www/motorcare/publish/api`
  - `/var/www/motorcare/publish/app`
- keep env files root-owned under `/etc/motorcare`
- keep service-level logs in journald
- do not deploy directly from the git working tree

## 5. Server Provisioning Steps

## 5.1 Create the VM

Provision:

- Ubuntu 22.04 LTS or Ubuntu 24.04 LTS
- add your SSH key
- attach DNS records for UI and API hostnames

## 5.2 Base package setup

Run:

```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y nginx postgresql postgresql-contrib unzip curl ufw apt-transport-https ca-certificates gnupg
```

Set timezone if needed:

```bash
sudo timedatectl set-timezone Europe/Istanbul
```

## 5.3 Firewall

Recommended:

```bash
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw enable
sudo ufw status
```

## 6. Install .NET 8 Runtime

Follow Microsoft package feed steps for Ubuntu, then install ASP.NET Core runtime:

```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0 dotnet-sdk-8.0
dotnet --info
```

Use SDK on the server only if you plan to run migrations there.  
If migrations run in CI/CD, runtime alone is enough for app hosting.

## 7. PostgreSQL Setup

Switch to postgres user:

```bash
sudo -u postgres psql
```

Create user and database:

```sql
CREATE USER motorcare_user WITH PASSWORD 'CHANGE_ME_STRONG_PASSWORD';
CREATE DATABASE motorcare_staging OWNER motorcare_user;
\q
```

Connection string example:

```text
Host=127.0.0.1;Port=5432;Database=motorcare_staging;Username=motorcare_user;Password=CHANGE_ME_STRONG_PASSWORD
```

Verify:

```bash
psql "host=127.0.0.1 port=5432 dbname=motorcare_staging user=motorcare_user password=CHANGE_ME_STRONG_PASSWORD" -c '\dt'
```

## 8. Create Deploy Folders

Run:

```bash
sudo mkdir -p /var/www/motorcare/publish/api
sudo mkdir -p /var/www/motorcare/publish/app
sudo mkdir -p /var/www/motorcare/releases
sudo mkdir -p /var/www/motorcare/shared
sudo mkdir -p /var/www/motorcare/shared/dataprotection/api
sudo mkdir -p /var/www/motorcare/shared/dataprotection/app
sudo mkdir -p /etc/motorcare
sudo mkdir -p /var/log/motorcare
sudo chown -R $USER:$USER /var/www/motorcare
sudo chown -R www-data:www-data /var/www/motorcare/shared/dataprotection
```

## 9. Publish From Local or CI

From the repo root:

```bash
dotnet publish src/MotorCare.Api/MotorCare.Api.csproj -c Release -o ./publish/api
dotnet publish src/MotorCare.App/MotorCare.App.csproj -c Release -o ./publish/app
```

Upload with `rsync`:

```bash
rsync -avz ./publish/api/ user@SERVER_IP:/var/www/motorcare/publish/api/
rsync -avz ./publish/app/ user@SERVER_IP:/var/www/motorcare/publish/app/
```

## 10. Environment Variable Files

Create API env file:

`/etc/motorcare/api.env`

```bash
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://127.0.0.1:5002
ConnectionStrings__DefaultConnection=Host=127.0.0.1;Port=5432;Database=motorcare_staging;Username=motorcare_user;Password=CHANGE_ME_STRONG_PASSWORD
Jwt__Issuer=BakimSuite
Jwt__Audience=BakimSuite
Jwt__Key=CHANGE_ME_LONG_RANDOM_JWT_KEY
Jwt__AccessTokenMinutes=60
Jwt__RefreshTokenDays=7
Elastic__Uri=
Elastic__Username=
Elastic__Password=
Elastic__IndexFormat=motorcare-api-{0:yyyy.MM}
DataProtection__KeysPath=/var/www/motorcare/shared/dataprotection/api
```

Create App env file:

`/etc/motorcare/app.env`

```bash
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://127.0.0.1:5001
ApiBaseUrl=https://staging-api.bakimsuite.com
DataProtection__KeysPath=/var/www/motorcare/shared/dataprotection/app
```

Lock permissions:

```bash
sudo chown root:root /etc/motorcare/api.env /etc/motorcare/app.env
sudo chmod 600 /etc/motorcare/api.env /etc/motorcare/app.env
```

## 11. Run Database Migration

Recommended first rollout approach:

- run migration once before enabling services

If SDK is installed on the server and repo is available:

```bash
cd /path/to/repo
dotnet ef database update \
  --project src/MotorCare.Infrastructure/MotorCare.Infrastructure.csproj \
  --startup-project src/MotorCare.Api/MotorCare.Api.csproj
```

If CI/CD handles schema rollout, run the equivalent migration step there before publish swap.

## 12. systemd Service Files

Create API service:

`/etc/systemd/system/motorcare-api.service`

```ini
[Unit]
Description=MotorCare API Staging
After=network.target postgresql.service
Wants=postgresql.service

[Service]
WorkingDirectory=/var/www/motorcare/publish/api
ExecStart=/usr/bin/dotnet /var/www/motorcare/publish/api/MotorCare.Api.dll
Restart=always
RestartSec=5
SyslogIdentifier=motorcare-api
User=www-data
EnvironmentFile=/etc/motorcare/api.env

[Install]
WantedBy=multi-user.target
```

Create App service:

`/etc/systemd/system/motorcare-app.service`

```ini
[Unit]
Description=MotorCare App Staging
After=network.target

[Service]
WorkingDirectory=/var/www/motorcare/publish/app
ExecStart=/usr/bin/dotnet /var/www/motorcare/publish/app/MotorCare.App.dll
Restart=always
RestartSec=5
SyslogIdentifier=motorcare-app
User=www-data
EnvironmentFile=/etc/motorcare/app.env

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable motorcare-api
sudo systemctl enable motorcare-app
sudo systemctl start motorcare-api
sudo systemctl start motorcare-app
```

Check:

```bash
sudo systemctl status motorcare-api --no-pager
sudo systemctl status motorcare-app --no-pager
journalctl -u motorcare-api -n 100 --no-pager
journalctl -u motorcare-app -n 100 --no-pager
```

## 13. Nginx Reverse Proxy Config

Domain-based staging config:

`/etc/nginx/sites-available/motorcare-staging`

```nginx
server {
    listen 80;
    listen [::]:80;
    server_name staging.bakimsuite.com staging-api.bakimsuite.com;

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

    ssl_certificate /etc/letsencrypt/live/staging-api.bakimsuite.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/staging-api.bakimsuite.com/privkey.pem;

    location = /health {
        proxy_pass http://127.0.0.1:5002/health;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Request-Id $request_id;
    }

    location / {
        proxy_pass http://127.0.0.1:5002;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Request-Id $request_id;
    }
}

server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name staging.bakimsuite.com;

    ssl_certificate /etc/letsencrypt/live/staging.bakimsuite.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/staging.bakimsuite.com/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:5001;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Request-Id $request_id;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_cache_bypass $http_upgrade;
    }
}
```

Enable:

```bash
sudo rm -f /etc/nginx/sites-enabled/default
sudo ln -s /etc/nginx/sites-available/motorcare-staging /etc/nginx/sites-enabled/motorcare-staging
sudo nginx -t
sudo systemctl restart nginx
```

## 14. SSL Setup

Use Let's Encrypt:

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo mkdir -p /var/www/certbot
sudo certbot --nginx -d staging.bakimsuite.com -d staging-api.bakimsuite.com
```

Verify renewal:

```bash
sudo systemctl status certbot.timer --no-pager
sudo certbot renew --dry-run
```

## 15. First Health Checks

Server-local checks:

```bash
curl http://127.0.0.1:5002/health
curl -I http://127.0.0.1:5001
```

Public checks:

```bash
curl -I https://staging.bakimsuite.com
curl https://staging-api.bakimsuite.com/health
```

Expected API health response:

```json
{
  "status": "Healthy",
  "service": "MotorCare.Api"
}
```

## 16. Kibana / Elastic Verification

After first requests:

1. confirm the target index exists:
   - `motorcare-api-staging-*`
2. verify fields:
   - `ApplicationName`
   - `Environment`
   - `EventId`
   - `EventName`
   - `CorrelationId`
   - `TenantId`
   - `UserId`
3. filter by:
   - `EventName : "LoginSucceeded"`
   - `EventName : "CustomerCreated"`
   - `EventName : "AppointmentCreated"`
   - `EventName : "ServiceOrderCreated"`
   - `EventName : "InspectionCreated"`

See:

- `OBSERVABILITY_DEPLOYMENT_CHECKLIST.md`

## 17. Go-Live Smoke Checklist

Run this in staging immediately after rollout:

1. `GET /health`
2. login
3. create customer
4. create vehicle
5. create appointment
6. convert appointment to service order or create service order directly
7. create inspection
8. open inspection print page
9. open service order print page
10. confirm logs appear in Kibana

## 18. Recommended First Rollout Sequence

1. provision VM
2. configure DNS
3. install packages
4. install .NET runtime
5. create PostgreSQL db and user
6. create folder layout
7. publish API and App
8. place env files
9. run migrations
10. install systemd services
11. start services
12. configure domain-based Nginx
13. issue SSL certificates
14. verify `https://staging-api.bakimsuite.com/health`
15. run smoke checklist
16. verify Kibana logs

## 19. Rollback Plan

For first staging rollout, keep rollback simple:

1. keep previous publish output under `/var/www/motorcare/releases`
2. stop services
3. restore previous API and App publish folders
4. start services
5. verify `/health`

If schema changes are backward-incompatible, do not deploy without a tested rollback path for the database.

## 20. Next Step For Production

After one stable staging cycle:

- separate PostgreSQL from the app server if needed
- add CI/CD deployment automation
- add backup + restore drill
- define zero-downtime or low-downtime publish flow
- decide whether production will use managed PostgreSQL and managed Elastic

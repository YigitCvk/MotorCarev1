#!/usr/bin/env bash
set -euo pipefail

cd /opt/motorcare-stack-src

api_conn="$(grep '^ConnectionStrings__DefaultConnection=' /etc/motorcare/api.env | cut -d= -f2-)"
jwt_key="$(grep '^Jwt__Key=' /etc/motorcare/api.env | cut -d= -f2-)"
postgres_password="${api_conn##*Password=}"

cat > .env <<EOF
POSTGRES_DB=motorcare_staging
POSTGRES_USER=motorcare_user
POSTGRES_PASSWORD=${postgres_password}
JWT_ISSUER=BakimSuite
JWT_AUDIENCE=BakimSuite
JWT_KEY=${jwt_key}
JWT_ACCESS_TOKEN_MINUTES=60
JWT_REFRESH_TOKEN_DAYS=7
ELASTIC_URI=
ELASTIC_USERNAME=
ELASTIC_PASSWORD=
ELASTIC_INDEX_FORMAT=motorcare-api-staging-{0:yyyy.MM}
APP_INTERNAL_API_BASE_URL=http://api:8080
API_HOST_PORT=5102
APP_HOST_PORT=5101
EOF

docker compose --env-file .env -f docker-compose.staging.yml build api app migrator
docker compose --env-file .env -f docker-compose.staging.yml up -d postgres api app
sleep 20

echo '--- container health ---'
docker compose --env-file .env -f docker-compose.staging.yml ps
echo '--- api health ---'
curl -sS http://127.0.0.1:5102/health
echo
echo '--- app assets ---'
curl -I -sS http://127.0.0.1:5101/app.css | head -n 5

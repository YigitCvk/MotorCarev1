# BakımSuite Portainer Staging Deploy

Bu doküman staging kurulumunu systemd publish akışından Docker + Portainer akışına taşımak için minimum uygulanabilir yolu tanımlar.

## 1. Neden Portainer

- staging deploy’u tekrarlanabilir hale getirir
- compose stack tek yerden yönetilir
- app/api/postgres container olarak izlenebilir
- GitHub Actions + webhook ile redeploy tetiklenebilir
- systemd publish klasörü yerine image tabanlı sürümleme sağlar

## 2. Hedef Mimari

- host: Hetzner Ubuntu 24.04
- Docker Engine
- Portainer CE
- stack:
  - `postgres`
  - `api`
  - `app`
  - opsiyonel `migrator` one-off job
- host-level nginx:
  - `staging.bakimsuite.com -> 127.0.0.1:5001`
  - `staging-api.bakimsuite.com -> 127.0.0.1:5002`

Host-level nginx seçimi bilinçli:
- sertifika ve reverse proxy tek yerde kalır
- Blazor Server websocket proxy davranışı sade kalır
- Portainer stack daha basit olur

## 3. Docker Kurulumu

```bash
sudo apt update
sudo apt install -y ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg
echo \
  \"deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo \"$VERSION_CODENAME\") stable\" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo systemctl enable docker
sudo systemctl start docker
docker --version
docker compose version
```

## 4. Portainer CE Kurulumu

```bash
docker volume create portainer_data
docker run -d \
  --name portainer \
  --restart=always \
  -p 8000:8000 \
  -p 9443:9443 \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v portainer_data:/data \
  portainer/portainer-ce:lts
```

Portainer UI:
- `https://SERVER_IP:9443`

## 5. Repo Tabanlı Stack

Portainer içinde:
1. `Stacks`
2. `Add stack`
3. `Repository` seç
4. repo URL gir
5. compose path:
   - `docker-compose.staging.yml`
6. environment variables alanına `deploy/portainer/staging.env.example` içindeki değerlerin gerçeklerini gir

Önemli environment variable’lar:
- `POSTGRES_PASSWORD`
- `JWT_KEY`
- `ELASTIC_URI`
- `ELASTIC_USERNAME`
- `ELASTIC_PASSWORD`
- `API_IMAGE`
- `APP_IMAGE`
- `MIGRATOR_IMAGE`

## 6. Migration Stratejisi

Bu pass için en sade ve güvenli yol:
- otomatik startup migration yok
- release öncesi one-off migrator çalıştır

Komut:

```bash
docker compose -f docker-compose.staging.yml --profile tools run --rm migrator
```

Portainer tarafında aynı şey:
- stack deployed olduktan sonra temporary console/container run ile `migrator` çalıştır

## 7. Nginx

Host-level nginx örnek config:
- `deploy/nginx/motorcare-staging.containers.conf`

Container port eşleşmeleri:
- app -> `127.0.0.1:5001`
- api -> `127.0.0.1:5002`

## 8. Webhook ile Redeploy

Portainer stack oluşturulduktan sonra:
- webhook URL üret
- bunu GitHub Actions secret olarak kaydet:
  - `PORTAINER_STAGING_WEBHOOK`

Workflow:
1. image build
2. GHCR push
3. Portainer webhook çağrısı
4. stack pull/redeploy

## 9. GitHub Secrets

Önerilen secret’lar:
- `GHCR_USERNAME`
- `GHCR_TOKEN`
- `PORTAINER_STAGING_WEBHOOK`

## 10. Rollout / Switch Planı

1. mevcut systemd deployment çalışır halde kalsın
2. Docker stack localhost portlarında ayağa kaldır:
   - `5001`
   - `5002`
3. host-header smoke yap
4. nginx upstream’i container portlarına doğrula
5. smoke tamamlanınca systemd servisleri durdur
6. rollback gerekirse nginx upstream’i tekrar systemd süreçlerine döndür

## 11. Kısa Smoke Checklist

1. `docker compose -f docker-compose.staging.yml up -d postgres api app`
2. `curl http://127.0.0.1:5002/health`
3. `curl -I http://127.0.0.1:5001/app.css`
4. `curl -I http://127.0.0.1:5001/MotorCare.App.styles.css`
5. `curl -I http://127.0.0.1:5001/js/storage.js`
6. `curl -I -H "Host: staging.bakimsuite.com" http://127.0.0.1/`
7. login
8. create customer
9. create appointment
10. create service order

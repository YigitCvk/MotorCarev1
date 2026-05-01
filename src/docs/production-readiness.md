# GarajPass Production Readiness Checklist

## 1. Environment

- [ ] `ConnectionStrings` production PostgreSQL'e bakiyor.
- [ ] `Jwt:Key`, `Issuer`, `Audience` production icin guclu ve dogru.
- [ ] `Email` SMTP host, port, username, password ve FromName dogru.
- [ ] `AppBaseUrl` production domainine ayarli.
- [ ] Logging seviyesi production icin uygun; debug noise kapali.
- [ ] `AllowedHosts` production domainleriyle sinirli.
- [ ] CORS varsa production origin listesi dogru.
- [ ] `ASPNETCORE_ENVIRONMENT=Production`.

## 2. Database

- [ ] Tum migration'lar production DB'ye uygulanabilir.
- [ ] Migration uygulama adimi deploy planinda net.
- [ ] Otomatik backup plani var.
- [ ] Restore testi yapildi ve sonucu kaydedildi.
- [ ] Seed data production icin guvenli.
- [ ] Default admin/test user temizlendi veya devre disi.
- [ ] Postgres container recreate edilmeden app/api deploy edilebiliyor.

## 3. Security

- [ ] HTTPS zorunlu.
- [ ] Cookie kullanimi varsa secure/samesite ayarlari kontrol edildi.
- [ ] JWT secret yeterli uzunluk ve entropiye sahip.
- [ ] Refresh token DB'de guvenli tutuluyor ve logout/reset sonrasi invalidate oluyor.
- [ ] Password policy aktif.
- [ ] Forgot/reset rate limit ve attempt limit calisiyor.
- [ ] Upload varsa dosya tipi/boyut validation aktif.
- [ ] Sensitive log kontrolu: password, token, reset code, SMTP secret ve reset link loglanmiyor.

## 4. Observability

- [ ] Structured logs aktif.
- [ ] Error loglari merkezi olarak izleniyor.
- [ ] `/health` endpoint kontrol ediliyor.
- [ ] `/api/version` commit, build time ve migration bilgisini gosteriyor.
- [ ] Deployment commit tracking release notuna yaziliyor.
- [ ] Alerting onerisi: 5xx orani, app unhealthy, DB connection failure ve SMTP failure icin alarm.

## 5. Deployment

- [ ] Docker Compose project name dogru.
- [ ] Portainer recreate adimlari dokumante.
- [ ] Postgres container'a dokunmama notu deploy runbook'ta var.
- [ ] Rollback stratejisi: onceki image tag/commit ve env metadata geri alinabilir.
- [ ] App/API recreate sonrasi health ve `/api/version` kontrol ediliyor.
- [ ] Smoke test sonrasi onay aliniyor.

## 6. Business Validation

- [ ] Test tenant ile login/register smoke.
- [ ] Test service order olusturma.
- [ ] Test appointment haftalik plan kontrolu.
- [ ] Test inspection print/body map kontrolu.
- [ ] Test email verification ve password reset code.
- [ ] Test mobile viewport.

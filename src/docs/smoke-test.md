# GarajPass Staging Smoke Test

Bu dokuman staging deploy sonrasi tekrarlanabilir browser-level smoke adimlarini tanimlar. Test kullanici sifresi, access token, reset code ve SMTP secret raporlanmaz.

## Ortam

- App: `http://127.0.0.1:5101` veya staging domain/proxy
- API: `http://127.0.0.1:5102` veya staging API domain/proxy
- Compose project: `motorcare-stack-src`
- Staging env: `.env.staging`
- Postgres container smoke sirasinda recreate edilmez.

Local makineden domain/DNS erisimi yoksa SSH port-forward ile test edilebilir:

```powershell
ssh -N -L 15101:127.0.0.1:5101 -L 15102:127.0.0.1:5102 root@46.225.166.254
```

Bu durumda App `http://127.0.0.1:15101`, API `http://127.0.0.1:15102` olarak kullanilir.

## Test Tenant

Tercih edilen tenant code:

```text
smoke-garajpass
```

Bu tenant daha once olusturulduysa timestamp suffix kullan:

```text
smoke-garajpass-YYYYMMDDHHmmss
```

Test email erisilebilir bir mailbox olmali. Mailbox erisimi yoksa sadece staging smoke icin ilgili test user DB'de verified yapilabilir; bu durum raporda acikca belirtilir. Password raporlanmaz.

## Smoke Kapsami

1. `/api/version` kontrol et.
   - Commit beklenen deploy commit'i olmali.
   - `latestAppliedMigration` beklenen migration'i gostermeli.

2. Public sayfalar:
   - `/` landing acilir.
   - GarajPass brand gorunur.
   - Eski BakimSuite/Garaj360 gorunen metinleri yoktur.
   - `/login`, `/register`, `/forgot-password`, `/reset-password` acilir.

3. Register/login:
   - Test tenant olustur.
   - Email verification gerekiyorsa inbox kod/link ile dogrula.
   - Inbox yoksa sadece smoke user icin DB verification yapildigini raporla.
   - Login basarili olur.
   - Dashboard acilir.

4. Appointments weekly plan:
   - `/appointments` varsayilan haftalik plan acilir.
   - Gunler Pazartesi-Pazar siralanir.
   - Onceki hafta, Bu hafta, Sonraki hafta calisir.
   - Liste gorunumune gecis calisir.
   - En az 3 randevu farkli gun/saatlerde olusturulur.
   - Kartlarda saat, musteri, arac, hizmet/complaint ve status gorunur.
   - Kart tiklaninca appointment detail acilir.

5. Service catalog price:
   - `/services` acilir.
   - Yeni hizmet olustur: `Periyodik Bakim Smoke`, fiyat `1250`, currency `TRY`.
   - Liste/detail/edit tarafinda fiyat gorunur.
   - Edit ile fiyat `1500` yapilir.
   - Negatif fiyat denenir ve `422` validation beklenir.

6. Vehicle motorcycle catalog:
   - `/vehicles/create` acilir.
   - Arac tipi `Motosiklet` secilir.
   - Marka aramasinda `hon` -> Honda suggestion gorunur.
   - Model aramasinda `africa` veya `cb` -> model suggestion gorunur.
   - Arac kaydedilir ve listede marka/model dogru gorunur.

7. Service order full flow:
   - `/service-orders/create` acilir.
   - Musteri ve arac secilir.
   - Hizmet secilince fiyat varsa catalog price'dan gelir.
   - Sarf urun eklenir: kategori `Motor Yagi`, marka/urun `Motul`.
   - Servis emri kaydedilir.
   - Detail sayfasinda hizmet, iscilik, parca, tahsilat ve sarf urunler gorunur.
   - Dashboard'a donunce sayfa hata vermez.

8. ServiceOrderDetail button smoke:
   - Gercek servis emri detail acilir.
   - `Yazdir / PDF Al`, `Listeye Don`, `Iscilik`, `Parca`, `Tahsilat`, `Durum Guncelle` denenir.
   - Login/public sayfaya dusme olmaz.
   - Auth state korunur.

9. Inspection print/PDF:
   - Motosiklet ekspertizi olusturulur.
   - Body map uzerinde en az 3 parca isaretlenir.
   - `/inspections/{id}/print` acilir.
   - Motosiklet gorseli, marker'lar, legend ve parca notlari gorunur.
   - Print preview/PDF layout kontrol edilir.

10. Mobile spot check:
    - 390px genislikte landing ve `/appointments` render edilir.
    - Toolbar ve kartlar tasma/kirilma yapmaz.

11. Log/security check:
    - `docker logs motorcare-staging-api --tail 500`
    - `docker logs motorcare-staging-app --tail 300`
    - Ara: `exception`, `error`, `500`, unexpected `401`, `circuit`, `ObjectDisposedException`, `JSDisconnectedException`, `token`, `password`, `code`, `SMTP`.
    - Expected validation/tenant conflict disinda unexpected 500 olmamali.
    - Code/password/token/SMTP secret degeri loglanmamali.

## Rapor Formati

- Deploy commit
- Test tenant/email
- Email verification yontemi
- Public/branding sonucu
- Auth/dashboard sonucu
- Appointments sonucu
- Service catalog sonucu
- Vehicle catalog sonucu
- Service order sonucu
- Inspection print sonucu
- Mobile sonucu
- Log/security sonucu
- Bulunan bug ve fix

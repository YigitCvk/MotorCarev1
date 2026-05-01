# GarajPass UAT Test Plan

## 1. Test Amaci

Urunun gercek kullanici gozuyle servis, randevu, arac, musteri ve is emri akislarini dogrulamak.

## 2. Test Kullanicisina Verilecek Bilgi

- Staging URL: ayri kanaldan paylasilacak.
- Test tenant adi: ayri kanaldan paylasilacak.
- Test email: ayri kanaldan paylasilacak.
- Gecici password ayri kanaldan paylasilacak.
- Gercek musteri verisi girilmemeli.
- Password, reset code veya token ekran goruntulerinde paylasilmamali.

## 3. Test Senaryolari

### A. Giris ve Genel Kontrol

- Login ol.
- Dashboard aciliyor mu?
- Menuleri gez.
- Mobilde temel sayfalari kontrol et.

### B. Musteri Olustur

- Yeni musteri ekle.
- Musteri listesinde gor.
- Musteri detayini ac.

### C. Arac Olustur

- Motosiklet marka/model gir.
- Suggestion calisiyor mu kontrol et.
- Plaka ve sasi gibi alanlarda validation kontrol et.

### D. Randevu Olustur

- Yeni randevu ekle.
- Haftalik gorunumde gor.
- Onceki/sonraki hafta gecisini dene.
- Liste gorunumunu ac/kapat.

### E. Servis Emri Olustur

- Musteri ve arac sec.
- Sikayet/aciklama gir.
- Servis emrini kaydet.
- Detaya git.

### F. Iscilik / Parca / Tahsilat Ekle

- Iscilik ekle.
- Parca ekle.
- Tahsilat ekle.
- Negatif fiyat girmeyi dene; validation goruyor musun kontrol et.

### G. Ekspertiz / Inspection

- Ekspertiz sayfasini ac.
- Motosiklet gorselinde parca/marker notlarini kontrol et.
- Yazdir/PDF ekranini ac.

### H. Katalog

- Servis katalog kalemi ekle.
- Fiyat ekle/guncelle.
- Listede dogru gorunuyor mu kontrol et.

### I. Logout / Login

- Logout ol.
- Tekrar login ol.
- Veri duruyor mu kontrol et.

## 4. Kullanicidan Istenecek Geri Bildirim

- Anlasilmayan ekranlar.
- Yavas gelen sayfalar.
- Mobilde bozuk gorunen yerler.
- Eksik oldugunu dusundugu alanlar.
- Gercek servis kullaniminda sart dedigi ozellikler.

## 5. Kabul Kriteri

- Kullanici musteri, arac, randevu ve servis emri olusturabiliyor.
- Iscilik, parca ve tahsilat ekleyebiliyor.
- Ekspertiz print/PDF acabiliyor.
- Kritik hata veya veri kaybi yok.
- Kullanici temel akisi anlayabiliyor.


# MotorCare MVP — User Stories (Yön A: Servis Ustasının Dijital Defteri)

**Tarih:** 2026-04-18  
**Ürün:** MotorCare — Küçük Oto Servisler için İş Emri Yönetimi  
**Hedef Kitle:** Servis sahibi / servis ustası  
**Format:** Gherkin (Given / When / Then)

---

## Kod Eşleşme Açıklaması

Her story için kullanılan kısaltmalar:

| Sembol | Anlam |
|---|---|
| ✅ | Mevcut — tam çalışır |
| 🔶 | Kısmen var — tamamlanması gerekiyor |
| ❌ | Hiç yok — sıfırdan yazılacak |

---

## EPİK 1 — Tenant Onboarding (Servis Kaydı)

> **Hedef:** Bir servis sahibi sisteme kaydolabilmeli ve kendi izole ortamında çalışabilmeli.

---

### US-01 — Yeni Servis Kaydı

**As a** servis sahibi  
**I want to** işletmemi sisteme kaydetmek  
**So that** çalışanlarım sistemi kullanmaya başlasın ve verilerim diğer servislerden ayrı kalsın

#### Acceptance Criteria

```gherkin
Feature: Servis Kaydı (Tenant Onboarding)

  Scenario: Başarılı servis kaydı
    Given sisteme daha önce kayıtlı olmayan bir işletme sahibiyim
    When "Mehmet Oto Servis" adıyla "mehmet-oto" kimliğiyle kayıt isteği gönderirim
    Then sistemde yeni bir tenant oluşturulur
    And tenant "IsActive = true" olarak ayarlanır
    And her sonraki işlemimde verim diğer servislerden tamamen izole çalışır

  Scenario: Aynı kimlik (identifier) ile tekrar kayıt
    Given "mehmet-oto" kimliğiyle kayıtlı bir servis zaten var
    When aynı "mehmet-oto" kimliğiyle yeni kayıt isteği gelirsem
    Then 409 Conflict hatası döner
    And "Bu kimlik zaten kullanılıyor" mesajı gösterilir

  Scenario: Eksik bilgiyle kayıt girişimi
    Given kayıt formundayım
    When işletme adını boş bırakıp kaydı tamamlamaya çalışırsam
    Then 422 Unprocessable Entity hatası döner
    And hangi alanın eksik olduğu belirtilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `Tenant` aggregate — `Identifier`, `Name`, `IsActive`, `Deactivate()` |
| Application | ❌ | `CreateTenantCommand` + Handler + Validator yazılacak |
| API | ❌ | `TenantsModule` Carter endpoint'i yazılacak |
| Frontend | ❌ | MAUI onboarding ekranı Sprint 2 |

**Eksik:** Application handler ve Carter endpoint yok.

---

### US-02 — Servis Pasifleştirme

**As a** servis sahibi  
**I want to** aboneliğimi askıya almak  
**So that** verilerim silinmeden sistemi geçici olarak kapayabileyim

```gherkin
  Scenario: Servis hesabını devre dışı bırakma
    Given aktif bir servis hesabım var
    When hesabımı devre dışı bırakma isteği gönderirim
    Then tenant "IsActive = false" olarak güncellenir
    And sisteme erişim engellenir

  Scenario: Pasif servis üzerinde işlem yapmaya çalışma
    Given "IsActive = false" olan bir tenant için X-Tenant-Id gelir
    When herhangi bir işlem yapılmaya çalışılırsa
    Then 401 Unauthorized döner
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `Tenant.Deactivate()` metodu mevcut |
| Application | ❌ | `DeactivateTenantCommand` + Handler yazılacak |
| API | ❌ | `PATCH /api/tenants/{identifier}/deactivate` Carter endpoint'i |
| Middleware | ❌ | Pasif tenant kontrolü (header middleware veya handler'da) |

---

## EPİK 2 — Müşteri Yönetimi

> **Hedef:** Servis ustası müşterilerini kayıt edebilmeli, arayabilmeli ve güncelleyebilmeli.

---

### US-03 — Yeni Müşteri Kaydı

**As a** servis ustası  
**I want to** yeni bir müşteri kaydı oluşturmak  
**So that** aracını getiren müşteriye sonraki ziyaretlerinde hızlıca ulaşabileyim

```gherkin
Feature: Müşteri Kaydı

  Scenario: Başarılı müşteri kaydı
    Given geçerli bir X-Tenant-Id header'ı ile bağlıyım
    When "Ahmet Yılmaz", telefon "05321234567" ile yeni müşteri kaydı oluştururum
    Then sistemde yeni müşteri oluşturulur
    And telefon numarası normalize edilerek kaydedilir (5321234567)
    And müşteri ID'si döner

  Scenario: Aynı telefon numarasıyla müşteri kaydı
    Given "05321234567" numaralı bir müşteri zaten kayıtlı
    When aynı telefon numarasıyla yeni müşteri oluşturmaya çalışırsam
    Then 409 Conflict hatası döner
    And "Bu telefon numarasıyla kayıtlı müşteri zaten var" mesajı gösterilir

  Scenario: WhatsApp numarası ekleme
    Given yeni müşteri kaydı oluştururum
    When WhatsApp numarası farklı bir numara "05359876543" olarak belirtilir
    Then her iki numara ayrı ayrı kaydedilir
    And müşteri iletişim kartında WhatsApp butonu aktif olur

  Scenario: Geçersiz telefon numarası
    Given yeni müşteri kaydı formundayım
    When telefon alanına "123" (10 haneden kısa) giriş yaparım
    Then 422 hatası döner
    And "Geçerli bir telefon numarası giriniz" uyarısı gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `Customer` aggregate, `PhoneNumber` VO — tam |
| Application | ✅ | `CreateCustomerCommand` + Handler + Validator — tam |
| API | ❌ | `CustomersModule` Carter endpoint'i yazılacak |
| Frontend | ❌ | MAUI müşteri formu — Sprint 2 |

**Eksik:** Sadece Carter endpoint yok, handler hazır.

---

### US-04 — Müşteri Arama

**As a** servis ustası  
**I want to** müşteri adı veya telefon numarasıyla arama yapmak  
**So that** aracını getiren müşteriyi hızlıca bulabileyim ve yeni müşteri mi yoksa kayıtlı mı olduğunu anlayayım

```gherkin
Feature: Müşteri Arama

  Scenario: İsme göre müşteri arama
    Given sistemde "Ahmet Yılmaz" adlı kayıtlı müşteri var
    When "ahmet" arama terimi ile arama yaparım
    Then "Ahmet Yılmaz" arama sonuçlarında görünür
    And sadece kendi servisimin müşterileri listelenir (tenant izolasyonu)

  Scenario: Telefona göre müşteri arama
    Given "05321234567" numaralı müşteri kayıtlı
    When "532" arama terimi ile arama yaparım
    Then ilgili müşteri sonuçlarda görünür

  Scenario: Sonuç bulunamadı
    Given "Mehmet Demir" adlı müşteri kayıtlı değil
    When "Mehmet Demir" ile arama yaparım
    Then boş liste döner
    And "Müşteri bulunamadı, yeni kayıt oluşturabilirsiniz" mesajı gösterilir

  Scenario: Arama terimi çok kısa
    Given arama formundayım
    When 1 karakterlik arama terimi girirsem
    Then 422 hatası döner
    And "En az 2 karakter giriniz" mesajı gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `ICustomerRepository.SearchAsync(tenantId, searchTerm)` — tam |
| Application | ❌ | `SearchCustomersQuery` + Handler + Validator yazılacak |
| API | ❌ | `GET /api/customers/search?q=...` Carter endpoint'i |
| Frontend | ❌ | MAUI arama bileşeni — Sprint 2 |

---

### US-05 — Müşteri Detayı Görüntüleme

**As a** servis ustası  
**I want to** bir müşterinin profilini ve araç geçmişini görmek  
**So that** müşteri geldiğinde geçmiş işlemleri hatırlayabileyim

```gherkin
Feature: Müşteri Detayı

  Scenario: Müşteri profili görüntüleme
    Given "Ahmet Yılmaz" ID'si bilinen kayıtlı bir müşteri
    When müşteri detay sayfasına giderim
    Then müşteri adı, telefon, email, WhatsApp görünür
    And müşteriye ait araçların listesi görünür

  Scenario: Bulunamayan müşteri
    Given geçersiz bir müşteri ID'si kullanırım
    When detay sayfasına erişmeye çalışırsam
    Then 404 Not Found döner
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `Customer` aggregate |
| Application | ❌ | `GetCustomerByIdQuery` + Handler yazılacak |
| API | ❌ | `GET /api/customers/{id}` Carter endpoint'i |
| Frontend | ❌ | MAUI müşteri detay sayfası — Sprint 2 |

---

## EPİK 3 — Araç Yönetimi

> **Hedef:** Servis ustası araçları plakaya göre kayıt edebilmeli ve müşteriye bağlayabilmeli.

---

### US-06 — Araç Kaydı

**As a** servis ustası  
**I want to** servisime gelen aracı plaka numarasıyla kaydetmek  
**So that** aynı araç tekrar geldiğinde geçmiş servislerini görebilim

```gherkin
Feature: Araç Kaydı

  Scenario: Başarılı araç kaydı
    Given geçerli X-Tenant-Id ile bağlıyım
    When "34 ABC 123" plakalı, 2019 model Toyota Corolla kaydı yaparım
    Then araç sisteme eklenir
    And plaka "34ABC123" olarak normalize edilerek kaydedilir
    And araç ID'si döner

  Scenario: Türk plaka formatı normalizasyonu
    Given yeni araç kaydı formundayım
    When "34-abc-123" veya "34.abc.123" veya "34 ABC 123" gibi farklı formatlar girerim
    Then hepsi "34ABC123" olarak eşit kabul edilir
    And aynı tenant'ta duplicate kayıt oluşturulamaz

  Scenario: Aynı plaka ile tekrar kayıt
    Given "34ABC123" plakalı araç zaten kayıtlı
    When aynı plaka ile yeni araç kaydı yapılmaya çalışılırsa
    Then 409 Conflict hatası döner

  Scenario: Araç müşteriye bağlama
    Given "34ABC123" plakalı araç ve kayıtlı "Ahmet Yılmaz" müşterisi var
    When araç kaydı sırasında müşteri seçilirse
    Then araç Ahmet Yılmaz'a bağlanır
    And müşteri profilinde bu araç görünür
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `Vehicle` aggregate, `PlateNumber` VO, `AssignCustomer()` |
| Application | ✅ | `CreateVehicleCommand` + Handler (+ SetChassisAndColor fix tamamlandı) |
| API | ✅ | `POST /api/vehicles` — VehiclesModule Carter endpoint'i mevcut |
| Frontend | ❌ | MAUI araç formu — Sprint 2 |

**Eksik:** Sadece MAUI frontend yok.

---

### US-07 — Araç Plakaya Göre Arama

**As a** servis ustası  
**I want to** gelen aracı plaka numarasıyla hızlıca bulmak  
**So that** daha önce gelen araçları saniyeler içinde tespit edebilim

```gherkin
Feature: Plakaya Göre Araç Arama

  Scenario: Kayıtlı araç bulma
    Given "34ABC123" plakalı araç kayıtlı
    When "34 ABC 123" olarak arama yaparsam
    Then araç bilgileri (plaka, marka, model, yıl) döner
    And varsa bağlı müşteri bilgisi de eklenir

  Scenario: Kayıtsız araç
    Given "06XYZ999" plakalı araç kayıtlı değil
    When bu plaka ile arama yaparsam
    Then 404 döner
    And "Bu plakayla kayıtlı araç yok, yeni kayıt oluşturabilirsiniz" mesajı gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `IVehicleRepository.GetByPlateAsync()` |
| Application | ✅ | `GetVehicleByPlateQuery` + Handler — tam |
| API | ✅ | `GET /api/vehicles/{plate}` — VehiclesModule Carter mevcut |
| Frontend | ❌ | MAUI arama çubuğu — Sprint 2 |

**Bu story tamamen hazır!** Sadece MAUI yok.

---

## EPİK 4 — İş Emri Yönetimi

> **Hedef:** Servis ustası dijital iş emri açabilmeli, durumunu güncelleyebilmeli ve tüm süreci takip edebilmeli.

---

### US-08 — İş Emri Açma

**As a** servis ustası  
**I want to** bir araç için yeni iş emri açmak  
**So that** hangi aracın serviste olduğunu, müşterinin şikayetini ve işlem sürecini kayıt altına alabileyim

```gherkin
Feature: İş Emri Açma

  Scenario: Başarılı iş emri açma
    Given "34ABC123" plakalı araç ve bağlı müşteri kayıtlı
    When 85000 km'de, "motor sesi geliyor" şikayetiyle iş emri açarım
    Then iş emri "SRV-20260418-0001" numarasıyla oluşturulur
    And durum "Açık" (Open) olarak ayarlanır
    And açılış tarihi ve saati otomatik kaydedilir
    And kilometre bilgisi iş emrine eklenir

  Scenario: Aynı gün ikinci iş emri numarası
    Given bugün daha önce açılmış bir iş emri var (SRV-20260418-0001)
    When aynı gün ikinci iş emri açılırsa
    Then "SRV-20260418-0002" numarasıyla oluşturulur

  Scenario: Müşteri bilgisi olmadan iş emri açılmaya çalışma
    Given geçersiz bir müşteri ID'si kullanıyorum
    When iş emri açmaya çalışırsam
    Then 404 Not Found döner
    And "Müşteri bulunamadı" mesajı gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `ServiceOrder` constructor, `OrderNo` formatı, `ServiceOrderStatus.Open` |
| Application | ❌ | `CreateServiceOrderCommand` + Handler + Validator yazılacak |
| API | ❌ | `POST /api/service-orders` Carter endpoint'i |
| Frontend | ❌ | MAUI iş emri formu — Sprint 2 |

---

### US-09 — İş Emri Durum Geçişleri

**As a** servis ustası  
**I want to** iş emrinin durumunu güncellemek  
**So that** aracın servisteki sürecini (bekliyor, çalışılıyor, parça bekleniyor, tamamlandı, teslim edildi) takip edebilelim

```gherkin
Feature: İş Emri Durum Yönetimi

  Scenario: İş emrine başlanması
    Given "Açık" durumda iş emri var
    When "Çalışmaya Başla" aksiyonu tetiklenir
    Then durum "Devam Ediyor" (InProgress) olur
    And işlem zamanı kaydedilir

  Scenario: Parça bekleme durumuna alınma
    Given "Devam Ediyor" durumda iş emri var
    When "Parça Bekleniyor" aksiyonu tetiklenir
    Then durum "Parça Bekleniyor" (WaitingForParts) olur

  Scenario: Parça geldikten sonra devam etme
    Given "Parça Bekleniyor" durumunda iş emri var
    When "Devam Et" aksiyonu tetiklenir
    Then durum tekrar "Devam Ediyor" olur

  Scenario: İş emrini tamamlama
    Given "Devam Ediyor" veya "Parça Bekleniyor" durumunda iş emri var
    When "Tamamlandı" aksiyonu tetiklenir
    Then durum "Tamamlandı" (Completed) olur
    And kapanış tarihi ve saati otomatik kaydedilir

  Scenario: Araç teslimi
    Given "Tamamlandı" durumda iş emri var
    When "Teslim Edildi" aksiyonu tetiklenir
    Then durum "Teslim Edildi" (Delivered) olur

  Scenario: İş emri iptali
    Given "Açık", "Devam Ediyor" veya "Parça Bekleniyor" durumunda iş emri var
    When "İptal Et" aksiyonu tetiklenir
    Then durum "İptal Edildi" (Cancelled) olur
    And kapanış zamanı kaydedilir

  Scenario: Tamamlanmış iş emrini değiştirmeye çalışma
    Given "Tamamlandı" durumunda iş emri var
    When işçilik kalemi eklemeye çalışırsam
    Then 422 hatası döner
    And "Tamamlanmış iş emrine değişiklik yapılamaz" mesajı gösterilir

  Scenario: Geçersiz durum geçişi
    Given "Açık" durumda iş emri var
    When direkt "Tamamlandı" yapmaya çalışırsam (InProgress atlanarak)
    Then 422 hatası döner
    And "Bu durumdan tamamlama yapılamaz, önce çalışmaya başlayın" mesajı gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `StartProgress()`, `WaitForParts()`, `ResumeProgress()`, `MarkAsCompleted()`, `MarkAsDelivered()`, `Cancel()`, `EnsureModifiable()` — tam state machine |
| Application | ❌ | `UpdateServiceOrderStatusCommand` + Handler yazılacak |
| API | ❌ | `PATCH /api/service-orders/{id}/status` Carter endpoint'i |
| Frontend | ❌ | MAUI durum butonları — Sprint 2 |

---

### US-10 — İş Emri Detayı Görüntüleme

**As a** servis ustası  
**I want to** aktif bir iş emrinin tüm detaylarını görmek  
**So that** hangi işlemlerin yapıldığını, toplam tutarı ve ödeme durumunu takip edebileyim

```gherkin
Feature: İş Emri Detay Görüntüleme

  Scenario: İş emri detayları
    Given "SRV-20260418-0001" numaralı iş emri var
    When detay sayfasına giderim
    Then araç bilgisi (plaka, marka, model), müşteri adı görünür
    And tüm işçilik kalemleri ve tutarları görünür
    And tüm parça kalemleri ve tutarları görünür
    And toplam tutar, ödenen tutar, kalan bakiye görünür
    And ödeme geçmişi görünür
    And mevcut durum gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `ServiceOrder` tüm ilişkili koleksiyonlarla (Operations, Parts, Payments) |
| Application | ❌ | `GetServiceOrderByIdQuery` + Handler yazılacak |
| API | ❌ | `GET /api/service-orders/{id}` Carter endpoint'i |
| Frontend | ❌ | MAUI detay sayfası — Sprint 2 |

---

## EPİK 5 — İşçilik ve Parça Yönetimi

> **Hedef:** Servis ustası iş emrine yapılan işçilikleri ve kullanılan parçaları ekleyebilmeli.

---

### US-11 — İşçilik Kalemi Ekleme

**As a** servis ustası  
**I want to** iş emrine yaptığım işçilikleri ve ücretlerini kaydetmek  
**So that** müşteriye doğru fatura çıkarabileyim ve gelirimi takip edebileyim

```gherkin
Feature: İşçilik Kalemi Yönetimi

  Scenario: İşçilik kalemi ekleme
    Given "Devam Ediyor" durumda iş emri var
    When "Yağ değişimi" işçiliği 500 TL olarak eklenir
    Then işçilik listesine eklenir
    And işçilik toplamı güncellenir (LaborTotal)
    And genel toplam (GrandTotal) yeniden hesaplanır

  Scenario: Birden fazla işçilik kalemi
    Given iş emrine 2 işçilik eklenmiş (500 + 300 = 800 TL)
    When üçüncü işçilik "Balata değişimi" 400 TL olarak eklenir
    Then toplam işçilik 1200 TL olur

  Scenario: İşçilik kaldırma
    Given iş emrinde bir işçilik kalemi var
    When bu kalemi silerim
    Then kalem listeden çıkar
    And toplam güncellenir

  Scenario: Tamamlanmış iş emrine işçilik ekleme
    Given "Tamamlandı" durumda iş emri var
    When işçilik eklemeye çalışırsam
    Then 422 hatası döner
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `ServiceOrder.AddOperation()`, `RemoveOperation()`, `RecalculateTotals()` — tam |
| Application | ❌ | `AddOperationToOrderCommand` + Handler + Validator yazılacak |
| API | ❌ | `POST /api/service-orders/{id}/operations` Carter endpoint'i |
| Frontend | ❌ | MAUI işçilik formu — Sprint 2 |

---

### US-12 — Parça Kalemi Ekleme

**As a** servis ustası  
**I want to** iş emrine kullanılan yedek parçaları ve fiyatlarını girmek  
**So that** maliyet takibi yapabileyim ve müşteriye hangi parçaların takıldığını gösterebileyim

```gherkin
Feature: Parça Kalemi Yönetimi

  Scenario: Parça ekleme
    Given "Devam Ediyor" durumda iş emri var
    When "Yağ filtresi" parçası, 1 adet, 150 TL birim fiyatla eklenir
    Then parça listesine eklenir
    And parça toplamı güncellenir (PartsTotal)
    And genel toplam (GrandTotal) yeniden hesaplanır

  Scenario: Birden fazla adet parça
    Given iş emrine parça ekleyeceğim
    When "Cıvata" parçası, 4 adet, 25 TL birim fiyatla eklenir
    Then toplam 100 TL parça fiyatı hesaplanır (4 x 25)

  Scenario: Parça numarası isteğe bağlı
    Given yeni parça ekliyorum
    When parça numarasını boş bırakıp kaydedersem
    Then parça başarıyla kaydedilir
    And parça numarası "-" veya boş gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `ServiceOrder.AddPart()`, `RemovePart()`, `ServicePartItem.TotalPrice` hesaplama — tam |
| Application | ❌ | `AddPartToOrderCommand` + Handler + Validator yazılacak |
| API | ❌ | `POST /api/service-orders/{id}/parts` Carter endpoint'i |
| Frontend | ❌ | MAUI parça formu — Sprint 2 |

---

### US-13 — İndirim Uygulama

**As a** servis ustası  
**I want to** iş emrine indirim uygulamak  
**So that** müşteriye özel fiyat verebilirken kayıtlarım doğru kalsın

```gherkin
Feature: İndirim Yönetimi

  Scenario: Geçerli indirim uygulama
    Given işçilik + parça toplamı 1000 TL olan iş emri var
    When 100 TL indirim uygularım
    Then genel toplam 900 TL olur
    And indirim miktarı ayrıca gösterilir

  Scenario: Toplamı aşan indirim girişimi
    Given işçilik + parça toplamı 1000 TL
    When 1200 TL indirim girmeye çalışırsam
    Then 422 hatası döner
    And "İndirim toplam tutarı geçemez" mesajı gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `ServiceOrder.SetDiscount()` — validasyon mevcut |
| Application | ❌ | `SetOrderDiscountCommand` + Handler yazılacak |
| API | ❌ | `PATCH /api/service-orders/{id}/discount` Carter endpoint'i |
| Frontend | ❌ | — |

---

## EPİK 6 — Ödeme Yönetimi

> **Hedef:** Servis ustası nakit, kart veya havale ile ödeme alabilmeli ve kalan bakiyeyi takip edebilmeli.

---

### US-14 — Ödeme Kaydı

**As a** servis ustası  
**I want to** müşteriden aldığım ödemeyi kaydetmek  
**So that** hangi araçta ne kadar ödeme alındığını ve kalan bakiyeyi anlık görebileyim

```gherkin
Feature: Ödeme Kaydı

  Scenario: Nakit ödeme kaydı
    Given toplam 1000 TL, 0 TL ödenmiş iş emri var
    When nakit 500 TL ödeme kaydedilir
    Then ödeme listesine "Nakit / 500 TL" eklenir
    And ödenen toplam 500 TL olur
    And kalan bakiye 500 TL olur

  Scenario: Kredi kartı ile tam ödeme
    Given kalan bakiyesi 1000 TL olan iş emri var
    When kredi kartıyla 1000 TL ödeme yapılırsa
    Then kalan bakiye 0 TL olur
    And iş emri "tam ödendi" olarak işaretlenir

  Scenario: Havale ile ödeme
    Given kalan bakiyesi 800 TL olan iş emri var
    When banka havalesi olarak 800 TL ödeme kaydedilir
    Then ödeme yöntemi "Havale" olarak kaydedilir

  Scenario: Kalan bakiyeyi aşan ödeme girişimi
    Given kalan bakiyesi 500 TL olan iş emri var
    When 600 TL ödeme kaydetmeye çalışırsam
    Then 422 hatası döner
    And "Ödeme tutarı kalan bakiyeyi geçemez" mesajı gösterilir

  Scenario: Sıfır veya negatif ödeme girişimi
    Given aktif iş emri var
    When 0 TL veya -100 TL ödeme kaydetmeye çalışırsam
    Then 422 hatası döner
    And "Geçerli bir ödeme tutarı giriniz" mesajı gösterilir
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | ✅ | `ServiceOrder.AddPayment()`, `ServicePayment`, `PaymentMethod` enum — tam |
| Application | ❌ | `AddPaymentToOrderCommand` + Handler + Validator yazılacak |
| API | ❌ | `POST /api/service-orders/{id}/payments` Carter endpoint'i |
| Frontend | ❌ | MAUI ödeme formu — Sprint 2 |

---

## EPİK 7 — Günlük Özet Dashboard

> **Hedef:** Servis sahibi günün aktivitesini tek bakışta görebilmeli.

---

### US-15 — Günlük Özet

**As a** servis sahibi  
**I want to** gün sonunda bugün ne kadar iş yapıldığını görmek  
**So that** günlük ciromun farkında olayım ve servisin ne kadar meşgul olduğunu anlayayım

```gherkin
Feature: Günlük Özet Dashboard

  Scenario: Bugünkü özet görüntüleme
    Given bugün 5 iş emri açılmış ve çeşitli işlemler yapılmış
    When günlük özet ekranını açarım
    Then "Bugün Açılan İş Emri: 5" görünür
    And "Bugün Tamamlanan İş Emri: 3" görünür
    And "Devam Eden İş Emri: 2" görünür
    And "Bugün Tahsil Edilen: 4.500 TL" görünür
    And "Servisteki Araç Sayısı: 2" görünür

  Scenario: Hiç iş emri yokken özet
    Given bugün hiç iş emri açılmamış
    When günlük özet ekranını açarım
    Then tüm sayılar 0 gösterilir
    And "Bugün henüz iş emri açılmamış" mesajı görünür
```

#### Kod Eşleşmesi

| Katman | Durum | Detay |
|---|---|---|
| Domain | 🔶 | `IServiceOrderRepository.GetTodayOrderCountAsync()` var; diğer aggregation yok |
| Application | ❌ | `GetDailySummaryQuery` + Handler — sıfırdan yazılacak |
| API | ❌ | `GET /api/dashboard/daily` Carter endpoint'i |
| Frontend | ❌ | MAUI dashboard sayfası — Sprint 2 |

**Bu story tamamen yeni — domain'de de yok.**

---

## User Story Özet Tablosu

| Story | Epik | Domain | Application | API | Öncelik Sprint 1 |
|---|---|---|---|---|---|
| US-01 Servis Kaydı | Tenant | ✅ | ❌ | ❌ | Must |
| US-02 Servis Pasifleştirme | Tenant | ✅ | ❌ | ❌ | Should |
| US-03 Müşteri Kaydı | Müşteri | ✅ | ✅ | ❌ | Must |
| US-04 Müşteri Arama | Müşteri | ✅ | ❌ | ❌ | Must |
| US-05 Müşteri Detayı | Müşteri | ✅ | ❌ | ❌ | Must |
| US-06 Araç Kaydı | Araç | ✅ | ✅ | ✅ | **Hazır** |
| US-07 Plakaya Araç Arama | Araç | ✅ | ✅ | ✅ | **Hazır** |
| US-08 İş Emri Açma | İş Emri | ✅ | ❌ | ❌ | Must |
| US-09 Durum Geçişleri | İş Emri | ✅ | ❌ | ❌ | Must |
| US-10 İş Emri Detayı | İş Emri | ✅ | ❌ | ❌ | Must |
| US-11 İşçilik Ekleme | İşçilik/Parça | ✅ | ❌ | ❌ | Must |
| US-12 Parça Ekleme | İşçilik/Parça | ✅ | ❌ | ❌ | Must |
| US-13 İndirim Uygulama | İşçilik/Parça | ✅ | ❌ | ❌ | Should |
| US-14 Ödeme Kaydı | Ödeme | ✅ | ❌ | ❌ | Must |
| US-15 Günlük Özet | Dashboard | 🔶 | ❌ | ❌ | Should |

**Özet:** 15 story, tüm Domain ✅, Application 2/15 ✅, API 2/15 ✅

---

*Oluşturulma: 2026-04-18 | Güncelleme bekleniyor: Sprint 1 mid-review*

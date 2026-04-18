# Claude Code Prompt'ları

## 1. PM — Klasörü Analiz Et + Sprint 1 Planı

Aşağıdaki prompt'u Claude Code'a yapıştır:

---

Sen bir Product Manager'sın. Bu projeyi kök dizininden başlayarak analiz et.

Görevlerin:
1. Tüm dosya ve klasör yapısını incele — `.agents/`, `GEMINI.md`, `CLAUDE.md`, `AGENTS.md` dahil
2. Agent tanımlarını, skill'leri ve workflow'ları oku
3. Projenin mevcut durumunu özetle: neler hazır, neler eksik
4. Aşağıdaki formatta bir Sprint 1 planı oluştur:

**Sprint 1 Planı** (2 hafta):
- Sprint Goal: [tek cümle]
- User Stories (MoSCoW önceliklendirmesiyle):
  - Must Have: [...]
  - Should Have: [...]
  - Could Have: [...]
- Teknik Tasklar:
  - [ ] Task adı — tahmini süre — sorumlu agent (@analyst/@backend/@frontend)
  - [ ] ...
- Definition of Done kriterleri
- Riskler ve bağımlılıklar

Planı `production_artifacts/sprint1_plan.md` dosyasına kaydet.

SaaS fikri henüz belirlenmedi — bu yüzden sprint 1'i "altyapı + scaffold" odaklı planla:
- .NET 8 solution scaffold (Clean Architecture)
- Domain layer base classes (Entity, AggregateRoot, ValueObject, Result)
- EF Core + PostgreSQL altyapısı
- Carter API boilerplate + health check
- MAUI project scaffold + Shell navigation
- CI pipeline (dotnet build + test)

---

## 2. Code Reviewer — Mevcut Dosyaları İncele

Aşağıdaki prompt'u Claude Code'a yapıştır:

---

Sen bir Senior .NET Architect / Code Reviewer'sın. Bu projeyi kök dizininden başlayarak tamamen incele.

Görevlerin:
1. Tüm dosyaları oku — markdown, config, her şey
2. Aşağıdaki kategorilerde sorunları raporla:

**Kritik (Must Fix):**
- Yapısal hatalar, eksik konfigürasyonlar, çelişkili kurallar

**Uyarı (Should Fix):**
- İyileştirilebilecek tanımlar, eksik edge case'ler, belirsiz instruction'lar

**Öneri (Nice to Have):**
- Best practice önerileri, ek skill/workflow fikirleri

İnceleme kapsamı:
- `.agents/agents.md` — Agent rollerinin tutarlılığı, sorumluluk çakışması var mı?
- `.agents/skills/*.md` — Skill talimatları yeterince detaylı mı? Eksik adım var mı?
- `.agents/workflows/*.md` — Pipeline akışında mantık hatası var mı?
- `GEMINI.md` — Kurallar arasında çelişki var mı?
- `CLAUDE.md` — Claude Code için yeterli context var mı?
- `AGENTS.md` — Cross-tool uyumluluk doğru mu?
- `.gitignore` — Eksik pattern var mı?
- `README.md` — Dökümantasyon yeterli mi?

Raporu şu formatta yaz:
```
## Code Review Raporu
Tarih: [bugün]
Reviewer: Senior .NET Architect (AI)

### Kritik (X adet)
1. [dosya:satır] — Açıklama — Önerilen düzeltme

### Uyarı (X adet)
1. [dosya] — Açıklama — Önerilen düzeltme

### Öneri (X adet)
1. Açıklama

### Genel Değerlendirme
[2-3 cümle özet]
```

Raporu `production_artifacts/code_review_report.md` dosyasına kaydet.

---

# SaaS Antigravity Agent Pipeline

Multi-agent SaaS geliştirme pipeline'ı — Google Antigravity IDE ile.

## Ekip

| Agent      | Rol                     | Skill Dosyası                |
|------------|-------------------------|------------------------------|
| @analyst   | Business Analyst / DDD  | `analyze_requirements.md`    |
| @pm        | Product Manager         | `write_specs.md`             |
| @backend   | .NET 8+ Backend Dev     | `implement_backend.md`       |
| @frontend  | .NET MAUI Frontend Dev  | `implement_frontend.md`      |

## Kurulum

### 1. Antigravity IDE'yi Kur
```bash
# https://antigravity.google adresinden indir
# Windows, macOS, Linux destekli
# Gmail hesabıyla giriş yap (ücretsiz preview)
```

### 2. Repo'yu Aç
```bash
git init saas-app
cd saas-app
# Bu template'in içeriğini kopyala
```

### 3. Antigravity'de Aç
- Antigravity IDE'yi başlat
- "Open Workspace" ile repo klasörünü aç
- Planning Mode'u seç (Fast değil)
- Agent-Assisted Development modu önerilir

### 4. Pipeline'ı Başlat
Antigravity chat'inde:
```
/buildsaas Abonelik bazlı proje yönetim SaaS uygulaması
```

## Pipeline Akışı

```
Kullanıcı Fikri
    ↓
@analyst → Domain Model + User Stories + Context Map
    ↓ (kullanıcı onayı)
@pm → Technical Specification + Sprint Plan
    ↓ (kullanıcı onayı)
@backend → .NET 8 Clean Architecture + DDD Implementation
    ↓ (dotnet build + test)
@frontend → .NET MAUI MVVM Implementation
    ↓ (dotnet build)
Çalışan Prototip ✓
```

## Proje Yapısı (Üretilecek)

```
src/
├── SaaSApp.Domain/           # Entities, Value Objects, Aggregates
├── SaaSApp.Application/      # MediatR Handlers, DTOs, Validators
├── SaaSApp.Infrastructure/   # EF Core, Repositories
├── SaaSApp.Api/              # Carter Endpoints, Middleware
├── SaaSApp.Maui/             # MAUI Views, ViewModels, Services
└── SaaSApp.Domain.Tests/     # xUnit Domain Tests

production_artifacts/
├── specs/
│   ├── domain_model.md
│   ├── user_stories.md
│   └── context_map.md
└── Technical_Specification.md
```

## Tech Stack

- .NET 8+ / C# 12
- Entity Framework Core (PostgreSQL)
- MediatR (CQRS)
- FluentValidation
- Carter (Minimal API endpoints)
- .NET MAUI (Android + Windows)
- CommunityToolkit.Mvvm
- Refit (HTTP client)
- xUnit + FluentAssertions

## Notlar

- Her aşamada kullanıcı onayı gerekir (approval gate)
- Kod compile olmadan sonraki aşamaya geçilmez
- Domain layer sıfır dış bağımlılık kuralı uygulanır
- Antigravity ücretsiz preview'da Gemini 3 Pro kullanır

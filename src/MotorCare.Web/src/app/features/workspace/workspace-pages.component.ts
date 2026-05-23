import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { friendlyError } from '../../core/api/error.util';
import { EntityRecord, PagedResult, TenantProfile } from '../../core/models/api.models';
import { dateText, field, money } from '../../shared/formatters';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page-heading">
      <div><h1>Müşteriler</h1><p>Müşteri arama, kayıt ve düzenleme</p></div>
      <a class="primary-button" routerLink="/customers/create">Yeni Müşteri</a>
    </section>
    <form class="toolbar" (ngSubmit)="load()">
      <input [formControl]="search" placeholder="Ad, telefon veya e-posta ile ara" />
      <button class="ghost-button">Ara</button>
    </form>
    <div class="state-card error" *ngIf="error">{{ error }}</div>
    <div class="panel">
      <a class="list-row clickable" *ngFor="let item of items" [routerLink]="['/customers', item['id']]">
        <div><strong>{{ item['fullName'] || item['name'] }}</strong><small>{{ item['phone'] || item['email'] || '-' }}</small></div>
        <span>{{ item['vehicleCount'] || '' }}</span>
      </a>
      <p class="muted" *ngIf="!loading && items.length === 0">Müşteri bulunamadı.</p>
    </div>
  `
})
export class CustomersComponent implements OnInit {
  loading = false;
  error = '';
  items: EntityRecord[] = [];
  readonly search = this.fb.nonNullable.control('');

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api.get<PagedResult<EntityRecord>>('/api/customers', { q: this.search.value, pageNumber: 1, pageSize: 30 }).subscribe({
      next: (page) => { this.items = page.items ?? []; this.loading = false; },
      error: (err) => { this.error = friendlyError(err, 'Müşteriler yüklenemedi.'); this.loading = false; }
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-heading"><div><h1>Yeni Müşteri</h1><p>Müşteri bilgilerini girin</p></div></section>
    <form class="form-grid panel" [formGroup]="form" (ngSubmit)="submit()">
      <label>Ad soyad<input formControlName="fullName" /></label>
      <label>Telefon<input formControlName="phone" /></label>
      <label>E-posta<input formControlName="email" type="email" /></label>
      <label>WhatsApp<input formControlName="whatsapp" /></label>
      <label class="wide">Not<textarea formControlName="notes"></textarea></label>
      <p class="error wide" *ngIf="error">{{ error }}</p>
      <button class="primary-button" [disabled]="form.invalid || loading">Kaydet</button>
    </form>
  `
})
export class CustomerFormComponent {
  loading = false;
  error = '';
  readonly form = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    phone: ['', Validators.required],
    email: [''],
    whatsapp: [''],
    notes: ['']
  });

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService, private readonly router: Router) {}

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.api.post<string>('/api/customers', this.form.getRawValue()).pipe(finalize(() => (this.loading = false))).subscribe({
      next: (id) => void this.router.navigate(['/customers', id]),
      error: (err) => (this.error = friendlyError(err, 'Müşteri kaydedilemedi.'))
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page-heading"><div><h1>{{ customer?.['fullName'] || 'Müşteri' }}</h1><p>{{ customer?.['phone'] || '-' }}</p></div></section>
    <div class="state-card error" *ngIf="error">{{ error }}</div>
    <form class="form-grid panel" [formGroup]="form" (ngSubmit)="save()" *ngIf="customer">
      <label>Ad soyad<input formControlName="fullName" /></label>
      <label>Telefon<input formControlName="phone" /></label>
      <label>E-posta<input formControlName="email" /></label>
      <label>WhatsApp<input formControlName="whatsapp" /></label>
      <label class="wide">Not<textarea formControlName="notes"></textarea></label>
      <button class="primary-button" [disabled]="form.invalid || loading">Güncelle</button>
      <a class="ghost-button" [routerLink]="['/vehicles/create']" [queryParams]="{ customerId: id }">Araç Ekle</a>
    </form>
  `
})
export class CustomerDetailComponent implements OnInit {
  readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  loading = false;
  error = '';
  customer: EntityRecord | null = null;
  readonly form = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    phone: ['', Validators.required],
    email: [''],
    whatsapp: [''],
    notes: ['']
  });

  constructor(private readonly fb: FormBuilder, private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.get<EntityRecord>(`/api/customers/${this.id}`).subscribe({
      next: (customer) => {
        this.customer = customer;
        this.form.patchValue({
          fullName: String(customer['fullName'] ?? ''),
          phone: String(customer['phone'] ?? ''),
          email: String(customer['email'] ?? ''),
          whatsapp: String(customer['whatsapp'] ?? ''),
          notes: String(customer['notes'] ?? '')
        });
      },
      error: (err) => (this.error = friendlyError(err, 'Müşteri bilgileri yüklenemedi.'))
    });
  }

  save(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.api.put(`/api/customers/${this.id}`, this.form.getRawValue()).pipe(finalize(() => (this.loading = false))).subscribe({
      next: () => (this.error = ''),
      error: (err) => (this.error = friendlyError(err, 'Müşteri güncellenemedi.'))
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page-heading">
      <div><h1>Araçlar</h1><p>Plaka ile araç kaydı ve servis geçmişi</p></div>
      <a class="primary-button" routerLink="/vehicles/create">Yeni Araç</a>
    </section>
    <form class="toolbar" (ngSubmit)="searchPlate()">
      <input [formControl]="plate" placeholder="Plaka ara" />
      <button class="ghost-button">Ara</button>
    </form>
    <div class="state-card error" *ngIf="error">{{ error }}</div>
    <div class="panel" *ngIf="vehicle">
      <div class="list-row">
        <div><strong>{{ vehicle['plate'] }}</strong><small>{{ vehicle['brand'] }} {{ vehicle['model'] }} · {{ vehicle['year'] }}</small></div>
        <a [routerLink]="['/vehicles', vehicle['id'], 'history']">Geçmiş</a>
      </div>
    </div>
  `
})
export class VehiclesComponent {
  error = '';
  vehicle: EntityRecord | null = null;
  readonly plate = this.fb.nonNullable.control('', Validators.required);

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {}

  searchPlate(): void {
    if (this.plate.invalid) return;
    this.api.get<EntityRecord>(`/api/vehicles/${encodeURIComponent(this.plate.value)}`).subscribe({
      next: (vehicle) => { this.vehicle = vehicle; this.error = ''; },
      error: (err) => { this.vehicle = null; this.error = friendlyError(err, 'Araç bulunamadı.'); }
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-heading"><div><h1>Yeni Araç</h1><p>Plaka ve motosiklet bilgileri</p></div></section>
    <form class="form-grid panel" [formGroup]="form" (ngSubmit)="submit()">
      <label>Plaka<input formControlName="plate" /></label>
      <label>Marka<input formControlName="brand" /></label>
      <label>Model<input formControlName="model" /></label>
      <label>Yıl<input formControlName="year" type="number" /></label>
      <label>KM<input formControlName="currentKm" type="number" /></label>
      <label>Müşteri ID<input formControlName="currentCustomerId" /></label>
      <label>Şasi no<input formControlName="chassisNumber" /></label>
      <label>Motor no<input formControlName="engineNumber" /></label>
      <label>Renk<input formControlName="color" /></label>
      <p class="error wide" *ngIf="error">{{ error }}</p>
      <button class="primary-button" [disabled]="form.invalid || loading">Kaydet</button>
    </form>
  `
})
export class VehicleFormComponent {
  loading = false;
  error = '';
  readonly form = this.fb.nonNullable.group({
    plate: ['', Validators.required],
    brand: ['', Validators.required],
    model: ['', Validators.required],
    year: [new Date().getFullYear(), Validators.required],
    currentKm: [0],
    currentCustomerId: [this.route.snapshot.queryParamMap.get('customerId') ?? ''],
    chassisNumber: [''],
    engineNumber: [''],
    color: ['']
  });

  constructor(private readonly fb: FormBuilder, private readonly route: ActivatedRoute, private readonly api: ApiService, private readonly router: Router) {}

  submit(): void {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    const body = { ...raw, currentKm: Number(raw.currentKm) || null, currentCustomerId: raw.currentCustomerId || null };
    this.loading = true;
    this.api.post<string>('/api/vehicles', body).pipe(finalize(() => (this.loading = false))).subscribe({
      next: (id) => void this.router.navigate(['/vehicles', id, 'history']),
      error: (err) => (this.error = friendlyError(err, 'Araç kaydedilemedi.'))
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="page-heading"><div><h1>Araç Servis Geçmişi</h1><p>{{ history?.['plate'] || id }}</p></div></section>
    <div class="state-card error" *ngIf="error">{{ error }}</div>
    <section class="panel">
      <div class="list-row" *ngFor="let item of orders">
        <div><strong>{{ item['serviceOrderNo'] || item['orderNumber'] || 'Servis' }}</strong><small>{{ dateText(item['openedAt'] || item['createdAt']) }}</small></div>
        <a [routerLink]="['/service-orders', item['id']]">Aç</a>
      </div>
      <p class="muted" *ngIf="orders.length === 0">Geçmiş kayıt yok.</p>
    </section>
  `
})
export class VehicleHistoryComponent implements OnInit {
  readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  error = '';
  history: EntityRecord | null = null;
  orders: EntityRecord[] = [];

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.get<EntityRecord>(`/api/vehicles/${this.id}/history`).subscribe({
      next: (history) => {
        this.history = history;
        this.orders = (history['serviceOrders'] as EntityRecord[]) ?? [];
      },
      error: (err) => (this.error = friendlyError(err, 'Araç geçmişi yüklenemedi.'))
    });
  }

  dateText(value: unknown): string { return dateText(value); }
}

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `<section class="page-heading"><div><h1>Yetki Yok</h1><p>Bu ekran için yetkiniz bulunmuyor.</p></div></section>`
})
export class ForbiddenComponent {}

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-heading"><div><h1>Randevular</h1><p>Randevu listesi</p></div></section>
    <div class="panel">
      <div class="list-row" *ngFor="let item of items"><strong>{{ item['customerName'] || item['title'] || 'Randevu' }}</strong><span>{{ dateText(item['appointmentDate'] || item['scheduledAt']) }}</span></div>
      <p class="muted" *ngIf="items.length === 0">Randevu bulunamadı.</p>
    </div>
  `
})
export class AppointmentsComponent implements OnInit {
  items: EntityRecord[] = [];
  constructor(private readonly api: ApiService) {}
  ngOnInit(): void {
    this.api.get<PagedResult<EntityRecord>>('/api/appointments', { pageNumber: 1, pageSize: 30 }).subscribe({ next: (p) => (this.items = p.items ?? []), error: () => (this.items = []) });
  }
  dateText(value: unknown): string { return dateText(value); }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-heading"><div><h1>Stok</h1><p>Parça ve ürün yönetimi</p></div></section>
    <form class="toolbar" (ngSubmit)="load()"><input [formControl]="q" placeholder="Stok ara" /><button class="ghost-button">Ara</button></form>
    <div class="panel">
      <div class="list-row" *ngFor="let item of items"><div><strong>{{ item['name'] || item['partName'] }}</strong><small>{{ item['sku'] || item['category'] || '-' }}</small></div><span>{{ item['quantityOnHand'] ?? item['stock'] ?? '-' }}</span></div>
    </div>
  `
})
export class InventoryComponent implements OnInit {
  items: EntityRecord[] = [];
  readonly q = this.fb.nonNullable.control('');
  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {}
  ngOnInit(): void { this.load(); }
  load(): void {
    this.api.get<PagedResult<EntityRecord>>('/api/inventory', { q: this.q.value, pageNumber: 1, pageSize: 30 }).subscribe({ next: (p) => (this.items = p.items ?? []), error: () => (this.items = []) });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-heading"><div><h1>Hizmet Kataloğu</h1><p>Tanımlı hizmetler ve fiyatlar</p></div></section>
    <div class="panel">
      <div class="list-row" *ngFor="let item of items"><div><strong>{{ item['name'] || item['description'] }}</strong><small>{{ item['category'] || '-' }}</small></div><span>{{ money(item['price']) }}</span></div>
    </div>
  `
})
export class GenericCatalogComponent implements OnInit {
  items: EntityRecord[] = [];
  constructor(private readonly api: ApiService) {}
  ngOnInit(): void {
    this.api.get<PagedResult<EntityRecord>>('/api/services', { pageNumber: 1, pageSize: 50 }).subscribe({ next: (p) => (this.items = p.items ?? []), error: () => (this.items = []) });
  }
  money(value: unknown): string { return money(value); }
}

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="page-heading"><div><h1>Ayarlar</h1><p>İşletme ve kullanıcı yönetimi</p></div></section>
    <div class="card-grid">
      <a class="panel clickable" routerLink="/settings/business"><h2>İşletme Bilgileri</h2><p>Firma, vergi, iletişim ve logo bilgileri</p></a>
      <a class="panel clickable" routerLink="/settings/users"><h2>Kullanıcılar</h2><p>Davet ve rol yönetimi</p></a>
    </div>
  `
})
export class SettingsComponent {}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-heading"><div><h1>İşletme Bilgileri</h1><p>Profil bilgileri</p></div></section>
    <form class="form-grid panel" [formGroup]="form" (ngSubmit)="save()">
      <label>Firma adı<input formControlName="name" /></label>
      <label>Legal name<input formControlName="legalName" /></label>
      <label>Vergi no<input formControlName="taxNumber" /></label>
      <label>Vergi dairesi<input formControlName="taxOffice" /></label>
      <label>Telefon<input formControlName="phone" /></label>
      <label>E-posta<input formControlName="email" /></label>
      <label>Website<input formControlName="website" /></label>
      <label>Logo URL<input formControlName="logoUrl" /></label>
      <label class="wide">Adres<textarea formControlName="address"></textarea></label>
      <p class="error wide" *ngIf="error">{{ error }}</p>
      <p class="success wide" *ngIf="message">{{ message }}</p>
      <button class="primary-button" [disabled]="form.invalid || loading">Kaydet</button>
    </form>
  `
})
export class SettingsBusinessComponent implements OnInit {
  loading = false;
  error = '';
  message = '';
  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    legalName: [''],
    taxNumber: [''],
    taxOffice: [''],
    address: [''],
    phone: [''],
    email: [''],
    website: [''],
    logoUrl: ['']
  });

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.get<TenantProfile>('/api/tenants/current/profile').subscribe({ next: (profile) => this.form.patchValue(profile), error: (err) => (this.error = friendlyError(err, 'Profil yüklenemedi.')) });
  }

  save(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.api.put<TenantProfile>('/api/tenants/current/profile', this.form.getRawValue()).pipe(finalize(() => (this.loading = false))).subscribe({
      next: () => (this.message = 'İşletme bilgileri güncellendi.'),
      error: (err) => (this.error = friendlyError(err, 'İşletme bilgileri kaydedilemedi.'))
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-heading"><div><h1>Kullanıcı Yönetimi</h1><p>Davet ve rol yönetimi</p></div></section>
    <form class="form-grid panel" [formGroup]="form" (ngSubmit)="invite()">
      <label>Ad soyad<input formControlName="fullName" /></label>
      <label>E-posta<input formControlName="email" /></label>
      <label>Rol<select formControlName="role"><option *ngFor="let role of roles" [value]="role">{{ role }}</option></select></label>
      <p class="error wide" *ngIf="error">{{ error }}</p>
      <p class="success wide" *ngIf="message">{{ message }}</p>
      <button class="primary-button" [disabled]="form.invalid || loading">Davet Gönder</button>
    </form>
    <section class="panel">
      <div class="list-row" *ngFor="let user of users"><div><strong>{{ user['fullName'] || user['email'] }}</strong><small>{{ user['email'] }}</small></div><span>{{ user['role'] }}</span></div>
    </section>
  `
})
export class SettingsUsersComponent implements OnInit {
  readonly roles = ['Owner', 'Admin', 'Manager', 'Technician', 'Inspector', 'Accountant', 'ReadOnly'];
  users: EntityRecord[] = [];
  loading = false;
  error = '';
  message = '';
  readonly form = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    role: ['Technician', Validators.required]
  });

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.api.get<EntityRecord[]>('/api/users').subscribe({ next: (users) => (this.users = users ?? []), error: () => (this.users = []) });
  }

  invite(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.api.post('/api/users/invite', this.form.getRawValue()).pipe(finalize(() => (this.loading = false))).subscribe({
      next: () => { this.message = 'Davet e-postası gönderildi.'; this.form.reset({ role: 'Technician', fullName: '', email: '' }); this.load(); },
      error: (err) => (this.error = friendlyError(err, 'Davet gönderilemedi.'))
    });
  }
}

export { field };

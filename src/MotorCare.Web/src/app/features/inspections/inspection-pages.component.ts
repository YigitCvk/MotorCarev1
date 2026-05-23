import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { friendlyError } from '../../core/api/error.util';
import { EntityRecord, PagedResult } from '../../core/models/api.models';
import { dateText } from '../../shared/formatters';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page-heading">
      <div><h1>Expertiz</h1><p>Kontrol raporları ve public QR paylaşımları</p></div>
      <a class="primary-button" routerLink="/inspections/create">Yeni Expertiz</a>
    </section>

    <form class="toolbar" (ngSubmit)="load()">
      <input [formControl]="q" placeholder="Plaka, müşteri veya rapor no ara" />
      <button class="ghost-button" type="submit">Ara</button>
    </form>

    <div class="state-card error" *ngIf="error">{{ error }}</div>
    <section class="panel">
      <p class="muted" *ngIf="loading">Expertiz kayıtları yükleniyor...</p>
      <a class="list-row clickable" *ngFor="let item of items" [routerLink]="['/inspections', item['id']]">
        <div>
          <strong>{{ item['inspectionNo'] || item['reportNo'] || item['vehiclePlate'] || item['plate'] || 'Expertiz' }}</strong>
          <small>{{ item['customerName'] || '-' }} · {{ item['vehicleDisplay'] || item['plate'] || '-' }}</small>
        </div>
        <span>{{ item['statusText'] || item['status'] || dateText(item['createdAt']) }}</span>
      </a>
      <p class="muted" *ngIf="!loading && items.length === 0">Expertiz kaydı bulunamadı.</p>
    </section>
  `
})
export class InspectionsComponent implements OnInit {
  loading = false;
  error = '';
  items: EntityRecord[] = [];
  readonly q = this.fb.nonNullable.control('');

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api
      .get<PagedResult<EntityRecord>>('/api/inspections', {
        q: this.q.value,
        pageNumber: 1,
        pageSize: 30
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (page) => (this.items = page.items ?? []),
        error: (err) => (this.error = friendlyError(err, 'Expertiz kayıtları yüklenemedi.'))
      });
  }

  dateText(value: unknown): string {
    return dateText(value);
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-heading">
      <div><h1>Yeni Expertiz</h1><p>Araç ve müşteri bilgisiyle yeni rapor oluşturun</p></div>
    </section>

    <form class="form-grid panel" [formGroup]="form" (ngSubmit)="submit()">
      <label>Müşteri adı<input formControlName="customerName" /></label>
      <label>Telefon<input formControlName="phone" /></label>
      <label>Plaka<input formControlName="plate" /></label>
      <label>Marka<input formControlName="brand" /></label>
      <label>Model<input formControlName="model" /></label>
      <label>Yıl<input formControlName="year" type="number" /></label>
      <label>KM<input formControlName="mileage" type="number" /></label>
      <label>Paket
        <select formControlName="packageType">
          <option [ngValue]="1">Standart</option>
          <option [ngValue]="2">Detaylı</option>
          <option [ngValue]="3">Premium</option>
          <option [ngValue]="4">Ekspertiz</option>
        </select>
      </label>
      <label class="wide">5664 sorgusu<textarea formControlName="query5664"></textarea></label>
      <label class="wide">KM sorgusu<textarea formControlName="mileageQuery"></textarea></label>
      <label class="wide">Genel notlar<textarea formControlName="generalNotes"></textarea></label>
      <label class="wide">Test sürüşü notları<textarea formControlName="testRideNotes"></textarea></label>
      <label class="wide">Kozmetik notlar<textarea formControlName="cosmeticNotes"></textarea></label>
      <p class="error wide" *ngIf="error">{{ error }}</p>
      <button class="primary-button" [disabled]="form.invalid || loading">Expertiz Oluştur</button>
    </form>
  `
})
export class InspectionFormComponent {
  loading = false;
  error = '';
  readonly form = this.fb.nonNullable.group({
    customerName: ['', Validators.required],
    phone: ['', Validators.required],
    plate: ['', Validators.required],
    brand: [''],
    model: [''],
    year: [new Date().getFullYear()],
    mileage: [0, [Validators.required, Validators.min(0)]],
    chassisNumber: [''],
    engineNumber: [''],
    query5664: [''],
    mileageQuery: [''],
    packageType: [4, Validators.required],
    generalNotes: [''],
    testRideNotes: [''],
    cosmeticNotes: ['']
  });

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService, private readonly router: Router) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const raw = this.form.getRawValue();
    this.api
      .post<{ id: string }>('/api/inspections', {
        ...raw,
        customerId: null,
        vehicleId: null,
        year: Number(raw.year) || null,
        mileage: Number(raw.mileage) || null,
        packageType: Number(raw.packageType)
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (response) => void this.router.navigate(['/inspections', response.id]),
        error: (err) => (this.error = friendlyError(err, 'Expertiz oluşturulamadı.'))
      });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page-heading">
      <div>
        <h1>{{ inspection?.['inspectionNo'] || inspection?.['reportNo'] || 'Expertiz Detayı' }}</h1>
        <p>{{ inspection?.['customerName'] || '-' }} · {{ inspection?.['vehicleDisplay'] || inspection?.['plate'] || '-' }}</p>
      </div>
      <div class="action-row">
        <a class="ghost-button" [routerLink]="['/inspections', id, 'print']">Yazdır</a>
        <button class="primary-button" type="button" (click)="complete()">Tamamla</button>
      </div>
    </section>

    <div class="state-card error" *ngIf="error">{{ error }}</div>
    <section class="panel">
      <h2>Kontrol Kalemleri</h2>
      <p class="muted" *ngIf="loading">Expertiz raporu yükleniyor...</p>
      <div class="list-row" *ngFor="let item of items">
        <div>
          <strong>{{ item['name'] || item['title'] || item['category'] }}</strong>
          <small>{{ item['notes'] || item['description'] || '-' }}</small>
        </div>
        <span>{{ item['resultText'] || item['result'] || '-' }}</span>
      </div>
      <p class="muted" *ngIf="!loading && items.length === 0">Bu raporda henüz kontrol kalemi yok.</p>
    </section>

    <section class="panel">
      <h2>Sonuç ve Notlar</h2>
      <p><strong>Sonuç:</strong> {{ inspection?.['resultText'] || inspection?.['result'] || '-' }}</p>
      <p>{{ inspection?.['notes'] || inspection?.['summary'] || 'Not girilmemiş.' }}</p>
      <p class="muted" *ngIf="inspection?.['publicUrl'] || inspection?.['publicSlug']">
        QR public link: {{ inspection?.['publicUrl'] || ('/public/inspection-report/' + inspection?.['publicSlug']) }}
      </p>
    </section>
  `
})
export class InspectionDetailComponent implements OnInit {
  readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  inspection: EntityRecord | null = null;
  loading = false;
  error = '';

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  get items(): EntityRecord[] {
    return (this.inspection?.['items'] as EntityRecord[]) ?? (this.inspection?.['checkItems'] as EntityRecord[]) ?? [];
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api
      .get<EntityRecord>(`/api/inspections/${this.id}`)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (inspection) => (this.inspection = inspection),
        error: (err) => (this.error = friendlyError(err, 'Expertiz detayı yüklenemedi.'))
      });
  }

  complete(): void {
    this.api.put(`/api/inspections/${this.id}/complete`, {}).subscribe({
      next: () => this.load(),
      error: (err) => (this.error = friendlyError(err, 'Expertiz tamamlanamadı.'))
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="print-page" *ngIf="inspection">
      <button class="ghost-button no-print" type="button" (click)="print()">Yazdır</button>
      <h1>Expertiz Raporu {{ inspection['inspectionNo'] || inspection['reportNo'] || '' }}</h1>
      <p>{{ inspection['vehicleDisplay'] || inspection['plate'] }} · {{ inspection['createdAt'] ? dateText(inspection['createdAt']) : '' }}</p>

      <svg class="vehicle-diagram" viewBox="0 0 420 180" role="img" aria-label="Araç diyagramı">
        <rect x="70" y="35" width="280" height="105" rx="28" fill="#f8fafc" stroke="#0f172a" stroke-width="3" />
        <rect x="125" y="20" width="170" height="70" rx="24" fill="#e0f2fe" stroke="#0f172a" stroke-width="3" />
        <circle cx="125" cy="145" r="21" fill="#111827" />
        <circle cx="295" cy="145" r="21" fill="#111827" />
        <path d="M92 85h238M180 22v112M250 22v112" stroke="#475569" stroke-width="2" stroke-dasharray="6 6" />
      </svg>

      <table>
        <thead><tr><th>Kontrol</th><th>Sonuç</th><th>Not</th></tr></thead>
        <tbody>
          <tr *ngFor="let item of items">
            <td>{{ item['name'] || item['title'] || item['category'] }}</td>
            <td>{{ item['resultText'] || item['result'] || '-' }}</td>
            <td>{{ item['notes'] || '-' }}</td>
          </tr>
        </tbody>
      </table>
      <h2>Genel Sonuç: {{ inspection['resultText'] || inspection['result'] || '-' }}</h2>
    </section>
  `
})
export class InspectionPrintComponent implements OnInit {
  readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  inspection: EntityRecord | null = null;

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.get<EntityRecord>(`/api/inspections/${this.id}`).subscribe({
      next: (inspection) => (this.inspection = inspection)
    });
  }

  get items(): EntityRecord[] {
    return (this.inspection?.['items'] as EntityRecord[]) ?? (this.inspection?.['checkItems'] as EntityRecord[]) ?? [];
  }

  dateText(value: unknown): string {
    return dateText(value);
  }

  print(): void {
    window.print();
  }
}

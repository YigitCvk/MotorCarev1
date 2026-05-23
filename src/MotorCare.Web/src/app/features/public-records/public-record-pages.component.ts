import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { EntityRecord } from '../../core/models/api.models';
import { dateText, maskPublic, money } from '../../shared/formatters';

const publicError = 'Bu paylaşım bağlantısı geçersiz veya artık kullanılamıyor.';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <main class="public-page">
      <section class="public-header">
        <span>BakımSuite Public QR</span>
        <h1>Servis Kaydı</h1>
        <p>Bu sayfa yalnızca paylaşılması güvenli servis özetini gösterir.</p>
      </section>

      <div class="state-card error" *ngIf="error">{{ error }}</div>
      <section class="panel" *ngIf="record">
        <h2>{{ record['serviceOrderNo'] || record['orderNo'] || 'Servis Özeti' }}</h2>
        <div class="detail-grid">
          <div><span>Plaka</span><strong>{{ record['plateMasked'] || mask(record['plate']) }}</strong></div>
          <div><span>Araç</span><strong>{{ record['vehicleDisplay'] || '-' }}</strong></div>
          <div><span>Durum</span><strong>{{ record['statusText'] || record['status'] || '-' }}</strong></div>
          <div><span>Tarih</span><strong>{{ dateText(record['openedAt'] || record['createdAt']) }}</strong></div>
        </div>
        <h3>İşlemler</h3>
        <div class="list-row" *ngFor="let row of rows">
          <strong>{{ row['name'] || row['description'] || row['partName'] || row['productName'] }}</strong>
          <span>{{ money(row['lineTotal'] || row['totalPrice'] || row['price']) }}</span>
        </div>
        <p class="muted" *ngIf="rows.length === 0">Paylaşılabilir işlem özeti bulunmuyor.</p>
      </section>
    </main>
  `
})
export class PublicServiceRecordComponent implements OnInit {
  readonly slug = this.route.snapshot.paramMap.get('slug') ?? '';
  record: EntityRecord | null = null;
  error = '';
  loading = false;

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loading = true;
    this.api
      .get<EntityRecord>(`/api/public/service-record/${this.slug}`)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (record) => (this.record = sanitizePublicRecord(record)),
        error: () => (this.error = publicError)
      });
  }

  get rows(): EntityRecord[] {
    const operations = (this.record?.['operations'] as EntityRecord[]) ?? [];
    const parts = (this.record?.['parts'] as EntityRecord[]) ?? [];
    const consumables = (this.record?.['consumables'] as EntityRecord[]) ?? [];
    return [...operations, ...parts, ...consumables];
  }

  mask(value: unknown): string {
    return maskPublic(value);
  }

  dateText(value: unknown): string {
    return dateText(value);
  }

  money(value: unknown): string {
    return money(value);
  }
}

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <main class="public-page">
      <section class="public-header">
        <span>BakımSuite Public QR</span>
        <h1>Expertiz Raporu</h1>
        <p>Bu sayfa kimlik ve iletişim bilgilerini maskeleyerek rapor özetini gösterir.</p>
      </section>

      <div class="state-card error" *ngIf="error">{{ error }}</div>
      <section class="panel" *ngIf="report">
        <h2>{{ report['inspectionNo'] || report['reportNo'] || 'Public Rapor' }}</h2>
        <div class="detail-grid">
          <div><span>Plaka</span><strong>{{ report['plateMasked'] || mask(report['plate']) }}</strong></div>
          <div><span>Araç</span><strong>{{ report['vehicleDisplay'] || '-' }}</strong></div>
          <div><span>Sonuç</span><strong>{{ report['resultText'] || report['result'] || '-' }}</strong></div>
          <div><span>Tarih</span><strong>{{ dateText(report['createdAt'] || report['completedAt']) }}</strong></div>
        </div>

        <svg class="vehicle-diagram" viewBox="0 0 420 180" role="img" aria-label="Araç diyagramı">
          <rect x="70" y="35" width="280" height="105" rx="28" fill="#f8fafc" stroke="#0f172a" stroke-width="3" />
          <rect x="125" y="20" width="170" height="70" rx="24" fill="#e0f2fe" stroke="#0f172a" stroke-width="3" />
          <circle cx="125" cy="145" r="21" fill="#111827" />
          <circle cx="295" cy="145" r="21" fill="#111827" />
          <path d="M92 85h238M180 22v112M250 22v112" stroke="#475569" stroke-width="2" stroke-dasharray="6 6" />
        </svg>

        <h3>Kontrol Kalemleri</h3>
        <div class="list-row" *ngFor="let item of items">
          <strong>{{ item['name'] || item['title'] || item['category'] }}</strong>
          <span>{{ item['resultText'] || item['result'] || '-' }}</span>
        </div>
        <p class="muted" *ngIf="items.length === 0">Paylaşılabilir kontrol kalemi bulunmuyor.</p>
      </section>
    </main>
  `
})
export class PublicInspectionReportComponent implements OnInit {
  readonly slug = this.route.snapshot.paramMap.get('slug') ?? '';
  report: EntityRecord | null = null;
  error = '';

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.get<EntityRecord>(`/api/public/inspection-report/${this.slug}`).subscribe({
      next: (report) => (this.report = sanitizePublicRecord(report)),
      error: () => (this.error = publicError)
    });
  }

  get items(): EntityRecord[] {
    return (this.report?.['items'] as EntityRecord[]) ?? (this.report?.['checkItems'] as EntityRecord[]) ?? [];
  }

  mask(value: unknown): string {
    return maskPublic(value);
  }

  dateText(value: unknown): string {
    return dateText(value);
  }
}

function sanitizePublicRecord(record: EntityRecord): EntityRecord {
  const copy: EntityRecord = { ...record };
  for (const key of ['tenantId', 'userId', 'customerId', 'ownerId', 'email', 'phone', 'address', 'taxNumber', 'taxOffice']) {
    delete copy[key];
  }
  return copy;
}

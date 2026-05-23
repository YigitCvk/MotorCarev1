import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { friendlyError } from '../../core/api/error.util';
import { EntityRecord, PagedResult } from '../../core/models/api.models';
import { dateText, money } from '../../shared/formatters';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page-heading">
      <div><h1>Servis Kayıtları</h1><p>Servis emirleri ve tahsilat durumu</p></div>
      <a class="primary-button" routerLink="/service-orders/create">Yeni Servis</a>
    </section>
    <form class="toolbar" (ngSubmit)="load()">
      <input [formControl]="q" placeholder="Plaka, müşteri veya servis no ara" />
      <select [formControl]="status">
        <option value="">Tüm durumlar</option>
        <option value="Open">Açık</option>
        <option value="InProgress">İşlemde</option>
        <option value="Completed">Tamamlandı</option>
        <option value="Delivered">Teslim edildi</option>
      </select>
      <button class="ghost-button">Ara</button>
    </form>
    <div class="state-card error" *ngIf="error">{{ error }}</div>
    <section class="panel">
      <a class="list-row clickable" *ngFor="let item of items" [routerLink]="['/service-orders', item['id']]">
        <div>
          <strong>{{ item['serviceOrderNo'] || item['orderNumber'] || item['plate'] }}</strong>
          <small>{{ item['customerName'] || '-' }} · {{ item['plate'] || '-' }}</small>
        </div>
        <span>{{ item['statusText'] || item['status'] || '-' }}</span>
      </a>
      <p class="muted" *ngIf="!loading && items.length === 0">Servis kaydı bulunamadı.</p>
    </section>
  `
})
export class ServiceOrdersComponent implements OnInit {
  loading = false;
  error = '';
  items: EntityRecord[] = [];
  readonly q = this.fb.nonNullable.control('');
  readonly status = this.fb.nonNullable.control('');

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api.get<PagedResult<EntityRecord>>('/api/service-orders', { q: this.q.value, status: this.status.value, pageNumber: 1, pageSize: 30 }).subscribe({
      next: (page) => { this.items = page.items ?? []; this.loading = false; },
      error: (err) => { this.error = friendlyError(err, 'Servis kayıtları yüklenemedi.'); this.loading = false; }
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page-heading"><div><h1>Yeni Servis</h1><p>Müşteri ve araç seçerek servis emri açın</p></div></section>
    <form class="form-grid panel" [formGroup]="form" (ngSubmit)="submit()">
      <label class="wide">Müşteri arama<input [formControl]="customerSearch" (input)="searchCustomers()" placeholder="Müşteri adı veya telefon" /></label>
      <label>Müşteri
        <select formControlName="customerId" (change)="loadVehicles()">
          <option value="">Seçin</option>
          <option *ngFor="let c of customers" [value]="c['id']">{{ c['fullName'] || c['name'] }} - {{ c['phone'] }}</option>
        </select>
      </label>
      <label>Araç
        <select formControlName="vehicleId">
          <option value="">Seçin</option>
          <option *ngFor="let v of vehicles" [value]="v['id']">{{ v['plate'] }} - {{ v['brand'] }} {{ v['model'] }}</option>
        </select>
      </label>
      <label>KM<input formControlName="vehicleKm" type="number" /></label>
      <label class="wide">Şikayet / açıklama<textarea formControlName="complaint"></textarea></label>
      <p class="error wide" *ngIf="error">{{ error }}</p>
      <button class="primary-button" [disabled]="form.invalid || loading">Servis Aç</button>
    </form>
  `
})
export class ServiceOrderCreateComponent {
  loading = false;
  error = '';
  customers: EntityRecord[] = [];
  vehicles: EntityRecord[] = [];
  readonly customerSearch = this.fb.nonNullable.control('');
  readonly form = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    vehicleId: ['', Validators.required],
    vehicleKm: [0, [Validators.required, Validators.min(0)]],
    complaint: ['']
  });

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService, private readonly router: Router) {}

  searchCustomers(): void {
    this.api.get<PagedResult<EntityRecord>>('/api/customers', { q: this.customerSearch.value, pageNumber: 1, pageSize: 20 }).subscribe({
      next: (page) => (this.customers = page.items ?? []),
      error: () => (this.customers = [])
    });
  }

  loadVehicles(): void {
    const customerId = this.form.controls.customerId.value;
    if (!customerId) return;
    this.api.get<EntityRecord[]>(`/api/customers/${customerId}/vehicles`).subscribe({
      next: (vehicles) => (this.vehicles = vehicles ?? []),
      error: () => (this.vehicles = [])
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    this.loading = true;
    this.api.post<string>('/api/service-orders', { ...raw, vehicleKm: Number(raw.vehicleKm), consumables: [] }).pipe(finalize(() => (this.loading = false))).subscribe({
      next: (id) => void this.router.navigate(['/service-orders', id]),
      error: (err) => (this.error = friendlyError(err, 'Servis kaydı oluşturulamadı.'))
    });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page-heading">
      <div><h1>{{ order?.['serviceOrderNo'] || order?.['orderNumber'] || 'Servis Detayı' }}</h1><p>{{ order?.['customerName'] || '-' }} · {{ order?.['plate'] || '-' }}</p></div>
      <div class="action-row">
        <a class="ghost-button" [routerLink]="['/service-orders', id, 'print']">Yazdır</a>
        <button class="primary-button" type="button" (click)="complete()">Tamamla</button>
      </div>
    </section>
    <div class="state-card error" *ngIf="error">{{ error }}</div>

    <div class="metric-grid">
      <article class="metric-card"><span>İşçilik</span><strong>{{ money(operationTotal) }}</strong></article>
      <article class="metric-card"><span>Parça</span><strong>{{ money(partTotal) }}</strong></article>
      <article class="metric-card"><span>Sarf</span><strong>{{ money(consumableTotal) }}</strong></article>
      <article class="metric-card"><span>Genel Toplam</span><strong>{{ money(grandTotal) }}</strong></article>
      <article class="metric-card"><span>Ödenen</span><strong>{{ money(paidTotal) }}</strong></article>
      <article class="metric-card"><span>Kalan</span><strong>{{ money(grandTotal - paidTotal) }}</strong></article>
    </div>

    <div class="content-grid">
      <section class="panel">
        <h2>İşçilik / Operasyon</h2>
        <form class="inline-form" [formGroup]="operationForm" (ngSubmit)="addOperation()">
          <input formControlName="description" placeholder="İşçilik adı" />
          <input formControlName="quantity" type="number" step="0.01" placeholder="Adet" />
          <input formControlName="unitPrice" type="number" step="0.01" placeholder="Birim fiyat" />
          <input formControlName="discount" type="number" step="0.01" placeholder="İndirim" />
          <input formControlName="notes" placeholder="Not" />
          <button class="primary-button">Ekle</button>
        </form>
        <div class="list-row" *ngFor="let item of operations">
          <div><strong>{{ item['description'] }}</strong><small>{{ item['quantity'] }} x {{ money(item['unitPrice']) }} · indirim {{ money(item['discount']) }}</small></div>
          <button class="link-button" type="button" (click)="remove('operations', item['id'])">Sil</button>
        </div>
      </section>

      <section class="panel">
        <h2>Parça</h2>
        <form class="inline-form" [formGroup]="partForm" (ngSubmit)="addPart()">
          <input formControlName="partName" placeholder="Parça adı" />
          <input formControlName="partNumber" placeholder="Parça no" />
          <input formControlName="quantity" type="number" placeholder="Adet" />
          <input formControlName="unitPrice" type="number" step="0.01" placeholder="Birim fiyat" />
          <button class="primary-button">Ekle</button>
        </form>
        <div class="list-row" *ngFor="let item of parts">
          <div><strong>{{ item['partName'] }}</strong><small>{{ item['quantity'] }} x {{ money(item['unitPrice']) }}</small></div>
          <button class="link-button" type="button" (click)="remove('parts', item['id'])">Sil</button>
        </div>
      </section>

      <section class="panel">
        <h2>Sarf</h2>
        <form class="inline-form" [formGroup]="consumableForm" (ngSubmit)="addConsumable()">
          <input formControlName="category" placeholder="Kategori" />
          <input formControlName="productName" placeholder="Ürün" />
          <input formControlName="quantity" type="number" placeholder="Adet" />
          <input formControlName="unitPrice" type="number" step="0.01" placeholder="Birim fiyat" />
          <button class="primary-button">Ekle</button>
        </form>
        <div class="list-row" *ngFor="let item of consumables">
          <div><strong>{{ item['productName'] }}</strong><small>{{ item['quantity'] }} x {{ money(item['unitPrice']) }}</small></div>
          <button class="link-button" type="button" (click)="remove('consumables', item['id'])">Sil</button>
        </div>
      </section>

      <section class="panel">
        <h2>Ödeme</h2>
        <form class="inline-form" [formGroup]="paymentForm" (ngSubmit)="addPayment()">
          <input formControlName="amount" type="number" step="0.01" placeholder="Tutar" />
          <select formControlName="method"><option value="Cash">Nakit</option><option value="CreditCard">Kart</option><option value="BankTransfer">Havale</option></select>
          <button class="primary-button">Ödeme Ekle</button>
        </form>
        <div class="list-row" *ngFor="let item of payments"><strong>{{ money(item['amount']) }}</strong><span>{{ item['method'] }} · {{ dateText(item['paymentDate']) }}</span></div>
      </section>
    </div>
  `
})
export class ServiceOrderDetailComponent implements OnInit {
  readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  order: EntityRecord | null = null;
  error = '';
  readonly operationForm = this.fb.nonNullable.group({ description: ['', Validators.required], quantity: [1, Validators.min(0.01)], unitPrice: [0, Validators.min(0)], discount: [0, Validators.min(0)], notes: [''] });
  readonly partForm = this.fb.nonNullable.group({ partName: ['', Validators.required], partNumber: [''], quantity: [1, Validators.min(1)], unitPrice: [0, Validators.min(0)] });
  readonly consumableForm = this.fb.nonNullable.group({ category: ['Genel', Validators.required], productName: ['', Validators.required], quantity: [1, Validators.min(1)], unitPrice: [0, Validators.min(0)] });
  readonly paymentForm = this.fb.nonNullable.group({ amount: [0, Validators.min(0.01)], method: ['Cash'] });

  constructor(private readonly fb: FormBuilder, private readonly route: ActivatedRoute, private readonly api: ApiService) {}

  ngOnInit(): void { this.load(); }

  get operations(): EntityRecord[] { return (this.order?.['operations'] as EntityRecord[]) ?? []; }
  get parts(): EntityRecord[] { return (this.order?.['parts'] as EntityRecord[]) ?? []; }
  get consumables(): EntityRecord[] { return (this.order?.['consumables'] as EntityRecord[]) ?? []; }
  get payments(): EntityRecord[] { return (this.order?.['payments'] as EntityRecord[]) ?? []; }
  get operationTotal(): number { return this.sum(this.operations, ['lineTotal', 'price']); }
  get partTotal(): number { return this.sum(this.parts, ['totalPrice', 'lineTotal']); }
  get consumableTotal(): number { return this.sum(this.consumables, ['lineTotal']); }
  get grandTotal(): number { return Number(this.order?.['grandTotal'] ?? this.order?.['totalAmount'] ?? this.operationTotal + this.partTotal + this.consumableTotal); }
  get paidTotal(): number { return this.sum(this.payments, ['amount']); }

  load(): void {
    this.api.get<EntityRecord>(`/api/service-orders/${this.id}`).subscribe({
      next: (order) => { this.order = order; this.error = ''; },
      error: (err) => (this.error = friendlyError(err, 'Servis detayı yüklenemedi.'))
    });
  }

  addOperation(): void {
    if (this.operationForm.invalid || this.invalidDiscount(this.operationForm.value.quantity, this.operationForm.value.unitPrice, this.operationForm.value.discount)) {
      this.error = 'İndirim satır toplamından büyük olamaz. Miktar ve fiyat değerlerini kontrol edin.';
      return;
    }
    this.api.post(`/api/service-orders/${this.id}/operations`, { serviceCatalogItemId: null, ...this.operationForm.getRawValue() }).subscribe({ next: () => { this.operationForm.reset({ description: '', quantity: 1, unitPrice: 0, discount: 0, notes: '' }); this.load(); }, error: (err) => (this.error = friendlyError(err, 'İşçilik eklenemedi.')) });
  }

  addPart(): void {
    if (this.partForm.invalid) return;
    this.api.post(`/api/service-orders/${this.id}/parts`, this.partForm.getRawValue()).subscribe({ next: () => { this.partForm.reset({ partName: '', partNumber: '', quantity: 1, unitPrice: 0 }); this.load(); }, error: (err) => (this.error = friendlyError(err, 'Parça eklenemedi.')) });
  }

  addConsumable(): void {
    if (this.consumableForm.invalid) return;
    this.api.post(`/api/service-orders/${this.id}/consumables`, this.consumableForm.getRawValue()).subscribe({ next: () => { this.consumableForm.reset({ category: 'Genel', productName: '', quantity: 1, unitPrice: 0 }); this.load(); }, error: (err) => (this.error = friendlyError(err, 'Sarf eklenemedi.')) });
  }

  addPayment(): void {
    if (this.paymentForm.invalid) return;
    this.api.post(`/api/service-orders/${this.id}/payments`, { ...this.paymentForm.getRawValue(), paymentDate: new Date().toISOString() }).subscribe({ next: () => { this.paymentForm.reset({ amount: 0, method: 'Cash' }); this.load(); }, error: (err) => (this.error = friendlyError(err, 'Ödeme eklenemedi.')) });
  }

  remove(type: 'operations' | 'parts' | 'consumables', id: unknown): void {
    if (!id) return;
    this.api.delete(`/api/service-orders/${this.id}/${type}/${id}`).subscribe({ next: () => this.load(), error: (err) => (this.error = friendlyError(err, 'Kayıt silinemedi.')) });
  }

  complete(): void {
    this.api.put(`/api/service-orders/${this.id}/status`, { status: 4, note: 'Angular MVP üzerinden tamamlandı.' }).subscribe({ next: () => this.load(), error: (err) => (this.error = friendlyError(err, 'Servis tamamlanamadı.')) });
  }

  money(value: unknown): string { return money(value); }
  dateText(value: unknown): string { return dateText(value); }
  private sum(items: EntityRecord[], keys: string[]): number { return items.reduce((total, item) => total + Number(keys.map((k) => item[k]).find((v) => v !== undefined) ?? 0), 0); }
  private invalidDiscount(quantity: unknown, unitPrice: unknown, discount: unknown): boolean { return Number(discount ?? 0) > Number(quantity ?? 0) * Number(unitPrice ?? 0); }
}

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="print-page" *ngIf="order">
      <button class="ghost-button no-print" type="button" (click)="print()">Yazdır</button>
      <h1>Servis Formu {{ order['serviceOrderNo'] || order['orderNumber'] }}</h1>
      <p>{{ order['customerName'] }} · {{ order['plate'] }}</p>
      <table>
        <thead><tr><th>Kalem</th><th>Adet</th><th>Birim</th><th>Toplam</th></tr></thead>
        <tbody>
          <tr *ngFor="let item of rows"><td>{{ item.name }}</td><td>{{ item.quantity }}</td><td>{{ money(item.unitPrice) }}</td><td>{{ money(item.total) }}</td></tr>
        </tbody>
      </table>
      <h2>Genel Toplam: {{ money(total) }}</h2>
    </section>
  `
})
export class ServiceOrderPrintComponent implements OnInit {
  readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  order: EntityRecord | null = null;
  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {}
  ngOnInit(): void { this.api.get<EntityRecord>(`/api/service-orders/${this.id}`).subscribe({ next: (order) => (this.order = order) }); }
  get rows(): { name: string; quantity: unknown; unitPrice: unknown; total: unknown }[] {
    const operations = ((this.order?.['operations'] as EntityRecord[]) ?? []).map((x) => ({ name: String(x['description']), quantity: x['quantity'], unitPrice: x['unitPrice'], total: x['lineTotal'] ?? x['price'] }));
    const parts = ((this.order?.['parts'] as EntityRecord[]) ?? []).map((x) => ({ name: String(x['partName']), quantity: x['quantity'], unitPrice: x['unitPrice'], total: x['totalPrice'] }));
    const consumables = ((this.order?.['consumables'] as EntityRecord[]) ?? []).map((x) => ({ name: String(x['productName']), quantity: x['quantity'], unitPrice: x['unitPrice'], total: x['lineTotal'] }));
    return [...operations, ...parts, ...consumables];
  }
  get total(): number { return this.rows.reduce((sum, row) => sum + Number(row.total ?? 0), 0); }
  money(value: unknown): string { return money(value); }
  print(): void { window.print(); }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/api/api.service';
import { friendlyError } from '../../core/api/error.util';
import { EntityRecord } from '../../core/models/api.models';
import { dateText, money } from '../../shared/formatters';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-heading">
      <div>
        <h1>Dashboard</h1>
        <p>Günlük operasyon özeti</p>
      </div>
      <button class="ghost-button" type="button" (click)="load()">Yenile</button>
    </section>

    <div class="state-card" *ngIf="loading">Dashboard bilgileri yükleniyor...</div>
    <div class="state-card error" *ngIf="error">
      {{ error }}
      <button class="ghost-button" type="button" (click)="load()">Tekrar Dene</button>
    </div>

    <ng-container *ngIf="!loading && !error">
      <div class="metric-grid">
        <article class="metric-card" *ngFor="let metric of metrics">
          <span>{{ metric.label }}</span>
          <strong>{{ metric.value }}</strong>
        </article>
      </div>

      <div class="content-grid">
        <section class="panel">
          <h2>Son Servisler</h2>
          <p class="muted" *ngIf="recentServiceOrders.length === 0">Henüz servis kaydı yok.</p>
          <div class="list-row" *ngFor="let order of recentServiceOrders">
            <div>
              <strong>{{ order['orderNo'] || order['serviceOrderNo'] || order['orderNumber'] || order['vehiclePlate'] || 'Servis' }}</strong>
              <small>{{ order['customerName'] || order['vehiclePlate'] || order['plate'] || '-' }}</small>
            </div>
            <span>{{ dateText(order['openedAt'] || order['createdAt']) }}</span>
          </div>
        </section>

        <section class="panel">
          <h2>Kritik Expertiz</h2>
          <p class="muted" *ngIf="criticalInspections.length === 0">Kritik expertiz bulunmuyor.</p>
          <div class="list-row" *ngFor="let item of criticalInspections">
            <div>
              <strong>{{ item['inspectionNo'] || item['plate'] || 'Expertiz' }}</strong>
              <small>{{ item['customerName'] || '-' }}</small>
            </div>
            <span>{{ item['statusText'] || '-' }}</span>
          </div>
        </section>
      </div>
    </ng-container>
  `
})
export class DashboardComponent implements OnInit {
  loading = false;
  error = '';
  data: EntityRecord | null = null;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  get metrics(): { label: string; value: string }[] {
    const source = this.data ?? {};
    return [
      { label: 'Toplam müşteri', value: this.value(source, ['totalCustomers', 'customerCount', 'totalCustomerCount']) },
      { label: 'Toplam araç', value: this.value(source, ['totalVehicles', 'vehicleCount', 'totalVehicleCount']) },
      { label: 'Açık servis', value: this.value(source, ['openServiceOrders', 'openServiceOrderCount']) },
      { label: 'Bugünkü randevu', value: this.value(source, ['todayAppointments', 'todayAppointmentCount']) },
      { label: 'Bu ay tamamlanan', value: this.value(source, ['completedThisMonth', 'completedServiceOrdersThisMonth', 'completedServiceCountThisMonth']) },
      { label: 'Bu ay tahsilat', value: money(source['monthlyCollection'] ?? source['collectionsThisMonth'] ?? source['paymentsThisMonth'] ?? source['totalPaymentsThisMonth']) },
      { label: 'Kritik expertiz', value: this.value(source, ['criticalInspectionCount']) }
    ];
  }

  get recentServiceOrders(): EntityRecord[] {
    return this.arrayFrom(['recentServiceOrders', 'latestServiceOrders', 'recentOrders']);
  }

  get criticalInspections(): EntityRecord[] {
    return this.arrayFrom(['criticalInspections', 'riskInspections']);
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api.get<EntityRecord>('/api/dashboard/daily').subscribe({
      next: (data) => {
        this.data = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = friendlyError(err, 'Dashboard bilgileri şu anda yüklenemedi. Lütfen tekrar deneyin.');
        this.loading = false;
      }
    });
  }

  dateText(value: unknown): string {
    return dateText(value);
  }

  private value(source: EntityRecord, keys: string[]): string {
    const found = keys.map((key) => source[key]).find((value) => value !== undefined && value !== null);
    return String(found ?? 0);
  }

  private arrayFrom(keys: string[]): EntityRecord[] {
    for (const key of keys) {
      const value = this.data?.[key];
      if (Array.isArray(value)) {
        return value as EntityRecord[];
      }
    }

    return [];
  }
}

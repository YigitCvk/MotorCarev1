import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, finalize, takeUntil } from 'rxjs';
import {
  SERVICE_ORDER_STATUS_OPTIONS,
  ServiceOrder,
  ServiceOrderStatus,
  serviceOrderStatusLabel
} from '../models/service-order.models';
import { ServiceOrdersApiService } from '../services/service-orders-api.service';

@Component({
  selector: 'mc-service-order-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './service-order-list.component.html',
  styleUrls: ['./service-order-list.component.scss']
})
export class ServiceOrderListComponent implements OnInit, OnDestroy {
  readonly statusOptions = SERVICE_ORDER_STATUS_OPTIONS;
  readonly filters = new FormGroup({
    q: new FormControl<string>(''),
    status: new FormControl<ServiceOrderStatus | ''>(''),
    openedFrom: new FormControl<string>(''),
    openedTo: new FormControl<string>('')
  });

  serviceOrders: ServiceOrder[] = [];
  loading = false;
  errorMessage = '';
  pageNumber = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;
  hasPreviousPage = false;
  hasNextPage = false;

  private readonly destroy$ = new Subject<void>();
  private readonly currencyFormatter = new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    maximumFractionDigits: 2
  });

  constructor(private readonly api: ServiceOrdersApiService) {}

  ngOnInit(): void {
    this.loadServiceOrders();
    this.filters.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged((left, right) => JSON.stringify(left) === JSON.stringify(right)),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.pageNumber = 1;
        this.loadServiceOrders();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  clearFilters(): void {
    this.filters.reset({ q: '', status: '', openedFrom: '', openedTo: '' });
  }

  previousPage(): void {
    if (!this.hasPreviousPage) {
      return;
    }

    this.pageNumber -= 1;
    this.loadServiceOrders();
  }

  nextPage(): void {
    if (!this.hasNextPage) {
      return;
    }

    this.pageNumber += 1;
    this.loadServiceOrders();
  }

  statusLabel(status: string | null | undefined): string {
    return serviceOrderStatusLabel(status);
  }

  money(value: number | null | undefined): string {
    return this.currencyFormatter.format(value ?? 0);
  }

  loadServiceOrders(): void {
    const values = this.filters.getRawValue();
    this.loading = true;
    this.errorMessage = '';

    this.api
      .getServiceOrders({
        q: values.q?.trim() || null,
        status: values.status || null,
        openedFrom: this.toDateTimeOffset(values.openedFrom, false),
        openedTo: this.toDateTimeOffset(values.openedTo, true),
        pageNumber: this.pageNumber,
        pageSize: this.pageSize
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          this.serviceOrders = result.items ?? [];
          this.pageNumber = result.pageNumber;
          this.pageSize = result.pageSize;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.hasPreviousPage = result.hasPreviousPage;
          this.hasNextPage = result.hasNextPage;
        },
        error: () => {
          this.errorMessage = 'Servis emirleri alınamadı.';
          this.serviceOrders = [];
        }
      });
  }

  private toDateTimeOffset(value: string | null | undefined, endOfDay: boolean): string | null {
    if (!value) {
      return null;
    }

    return `${value}T${endOfDay ? '23:59:59' : '00:00:00'}`;
  }
}

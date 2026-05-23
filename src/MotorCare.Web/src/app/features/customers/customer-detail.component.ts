import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import {
  CustomerServiceOrderHistoryItem,
  CustomerSummary,
  UpdateCustomerRequest
} from './models/customer.models';
import { CustomersService } from './services/customers.service';

type CustomerTab = 'overview' | 'vehicles' | 'serviceOrders' | 'notes';

@Component({
  selector: 'mc-customer-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './customer-detail.component.html',
  styleUrl: './customer-detail.component.css'
})
export class CustomerDetailComponent implements OnInit {
  customerId = '';
  summary: CustomerSummary | null = null;
  activeTab: CustomerTab = 'overview';
  isLoading = true;
  isSaving = false;
  errorMessage = '';
  saveMessage = '';

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    phone: ['', [Validators.required, Validators.maxLength(20)]],
    email: ['', [Validators.email, Validators.maxLength(150)]],
    whatsapp: ['', [Validators.maxLength(20)]],
    notes: ['', [Validators.maxLength(1000)]]
  });

  constructor(
    private readonly route: ActivatedRoute,
    private readonly fb: FormBuilder,
    private readonly customersService: CustomersService
  ) {}

  ngOnInit(): void {
    this.customerId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadSummary();
  }

  setTab(tab: CustomerTab): void {
    this.activeTab = tab;
  }

  saveCustomer(): void {
    this.errorMessage = '';
    this.saveMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage = 'Lütfen müşteri bilgilerini kontrol edin.';
      return;
    }

    this.isSaving = true;
    this.customersService
      .update(this.customerId, this.toRequest())
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: () => {
          this.saveMessage = 'Müşteri bilgileri güncellendi.';
          this.loadSummary(false);
        },
        error: (error: Error) => (this.errorMessage = error.message)
      });
  }

  remainingTotal(order: CustomerServiceOrderHistoryItem): number {
    return Math.max(order.grandTotal - order.paidTotal, 0);
  }

  openServiceOrderCount(): number {
    return this.summary?.serviceOrders.filter((order) =>
      ['Open', 'InProgress', 'WaitingForParts'].includes(order.status)
    ).length ?? 0;
  }

  totalSpent(): number {
    return this.summary?.serviceOrders.reduce((total, order) => total + order.grandTotal, 0) ?? 0;
  }

  isInvalid(name: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.dirty || control.touched);
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Open':
        return 'open';
      case 'InProgress':
        return 'progress';
      case 'WaitingForParts':
        return 'waiting';
      case 'Completed':
      case 'Delivered':
        return 'done';
      case 'Cancelled':
        return 'cancelled';
      default:
        return '';
    }
  }

  private loadSummary(showLoading = true): void {
    if (showLoading) {
      this.isLoading = true;
    }

    this.errorMessage = '';
    this.customersService
      .getSummary(this.customerId)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (summary) => {
          this.summary = summary;
          this.form.patchValue({
            fullName: summary.fullName,
            phone: summary.phone ?? '',
            email: summary.email ?? '',
            whatsapp: summary.whatsapp ?? '',
            notes: summary.notes ?? ''
          });
        },
        error: (error: Error) => {
          this.summary = null;
          this.errorMessage = error.message;
        }
      });
  }

  private toRequest(): UpdateCustomerRequest {
    const value = this.form.getRawValue();

    return {
      fullName: value.fullName?.trim() ?? '',
      phone: value.phone?.trim() ?? '',
      email: toOptional(value.email),
      whatsapp: toOptional(value.whatsapp),
      notes: toOptional(value.notes)
    };
  }
}

function toOptional(value: string | null | undefined): string | null {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, finalize, takeUntil } from 'rxjs';
import { CustomerLookup, normalizeOptionalText, VehicleLookup } from '../models/service-order.models';
import { ServiceOrdersApiService } from '../services/service-orders-api.service';

@Component({
  selector: 'mc-service-order-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './service-order-create.component.html',
  styleUrls: ['./service-order-create.component.scss']
})
export class ServiceOrderCreateComponent implements OnInit, OnDestroy {
  readonly customerSearch = this.fb.nonNullable.control('');
  readonly form = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    vehicleId: ['', Validators.required],
    vehicleKm: [0, [Validators.required, Validators.min(0)]],
    complaint: ['', Validators.maxLength(1000)]
  });

  customers: CustomerLookup[] = [];
  vehicles: VehicleLookup[] = [];
  loadingCustomers = false;
  loadingVehicles = false;
  saving = false;
  errorMessage = '';

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly api: ServiceOrdersApiService,
    private readonly router: Router,
    private readonly route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.searchCustomers('');

    this.customerSearch.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((query) => this.searchCustomers(query));

    this.form.controls.customerId.valueChanges.pipe(takeUntil(this.destroy$)).subscribe((customerId) => {
      this.form.controls.vehicleId.setValue('');
      this.vehicles = [];
      if (customerId) {
        this.loadVehicles(customerId);
      }
    });

    this.form.controls.vehicleId.valueChanges.pipe(takeUntil(this.destroy$)).subscribe((vehicleId) => {
      const vehicle = this.vehicles.find((item) => item.id === vehicleId);
      if (vehicle?.currentKm !== undefined && vehicle.currentKm !== null) {
        this.form.controls.vehicleKm.setValue(vehicle.currentKm);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving = true;
    this.errorMessage = '';

    this.api
      .createServiceOrder({
        customerId: value.customerId,
        vehicleId: value.vehicleId,
        vehicleKm: Number(value.vehicleKm),
        complaint: normalizeOptionalText(value.complaint)
      })
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (id) => this.router.navigate(['../', id], { relativeTo: this.route }),
        error: () => {
          this.errorMessage = 'Servis emri oluşturulamadı.';
        }
      });
  }

  fieldInvalid(name: 'customerId' | 'vehicleId' | 'vehicleKm' | 'complaint'): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || control.dirty);
  }

  private searchCustomers(query: string): void {
    this.loadingCustomers = true;
    this.api
      .searchCustomers(query.trim())
      .pipe(finalize(() => (this.loadingCustomers = false)))
      .subscribe({
        next: (result) => {
          this.customers = result.items ?? [];
        },
        error: () => {
          this.customers = [];
        }
      });
  }

  private loadVehicles(customerId: string): void {
    this.loadingVehicles = true;
    this.api
      .getCustomerVehicles(customerId)
      .pipe(finalize(() => (this.loadingVehicles = false)))
      .subscribe({
        next: (vehicles) => {
          this.vehicles = vehicles ?? [];
        },
        error: () => {
          this.vehicles = [];
        }
      });
  }
}

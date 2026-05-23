import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, Subject, finalize, takeUntil } from 'rxjs';
import {
  PAYMENT_METHOD_OPTIONS,
  PaymentMethodValue,
  SERVICE_ORDER_STATUS_OPTIONS,
  ServiceOrder,
  ServiceOrderStatus,
  lineTotal,
  normalizeOptionalText,
  paymentMethodLabel,
  serviceOrderStatusApiValue,
  serviceOrderStatusLabel
} from '../models/service-order.models';
import { ServiceOrdersApiService } from '../services/service-orders-api.service';

@Component({
  selector: 'mc-service-order-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './service-order-detail.component.html',
  styleUrls: ['./service-order-detail.component.scss']
})
export class ServiceOrderDetailComponent implements OnInit, OnDestroy {
  readonly statusOptions = SERVICE_ORDER_STATUS_OPTIONS;
  readonly paymentMethodOptions = PAYMENT_METHOD_OPTIONS;

  readonly operationForm = this.fb.nonNullable.group(
    {
      description: ['', [Validators.required, Validators.maxLength(500)]],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      discount: [0, [Validators.min(0)]],
      notes: ['', Validators.maxLength(250)]
    },
    { validators: lineDiscountValidator('quantity', 'unitPrice', 'discount') }
  );

  readonly partForm = this.fb.nonNullable.group(
    {
      partName: ['', [Validators.required, Validators.maxLength(200)]],
      partNumber: ['', Validators.maxLength(100)],
      unitPrice: [0, [Validators.required, Validators.min(0.01)]],
      quantity: [1, [Validators.required, Validators.min(1), Validators.pattern(/^\d+$/)]],
      discount: [0, [Validators.min(0)]],
      notes: ['', Validators.maxLength(250)]
    },
    { validators: lineDiscountValidator('quantity', 'unitPrice', 'discount') }
  );

  readonly consumableForm = this.fb.nonNullable.group({
    category: ['', [Validators.required, Validators.maxLength(64)]],
    brand: ['', Validators.maxLength(80)],
    productName: ['', [Validators.required, Validators.maxLength(160)]],
    subCategory: ['', Validators.maxLength(100)],
    specification: ['', Validators.maxLength(160)],
    unitPrice: [0, [Validators.required, Validators.min(0)]],
    quantity: [1, [Validators.required, Validators.min(1), Validators.pattern(/^\d+$/)]],
    notes: ['', Validators.maxLength(250)]
  });

  readonly paymentForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    method: [1 as PaymentMethodValue, Validators.required],
    paymentDate: [todayInputValue()]
  });

  readonly discountForm = this.fb.nonNullable.group({
    discount: [0, [Validators.required, Validators.min(0)]]
  });

  readonly statusForm = this.fb.nonNullable.group({
    status: ['Open' as ServiceOrderStatus, Validators.required],
    note: ['', Validators.maxLength(250)]
  });

  order?: ServiceOrder;
  orderId = '';
  loading = false;
  savingTarget = '';
  errorMessage = '';
  successMessage = '';

  private readonly destroy$ = new Subject<void>();
  private readonly currencyFormatter = new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    maximumFractionDigits: 2
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly api: ServiceOrdersApiService
  ) {}

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadOrder();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get isLocked(): boolean {
    return ['Completed', 'Cancelled', 'Delivered'].includes(String(this.order?.status));
  }

  get canAcceptPayment(): boolean {
    return !this.isLocked && (this.order?.remainingTotal ?? 0) > 0;
  }

  get operationPreviewTotal(): number {
    const value = this.operationForm.getRawValue();
    return lineTotal(value.quantity, value.unitPrice, value.discount);
  }

  get partPreviewTotal(): number {
    const value = this.partForm.getRawValue();
    return lineTotal(value.quantity, value.unitPrice, value.discount);
  }

  get consumablePreviewTotal(): number {
    const value = this.consumableForm.getRawValue();
    return lineTotal(value.quantity, value.unitPrice, 0);
  }

  get subtotal(): number {
    if (!this.order) {
      return 0;
    }

    return this.order.laborTotal + this.order.partsTotal + this.order.consumablesTotal;
  }

  money(value: number | null | undefined): string {
    return this.currencyFormatter.format(value ?? 0);
  }

  statusLabel(status: string | null | undefined): string {
    return serviceOrderStatusLabel(status);
  }

  paymentLabel(method: string | number | null | undefined): string {
    return paymentMethodLabel(method);
  }

  fieldInvalid(form: FormGroup, name: string): boolean {
    const control = form.get(name);
    return !!control && control.invalid && (control.touched || control.dirty);
  }

  hasLineDiscountError(form: FormGroup): boolean {
    return form.hasError('discountOverLineTotal') && (form.touched || form.dirty);
  }

  loadOrder(): void {
    if (!this.orderId) {
      this.errorMessage = 'Servis emri bulunamadı.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.api
      .getServiceOrder(this.orderId)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.loading = false))
      )
      .subscribe({
        next: (order) => {
          this.order = order;
          this.statusForm.patchValue({ status: order.status as ServiceOrderStatus, note: '' });
          this.discountForm.patchValue({ discount: order.discountTotal ?? 0 });
          this.paymentForm.patchValue({ amount: order.remainingTotal > 0 ? order.remainingTotal : 0 });
        },
        error: () => {
          this.errorMessage = 'Servis emri alınamadı.';
        }
      });
  }

  addOperation(): void {
    if (this.operationForm.invalid || this.isLocked) {
      this.operationForm.markAllAsTouched();
      return;
    }

    const value = this.operationForm.getRawValue();
    this.save('operation', () =>
      this.api.addOperation(this.orderId, {
        description: value.description.trim(),
        quantity: Number(value.quantity),
        unitPrice: Number(value.unitPrice),
        discount: Number(value.discount || 0),
        notes: normalizeOptionalText(value.notes),
        serviceCatalogItemId: null
      })
    );
  }

  addPart(): void {
    if (this.partForm.invalid || this.isLocked) {
      this.partForm.markAllAsTouched();
      return;
    }

    const value = this.partForm.getRawValue();
    this.save('part', () =>
      this.api.addPart(this.orderId, {
        partName: value.partName.trim(),
        partNumber: normalizeOptionalText(value.partNumber),
        unitPrice: Number(value.unitPrice),
        quantity: Number(value.quantity),
        inventoryItemId: null,
        discount: Number(value.discount || 0),
        notes: normalizeOptionalText(value.notes)
      })
    );
  }

  addConsumable(): void {
    if (this.consumableForm.invalid || this.isLocked) {
      this.consumableForm.markAllAsTouched();
      return;
    }

    const value = this.consumableForm.getRawValue();
    this.save('consumable', () =>
      this.api.addConsumable(this.orderId, {
        category: value.category.trim(),
        productName: value.productName.trim(),
        unitPrice: Number(value.unitPrice),
        quantity: Number(value.quantity),
        brand: normalizeOptionalText(value.brand),
        subCategory: normalizeOptionalText(value.subCategory),
        specification: normalizeOptionalText(value.specification),
        notes: normalizeOptionalText(value.notes)
      })
    );
  }

  addPayment(): void {
    if (!this.canAcceptPayment) {
      return;
    }

    const value = this.paymentForm.getRawValue();
    if (Number(value.amount) > (this.order?.remainingTotal ?? 0)) {
      this.paymentForm.controls.amount.setErrors({ overRemaining: true });
    }

    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    this.save('payment', () =>
      this.api.addPayment(this.orderId, {
        amount: Number(value.amount),
        method: Number(value.method) as PaymentMethodValue,
        paymentDate: value.paymentDate ? `${value.paymentDate}T12:00:00` : null
      })
    );
  }

  setDiscount(): void {
    const value = this.discountForm.getRawValue();
    if (Number(value.discount) > this.subtotal) {
      this.discountForm.controls.discount.setErrors({ overSubtotal: true });
    }

    if (this.discountForm.invalid || this.isLocked) {
      this.discountForm.markAllAsTouched();
      return;
    }

    this.save('discount', () =>
      this.api.setDiscount(this.orderId, {
        discount: Number(value.discount)
      })
    );
  }

  updateStatus(): void {
    if (this.statusForm.invalid) {
      this.statusForm.markAllAsTouched();
      return;
    }

    const value = this.statusForm.getRawValue();
    this.save('status', () =>
      this.api.updateStatus(this.orderId, {
        status: serviceOrderStatusApiValue(value.status as ServiceOrderStatus),
        note: normalizeOptionalText(value.note)
      })
    );
  }

  removeOperation(operationId: string): void {
    if (!window.confirm('İşçilik satırı silinsin mi?')) {
      return;
    }

    this.save('removeOperation', () => this.api.removeOperation(this.orderId, operationId));
  }

  removePart(partId: string): void {
    if (!window.confirm('Parça satırı silinsin mi?')) {
      return;
    }

    this.save('removePart', () => this.api.removePart(this.orderId, partId));
  }

  removeConsumable(consumableId: string): void {
    if (!window.confirm('Sarf satırı silinsin mi?')) {
      return;
    }

    this.save('removeConsumable', () => this.api.removeConsumable(this.orderId, consumableId));
  }

  private save(target: string, request: () => Observable<void>): void {
    this.savingTarget = target;
    this.errorMessage = '';
    this.successMessage = '';

    request()
      .pipe(finalize(() => (this.savingTarget = '')))
      .subscribe({
        next: () => {
          this.successMessage = 'Kayıt güncellendi.';
          this.resetForm(target);
          this.loadOrder();
        },
        error: () => {
          this.errorMessage = 'İşlem tamamlanamadı.';
        }
      });
  }

  private resetForm(target: string): void {
    if (target === 'operation') {
      this.operationForm.reset({ description: '', quantity: 1, unitPrice: 0, discount: 0, notes: '' });
    }

    if (target === 'part') {
      this.partForm.reset({ partName: '', partNumber: '', unitPrice: 0, quantity: 1, discount: 0, notes: '' });
    }

    if (target === 'consumable') {
      this.consumableForm.reset({
        category: '',
        brand: '',
        productName: '',
        subCategory: '',
        specification: '',
        unitPrice: 0,
        quantity: 1,
        notes: ''
      });
    }
  }
}

function lineDiscountValidator(quantityKey: string, unitPriceKey: string, discountKey: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const quantity = Number(control.get(quantityKey)?.value ?? 0);
    const unitPrice = Number(control.get(unitPriceKey)?.value ?? 0);
    const discount = Number(control.get(discountKey)?.value ?? 0);

    return discount > quantity * unitPrice ? { discountOverLineTotal: true } : null;
  };
}

function todayInputValue(): string {
  const now = new Date();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${now.getFullYear()}-${month}-${day}`;
}

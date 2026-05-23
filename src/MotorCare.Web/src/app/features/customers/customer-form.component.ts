import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { CreateCustomerRequest } from './models/customer.models';
import { CustomersService } from './services/customers.service';

@Component({
  selector: 'mc-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.css'
})
export class CustomerFormComponent {
  isSaving = false;
  errorMessage = '';

  readonly form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    phone: ['', [Validators.required, Validators.maxLength(20)]],
    email: ['', [Validators.email, Validators.maxLength(150)]],
    whatsapp: ['', [Validators.maxLength(20)]],
    notes: ['', [Validators.maxLength(1000)]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly customersService: CustomersService,
    private readonly router: Router
  ) {}

  save(): void {
    this.errorMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage = 'Lütfen zorunlu alanları kontrol edin.';
      return;
    }

    this.isSaving = true;
    this.customersService
      .create(this.toRequest())
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (id) => this.router.navigate(['/customers', id]),
        error: (error: Error) => (this.errorMessage = error.message)
      });
  }

  isInvalid(name: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.dirty || control.touched);
  }

  private toRequest(): CreateCustomerRequest {
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

import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { friendlyError } from '../../core/api/error.util';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="auth-page">
      <div class="auth-panel">
        <h1>GarajPass</h1>
        <p>Atölye operasyonları için Angular tabanlı yeni MotorCare arayüzü.</p>
        <div class="action-row">
          <a class="primary-button" routerLink="/login">Giriş Yap</a>
          <a class="ghost-button" routerLink="/register">İşletme Oluştur</a>
        </div>
      </div>
    </section>
  `
})
export class HomeComponent {
  constructor(auth: AuthService, router: Router) {
    if (auth.isAuthenticated) {
      void router.navigateByUrl(auth.roleLanding());
    }
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="auth-page">
      <form class="auth-panel" [formGroup]="form" (ngSubmit)="submit()">
        <h1>Giriş</h1>
        <label>İşletme kodu<input formControlName="tenantIdentifier" autocomplete="organization" /></label>
        <label>E-posta<input formControlName="email" type="email" autocomplete="email" /></label>
        <label>Şifre<input formControlName="password" type="password" autocomplete="current-password" /></label>
        <p class="error" *ngIf="error">{{ error }}</p>
        <button class="primary-button" [disabled]="form.invalid || loading">{{ loading ? 'Giriş yapılıyor...' : 'Giriş Yap' }}</button>
        <div class="auth-links">
          <a routerLink="/forgot-password">Şifremi unuttum</a>
          <a routerLink="/register">Yeni işletme</a>
        </div>
      </form>
    </section>
  `
})
export class LoginComponent {
  loading = false;
  error = '';
  readonly form = this.fb.nonNullable.group({
    tenantIdentifier: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth
      .login(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (response) => {
          if (response.requiresTwoFactor) {
            void this.router.navigate(['/two-factor'], { queryParams: { ticket: response.twoFactorToken } });
            return;
          }
          void this.router.navigateByUrl(this.auth.roleLanding(response.role));
        },
        error: (err) => (this.error = friendlyError(err, 'Giriş yapılamadı. Bilgileri kontrol edip tekrar deneyin.'))
      });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="auth-page">
      <form class="auth-panel" [formGroup]="form" (ngSubmit)="submit()">
        <h1>İşletme Oluştur</h1>
        <label>İşletme kodu<input formControlName="tenantIdentifier" /></label>
        <label>İşletme adı<input formControlName="tenantName" /></label>
        <label>Yetkili ad soyad<input formControlName="ownerFullName" /></label>
        <label>Yetkili e-posta<input formControlName="ownerEmail" type="email" /></label>
        <label>Şifre<input formControlName="ownerPassword" type="password" /></label>
        <p class="error" *ngIf="error">{{ error }}</p>
        <p class="success" *ngIf="message">{{ message }}</p>
        <button class="primary-button" [disabled]="form.invalid || loading">İşletmeyi Oluştur</button>
        <a routerLink="/verify-email">Doğrulama kodum var</a>
      </form>
    </section>
  `
})
export class RegisterComponent {
  loading = false;
  error = '';
  message = '';
  readonly form = this.fb.nonNullable.group({
    tenantIdentifier: ['', Validators.required],
    tenantName: ['', Validators.required],
    ownerFullName: ['', Validators.required],
    ownerEmail: ['', [Validators.required, Validators.email]],
    ownerPassword: ['', [Validators.required, Validators.minLength(8)]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth
      .register(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          this.message = 'İşletme oluşturuldu. E-posta doğrulama kodunu girin.';
          void this.router.navigate(['/verify-email'], {
            queryParams: {
              tenantIdentifier: this.form.controls.tenantIdentifier.value,
              email: this.form.controls.ownerEmail.value
            }
          });
        },
        error: (err) => (this.error = friendlyError(err))
      });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="auth-page">
      <form class="auth-panel" [formGroup]="form" (ngSubmit)="submit()">
        <h1>E-posta Doğrulama</h1>
        <label>İşletme kodu<input formControlName="tenantIdentifier" /></label>
        <label>E-posta<input formControlName="email" type="email" /></label>
        <label>6 haneli kod<input formControlName="code" inputmode="numeric" maxlength="6" /></label>
        <p class="error" *ngIf="error">{{ error }}</p>
        <p class="success" *ngIf="message">{{ message }}</p>
        <button class="primary-button" [disabled]="form.invalid || loading">Doğrula</button>
      </form>
    </section>
  `
})
export class VerifyEmailComponent {
  loading = false;
  error = '';
  message = '';
  readonly form = this.fb.nonNullable.group({
    tenantIdentifier: [this.route.snapshot.queryParamMap.get('tenantIdentifier') ?? '', Validators.required],
    email: [this.route.snapshot.queryParamMap.get('email') ?? '', [Validators.required, Validators.email]],
    code: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth
      .verifyEmail(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          this.message = 'E-posta doğrulandı. Giriş yapabilirsiniz.';
          setTimeout(() => void this.router.navigate(['/login']), 600);
        },
        error: (err) => (this.error = friendlyError(err, 'Doğrulama kodu geçersiz veya süresi dolmuş.'))
      });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="auth-page">
      <form class="auth-panel" [formGroup]="form" (ngSubmit)="submit()">
        <h1>Şifre Sıfırlama</h1>
        <label>İşletme kodu<input formControlName="tenantIdentifier" /></label>
        <label>E-posta<input formControlName="email" type="email" /></label>
        <p class="error" *ngIf="error">{{ error }}</p>
        <p class="success" *ngIf="message">{{ message }}</p>
        <button class="primary-button" [disabled]="form.invalid || loading">Kod Gönder</button>
        <a routerLink="/reset-password">Kodum var</a>
      </form>
    </section>
  `
})
export class ForgotPasswordComponent {
  loading = false;
  error = '';
  message = '';
  readonly form = this.fb.nonNullable.group({
    tenantIdentifier: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService
  ) {}

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.auth
      .forgotPassword(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => (this.message = 'Eğer hesap varsa şifre sıfırlama kodu gönderildi.'),
        error: (err) => (this.error = friendlyError(err))
      });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="auth-page">
      <form class="auth-panel" [formGroup]="form" (ngSubmit)="submit()">
        <h1>Yeni Şifre</h1>
        <label>İşletme kodu<input formControlName="tenantIdentifier" /></label>
        <label>E-posta<input formControlName="email" type="email" /></label>
        <label>Kod<input formControlName="code" inputmode="numeric" /></label>
        <label>Yeni şifre<input formControlName="newPassword" type="password" /></label>
        <label>Yeni şifre tekrar<input formControlName="confirmPassword" type="password" /></label>
        <p class="error" *ngIf="error">{{ error }}</p>
        <p class="success" *ngIf="message">{{ message }}</p>
        <button class="primary-button" [disabled]="form.invalid || loading">Şifreyi Güncelle</button>
      </form>
    </section>
  `
})
export class ResetPasswordComponent {
  loading = false;
  error = '';
  message = '';
  readonly form = this.fb.nonNullable.group({
    tenantIdentifier: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    code: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) return;
    if (this.form.controls.newPassword.value !== this.form.controls.confirmPassword.value) {
      this.error = 'Şifreler eşleşmiyor.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.auth
      .resetPassword(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          this.message = 'Şifre güncellendi. Giriş yapabilirsiniz.';
          setTimeout(() => void this.router.navigate(['/login']), 600);
        },
        error: (err) => (this.error = friendlyError(err, 'Şifre sıfırlama kodu geçersiz veya süresi dolmuş.'))
      });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="auth-page">
      <form class="auth-panel" [formGroup]="form" (ngSubmit)="submit()">
        <h1>Davet Kabul</h1>
        <label>Davet token<input formControlName="token" /></label>
        <label>Ad soyad<input formControlName="fullName" /></label>
        <label>Şifre<input formControlName="password" type="password" /></label>
        <label>Şifre tekrar<input formControlName="confirmPassword" type="password" /></label>
        <p class="error" *ngIf="error">{{ error }}</p>
        <p class="success" *ngIf="message">{{ message }}</p>
        <button class="primary-button" [disabled]="form.invalid || loading">Davet Kabul Et</button>
      </form>
    </section>
  `
})
export class AcceptInviteComponent {
  loading = false;
  error = '';
  message = '';
  readonly form = this.fb.nonNullable.group({
    token: [this.route.snapshot.queryParamMap.get('token') ?? '', Validators.required],
    fullName: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) return;
    if (this.form.controls.password.value !== this.form.controls.confirmPassword.value) {
      this.error = 'Şifreler eşleşmiyor.';
      return;
    }

    this.loading = true;
    this.auth
      .acceptInvite(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          this.message = 'Davet kabul edildi. Giriş yapabilirsiniz.';
          setTimeout(() => void this.router.navigate(['/login']), 600);
        },
        error: (err) => (this.error = friendlyError(err, 'Davet bağlantısı geçersiz veya süresi dolmuş.'))
      });
  }
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="auth-page">
      <form class="auth-panel" [formGroup]="form" (ngSubmit)="submit()">
        <h1>İki Aşamalı Doğrulama</h1>
        <label>Oturum bileti<input formControlName="ticket" /></label>
        <label>Kod<input formControlName="code" inputmode="numeric" /></label>
        <p class="error" *ngIf="error">{{ error }}</p>
        <button class="primary-button" [disabled]="form.invalid || loading">Doğrula</button>
      </form>
    </section>
  `
})
export class TwoFactorComponent {
  loading = false;
  error = '';
  readonly form = this.fb.nonNullable.group({
    ticket: [this.route.snapshot.queryParamMap.get('ticket') ?? '', Validators.required],
    code: ['', Validators.required]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.auth
      .verifyTwoFactor(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (response) => void this.router.navigateByUrl(this.auth.roleLanding(response.role)),
        error: (err) => (this.error = friendlyError(err, 'Doğrulama kodu geçersiz.'))
      });
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ServiceOrder, paymentMethodLabel, serviceOrderStatusLabel } from '../models/service-order.models';
import { ServiceOrdersApiService } from '../services/service-orders-api.service';

@Component({
  selector: 'mc-service-order-print',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './service-order-print.component.html',
  styleUrls: ['./service-order-print.component.scss']
})
export class ServiceOrderPrintComponent implements OnInit {
  order?: ServiceOrder;
  loading = false;
  errorMessage = '';

  private readonly currencyFormatter = new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    maximumFractionDigits: 2
  });

  constructor(
    private readonly route: ActivatedRoute,
    private readonly api: ServiceOrdersApiService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage = 'Servis emri bulunamadı.';
      return;
    }

    this.loading = true;
    this.api
      .getServiceOrder(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (order) => (this.order = order),
        error: () => {
          this.errorMessage = 'Servis emri alınamadı.';
        }
      });
  }

  print(): void {
    window.print();
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
}

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { CustomerLookupResponse } from './models/customer.models';
import { CustomersService } from './services/customers.service';

@Component({
  selector: 'mc-customers-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './customers-list.component.html',
  styleUrl: './customers-list.component.css'
})
export class CustomersListComponent implements OnInit {
  customers: CustomerLookupResponse[] = [];
  searchText = '';
  pageNumber = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;
  hasPreviousPage = false;
  hasNextPage = false;
  isLoading = false;
  errorMessage = '';

  constructor(private readonly customersService: CustomersService) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  search(): void {
    this.pageNumber = 1;
    this.loadCustomers();
  }

  reset(): void {
    this.searchText = '';
    this.pageNumber = 1;
    this.loadCustomers();
  }

  changePageSize(value: string): void {
    this.pageSize = Number(value);
    this.pageNumber = 1;
    this.loadCustomers();
  }

  previousPage(): void {
    if (!this.hasPreviousPage || this.isLoading) {
      return;
    }

    this.pageNumber -= 1;
    this.loadCustomers();
  }

  nextPage(): void {
    if (!this.hasNextPage || this.isLoading) {
      return;
    }

    this.pageNumber += 1;
    this.loadCustomers();
  }

  private loadCustomers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.customersService
      .searchCustomers(this.searchText, this.pageNumber, this.pageSize)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (result) => {
          this.customers = result.items ?? [];
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.hasPreviousPage = result.hasPreviousPage;
          this.hasNextPage = result.hasNextPage;
        },
        error: (error: Error) => {
          this.customers = [];
          this.errorMessage = error.message;
        }
      });
  }
}

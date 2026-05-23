import { Routes } from '@angular/router';

import { CustomerDetailComponent } from './customer-detail.component';
import { CustomerFormComponent } from './customer-form.component';
import { CustomersListComponent } from './customers-list.component';

export const CUSTOMER_ROUTES: Routes = [
  {
    path: '',
    component: CustomersListComponent,
    title: 'Müşteriler'
  },
  {
    path: 'create',
    component: CustomerFormComponent,
    title: 'Yeni Müşteri'
  },
  {
    path: ':id',
    component: CustomerDetailComponent,
    title: 'Müşteri Detayı'
  }
];

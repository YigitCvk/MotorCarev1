import { Routes } from '@angular/router';
import { ServiceOrderCreateComponent } from './pages/service-order-create.component';
import { ServiceOrderDetailComponent } from './pages/service-order-detail.component';
import { ServiceOrderListComponent } from './pages/service-order-list.component';
import { ServiceOrderPrintComponent } from './pages/service-order-print.component';

export const SERVICE_ORDER_ROUTES: Routes = [
  { path: '', component: ServiceOrderListComponent, title: 'Servis Emirleri' },
  { path: 'new', component: ServiceOrderCreateComponent, title: 'Yeni Servis Emri' },
  { path: ':id/print', component: ServiceOrderPrintComponent, title: 'Servis Emri Yazdır' },
  { path: ':id', component: ServiceOrderDetailComponent, title: 'Servis Emri Detayı' }
];

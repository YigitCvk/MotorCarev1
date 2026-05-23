import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { AppShellComponent } from './layout/app-shell.component';
import {
  AcceptInviteComponent,
  ForgotPasswordComponent,
  HomeComponent,
  LoginComponent,
  RegisterComponent,
  ResetPasswordComponent,
  TwoFactorComponent,
  VerifyEmailComponent
} from './features/auth/auth-pages.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import {
  AppointmentsComponent,
  CustomerDetailComponent,
  CustomerFormComponent,
  CustomersComponent,
  ForbiddenComponent,
  GenericCatalogComponent,
  InventoryComponent,
  SettingsBusinessComponent,
  SettingsComponent,
  SettingsUsersComponent,
  VehicleFormComponent,
  VehicleHistoryComponent,
  VehiclesComponent
} from './features/workspace/workspace-pages.component';
import {
  InspectionDetailComponent,
  InspectionFormComponent,
  InspectionPrintComponent,
  InspectionsComponent
} from './features/inspections/inspection-pages.component';
import { ServiceOrderCreateComponent } from './features/service-orders/pages/service-order-create.component';
import { ServiceOrderDetailComponent } from './features/service-orders/pages/service-order-detail.component';
import { ServiceOrderListComponent } from './features/service-orders/pages/service-order-list.component';
import { ServiceOrderPrintComponent } from './features/service-orders/pages/service-order-print.component';
import {
  PublicInspectionReportComponent,
  PublicServiceRecordComponent
} from './features/public-records/public-record-pages.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'verify-email', component: VerifyEmailComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'accept-invite', component: AcceptInviteComponent },
  { path: 'two-factor', component: TwoFactorComponent },
  { path: 'public/service-record/:slug', component: PublicServiceRecordComponent },
  { path: 'public/inspection-report/:slug', component: PublicInspectionReportComponent },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] } },
      { path: 'customers', component: CustomersComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] } },
      { path: 'customers/create', component: CustomerFormComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager'] } },
      { path: 'customers/:id', component: CustomerDetailComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] } },
      { path: 'vehicles', component: VehiclesComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] } },
      { path: 'vehicles/create', component: VehicleFormComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager'] } },
      { path: 'vehicles/:id/history', component: VehicleHistoryComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] } },
      { path: 'appointments', component: AppointmentsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] } },
      { path: 'appointments/create', component: AppointmentsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager'] } },
      { path: 'appointments/:id', component: AppointmentsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] } },
      { path: 'service-orders', component: ServiceOrderListComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Technician', 'Accountant', 'ReadOnly'] } },
      { path: 'service-orders/new', component: ServiceOrderCreateComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Technician'] } },
      { path: 'service-orders/create', redirectTo: 'service-orders/new', pathMatch: 'full' },
      { path: 'service-orders/:id/print', component: ServiceOrderPrintComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Technician', 'Accountant', 'ReadOnly'] } },
      { path: 'service-orders/:id', component: ServiceOrderDetailComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Technician', 'Accountant', 'ReadOnly'] } },
      { path: 'inspections', component: InspectionsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Inspector', 'ReadOnly'] } },
      { path: 'inspections/new', component: InspectionFormComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Inspector'] } },
      { path: 'inspections/create', redirectTo: 'inspections/new', pathMatch: 'full' },
      { path: 'inspections/:id/print', component: InspectionPrintComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Inspector', 'ReadOnly'] } },
      { path: 'inspections/:id', component: InspectionDetailComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Inspector', 'ReadOnly'] } },
      { path: 'inventory', component: InventoryComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] } },
      { path: 'inventory/create', component: InventoryComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager'] } },
      { path: 'inventory/:id/edit', component: InventoryComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager'] } },
      { path: 'services', component: GenericCatalogComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] } },
      { path: 'services/create', component: GenericCatalogComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager'] } },
      { path: 'services/:id/edit', component: GenericCatalogComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin', 'Manager'] } },
      { path: 'settings', component: SettingsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin'] } },
      { path: 'settings/business', component: SettingsBusinessComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin'] } },
      { path: 'settings/users', component: SettingsUsersComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin'] } },
      { path: 'settings/security', component: SettingsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin'] } },
      { path: 'settings/appointment-types', component: SettingsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin'] } },
      { path: 'settings/working-hours', component: SettingsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin'] } },
      { path: 'settings/notifications', component: SettingsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin'] } },
      { path: 'settings/subscription', component: SettingsComponent, canActivate: [roleGuard], data: { roles: ['Owner', 'Admin'] } },
      { path: 'forbidden', component: ForbiddenComponent }
    ]
  },
  { path: '**', redirectTo: '' }
];

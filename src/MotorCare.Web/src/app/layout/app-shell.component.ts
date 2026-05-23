import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';

interface NavigationItem {
  label: string;
  path: string;
  roles: string[];
  exact?: boolean;
}

@Component({
  selector: 'mc-app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  menuOpen = false;
  readonly currentUser$ = this.auth.currentUser$;

  readonly primaryNavigation: NavigationItem[] = [
    { label: 'Dashboard', path: '/dashboard', roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'], exact: true },
    { label: 'Müşteriler', path: '/customers', roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] },
    { label: 'Araçlar', path: '/vehicles', roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] },
    { label: 'Randevular', path: '/appointments', roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] },
    { label: 'Servis Kayıtları', path: '/service-orders', roles: ['Owner', 'Admin', 'Manager', 'Technician', 'Accountant', 'ReadOnly'] },
    { label: 'Expertiz', path: '/inspections', roles: ['Owner', 'Admin', 'Manager', 'Inspector', 'ReadOnly'] },
    { label: 'Stok', path: '/inventory', roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] },
    { label: 'Hizmet Kataloğu', path: '/services', roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] }
  ];

  readonly adminNavigation: NavigationItem[] = [
    { label: 'Ayarlar', path: '/settings', roles: ['Owner', 'Admin'], exact: true },
    { label: 'İşletme Bilgileri', path: '/settings/business', roles: ['Owner', 'Admin'] },
    { label: 'Kullanıcı Yönetimi', path: '/settings/users', roles: ['Owner', 'Admin'] }
  ];

  constructor(
    readonly auth: AuthService,
    private readonly router: Router
  ) {
    this.auth.loadCurrentUser().subscribe();
  }

  get visiblePrimaryNavigation(): NavigationItem[] {
    return this.primaryNavigation.filter((item) => this.auth.hasAnyRole(item.roles));
  }

  get visibleAdminNavigation(): NavigationItem[] {
    return this.adminNavigation.filter((item) => this.auth.hasAnyRole(item.roles));
  }

  closeMenu(): void {
    this.menuOpen = false;
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  logout(): void {
    this.closeMenu();
    this.auth.logout();
    void this.router.navigate(['/login']);
  }

  roleLabel(role?: string): string {
    switch (role) {
      case 'Owner':
        return 'Sahip';
      case 'Admin':
        return 'Yönetici';
      case 'Manager':
        return 'Operasyon';
      case 'Technician':
        return 'Teknisyen';
      case 'Inspector':
        return 'Expertiz';
      case 'Accountant':
        return 'Muhasebe';
      case 'ReadOnly':
        return 'Salt Okunur';
      default:
        return role ?? 'Kullanıcı';
    }
  }

  trackByPath(_: number, item: NavigationItem): string {
    return item.path;
  }
}

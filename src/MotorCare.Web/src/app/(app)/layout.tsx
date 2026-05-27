'use client';

import { useEffect, useState } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import Link from 'next/link';
import {
  BookOpen,
  CalendarDays,
  Car,
  ClipboardList,
  FileSearch,
  LayoutDashboard,
  LogOut,
  Menu,
  Package,
  Settings,
  Users,
} from 'lucide-react';
import clsx from 'clsx';
import { useAuth } from '@/core/auth/auth.context';
import { authService } from '@/core/auth/auth.service';
import { Breadcrumbs, BreadcrumbItem } from '@/components/ui/breadcrumbs';

const PATH_LABELS: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/customers': 'Müşteriler',
  '/customers/new': 'Yeni Müşteri',
  '/customers/create': 'Yeni Müşteri',
  '/vehicles': 'Araçlar',
  '/vehicles/new': 'Yeni Araç',
  '/appointments': 'Randevular',
  '/appointments/new': 'Yeni Randevu',
  '/service-orders': 'Servis Kayıtları',
  '/service-orders/new': 'Yeni Servis Emri',
  '/inspections': 'Expertiz',
  '/inspections/new': 'Yeni Expertiz',
  '/inventory': 'Stok',
  '/inventory/new': 'Yeni Stok Kalemi',
  '/inventory/create': 'Yeni Stok Kalemi',
  '/service-catalog': 'Hizmet Kataloğu',
  '/service-catalog/new': 'Yeni Hizmet',
  '/service-catalog/create': 'Yeni Hizmet',
  '/settings': 'Ayarlar',
  '/settings/business': 'Firma Ayarları',
  '/settings/users': 'Kullanıcı Yönetimi',
};

const EXISTING_ROUTES = new Set(Object.keys(PATH_LABELS));

function isActiveRoute(pathname: string, href: string): boolean {
  return pathname === href || pathname.startsWith(`${href}/`);
}

function getBreadcrumbs(pathname: string): BreadcrumbItem[] {
  if (!pathname || pathname === '/dashboard') return [];

  const segments = pathname.split('/').filter(Boolean);
  const items: BreadcrumbItem[] = [];
  let accumulated = '';

  for (let i = 0; i < segments.length; i++) {
    accumulated += `/${segments[i]}`;
    const isLast = i === segments.length - 1;
    const knownLabel = PATH_LABELS[accumulated];

    if (knownLabel) {
      items.push({
        label: knownLabel,
        href: isLast || !EXISTING_ROUTES.has(accumulated) ? undefined : accumulated,
      });
      continue;
    }

    const segment = segments[i];
    const label = segment === 'print' ? 'Yazdır' : 'Detay';
    items.push({ label });
  }

  return items;
}

interface NavItem {
  label: string;
  href: string;
  icon: React.ComponentType<{ size?: number; className?: string }>;
  roles: string[];
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', href: '/dashboard', icon: LayoutDashboard, roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] },
  { label: 'Müşteriler', href: '/customers', icon: Users, roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] },
  { label: 'Araçlar', href: '/vehicles', icon: Car, roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] },
  { label: 'Randevular', href: '/appointments', icon: CalendarDays, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] },
  { label: 'Servis Kayıtları', href: '/service-orders', icon: ClipboardList, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'Accountant', 'ReadOnly'] },
  { label: 'Expertiz', href: '/inspections', icon: FileSearch, roles: ['Owner', 'Admin', 'Manager', 'Inspector', 'ReadOnly'] },
  { label: 'Stok', href: '/inventory', icon: Package, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] },
  { label: 'Hizmet Kataloğu', href: '/service-catalog', icon: BookOpen, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] },
];

const BOTTOM_NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', href: '/dashboard', icon: LayoutDashboard, roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] },
  { label: 'Servis', href: '/service-orders', icon: ClipboardList, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'Accountant', 'ReadOnly'] },
  { label: 'Randevu', href: '/appointments', icon: CalendarDays, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] },
  { label: 'Müşteriler', href: '/customers', icon: Users, roles: ['Owner', 'Admin', 'Manager', 'Accountant', 'ReadOnly'] },
  { label: 'Expertiz', href: '/inspections', icon: FileSearch, roles: ['Owner', 'Admin', 'Manager', 'Inspector', 'ReadOnly'] },
];

function SidebarContent({
  user,
  pathname,
  onLogout,
  onNavigate,
}: {
  user: { email: string; role: string; fullName?: string } | null;
  pathname: string;
  onLogout: () => void;
  onNavigate?: () => void;
}) {
  const { hasRole } = useAuth();
  const visibleItems = NAV_ITEMS.filter((item) => hasRole(item.roles));

  return (
    <div className="flex h-full flex-col bg-white border-r border-slate-200">
      <div className="p-4 border-b border-slate-200">
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-lg bg-brand-600 flex items-center justify-center">
            <span className="text-white font-bold text-xs">GP</span>
          </div>
          <span className="font-bold text-slate-900">GarajPass</span>
        </div>
      </div>

      <nav className="flex-1 p-3 space-y-0.5 overflow-y-auto">
        {visibleItems.map((item) => {
          const Icon = item.icon;
          const isActive = isActiveRoute(pathname, item.href);

          return (
            <Link
              key={item.href}
              href={item.href}
              onClick={onNavigate}
              className={clsx('sidebar-link', isActive && 'sidebar-link-active')}
            >
              <Icon size={16} />
              {item.label}
            </Link>
          );
        })}
      </nav>

      <div className="border-t border-slate-200 p-3">
        {hasRole(['Owner', 'Admin']) && (
          <>
            <Link
              href="/settings/business"
              onClick={onNavigate}
              className={clsx('sidebar-link mb-0.5', isActiveRoute(pathname, '/settings/business') && 'sidebar-link-active')}
            >
              <Settings size={16} />
              Firma Ayarları
            </Link>
            <Link
              href="/settings/users"
              onClick={onNavigate}
              className={clsx('sidebar-link mb-1', isActiveRoute(pathname, '/settings/users') && 'sidebar-link-active')}
            >
              <Users size={16} />
              Kullanıcı Yönetimi
            </Link>
          </>
        )}
        <div className="px-3 py-2">
          <p className="text-xs font-medium text-slate-700 truncate">{user?.fullName ?? user?.email ?? ''}</p>
          <p className="text-xs text-slate-400 truncate">{user?.role ?? ''}</p>
        </div>
        <button onClick={onLogout} className="sidebar-link w-full text-left text-red-600 hover:bg-red-50 hover:text-red-700">
          <LogOut size={16} />
          Çıkış Yap
        </button>
      </div>
    </div>
  );
}

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const { user, isAuthenticated, isLoading, logout, hasRole } = useAuth();
  const router = useRouter();
  const pathname = usePathname();
  const [mounted, setMounted] = useState(false);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    if (mounted && !isLoading && !isAuthenticated) {
      router.replace('/login');
    }
  }, [mounted, isLoading, isAuthenticated, router]);

  const handleLogout = async () => {
    await logout();
    router.replace('/login');
  };

  if (!mounted || isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-brand-600" />
      </div>
    );
  }

  if (!isAuthenticated) return null;

  const currentPath = pathname ?? '';
  const visibleBottomItems = BOTTOM_NAV_ITEMS.filter((item) => hasRole(item.roles));
  const homeHref = authService.roleLanding(user?.role);

  return (
    <div className="flex h-screen bg-slate-50 overflow-hidden print:block print:h-auto print:overflow-visible print:bg-white">
      <div className="hidden lg:flex lg:w-64 lg:shrink-0 lg:flex-col print:hidden">
        <SidebarContent user={user} pathname={currentPath} onLogout={handleLogout} />
      </div>

      {sidebarOpen && (
        <div className="fixed inset-0 z-40 lg:hidden print:hidden">
          <div className="fixed inset-0 bg-slate-900/50" onClick={() => setSidebarOpen(false)} />
          <div className="fixed inset-y-0 left-0 z-50 w-64">
            <SidebarContent
              user={user}
              pathname={currentPath}
              onLogout={handleLogout}
              onNavigate={() => setSidebarOpen(false)}
            />
          </div>
        </div>
      )}

      <div className="flex flex-1 flex-col overflow-hidden print:block print:overflow-visible">
        <header className="shrink-0 bg-white border-b border-slate-200 px-4 py-3 flex items-center gap-3 lg:px-6 print:hidden">
          <button
            type="button"
            onClick={() => setSidebarOpen(true)}
            className="lg:hidden p-1.5 rounded-lg text-slate-500 hover:bg-slate-100"
            aria-label="Menüyü aç"
          >
            <Menu size={20} />
          </button>
          <Breadcrumbs items={getBreadcrumbs(currentPath)} homeHref={homeHref} className="hidden md:flex" />
          <div className="flex-1" />
          <div className="flex items-center gap-2">
            <span className="text-sm text-slate-600 hidden sm:block">{user?.email}</span>
          </div>
        </header>

        <main className="flex-1 overflow-auto print:block print:overflow-visible">
          <div className="p-4 pb-16 lg:p-6 lg:pb-6 print:p-0">{children}</div>
        </main>
      </div>

      <nav className="fixed bottom-0 left-0 right-0 z-30 bg-white border-t border-slate-200 flex lg:hidden print:hidden">
        {visibleBottomItems.map((item) => {
          const Icon = item.icon;
          const isActive = isActiveRoute(currentPath, item.href);

          return (
            <Link
              key={item.href}
              href={item.href}
              className={clsx(
                'flex flex-1 flex-col items-center py-2 text-xs gap-1',
                isActive ? 'text-brand-600' : 'text-slate-500',
              )}
            >
              <Icon size={isActive ? 22 : 20} />
              <span>{item.label}</span>
            </Link>
          );
        })}
      </nav>
    </div>
  );
}

'use client';

// src/app/(app)/layout.tsx

import { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import { useAuth } from '@/core/auth/auth.context';
import {
  LayoutDashboard,
  Users,
  Car,
  ClipboardList,
  FileSearch,
  Package,
  BookOpen,
  Settings,
  LogOut,
  Menu,
} from 'lucide-react';
import clsx from 'clsx';
import { Breadcrumbs, BreadcrumbItem } from '@/components/ui/breadcrumbs';

const PATH_LABELS: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/customers': 'Müşteriler',
  '/customers/create': 'Yeni Müşteri',
  '/vehicles': 'Araçlar',
  '/service-orders': 'Servis Kayıtları',
  '/service-orders/new': 'Yeni Servis Emri',
  '/inspections': 'Expertiz',
  '/inspections/new': 'Yeni Expertiz',
  '/inventory': 'Stok',
  '/inventory/create': 'Yeni Stok Kalemi',
  '/service-catalog': 'Hizmetler',
  '/service-catalog/create': 'Yeni Hizmet',
  '/settings/business': 'İşletme Profili',
  '/settings/users': 'Kullanıcılar',
};

function getBreadcrumbs(pathname: string): BreadcrumbItem[] {
  if (!pathname || pathname === '/dashboard') return [];
  const segments = pathname.split('/').filter(Boolean);
  const items: BreadcrumbItem[] = [];
  let accumulated = '';

  for (let i = 0; i < segments.length; i++) {
    accumulated += '/' + segments[i];
    const knownLabel = PATH_LABELS[accumulated];
    const isLast = i === segments.length - 1;

    if (knownLabel) {
      items.push({ label: knownLabel, href: isLast ? undefined : accumulated });
    } else {
      // Dynamic segment — treat as "Detay" for ID-like segments, "Yazdır" for print
      const seg = segments[i];
      let label = 'Detay';
      if (seg === 'print') label = 'Yazdır';
      items.push({ label, href: isLast ? undefined : accumulated });
    }
  }

  return items;
}

const BOTTOM_NAV_ITEMS = [
  { label: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { label: 'Servis', href: '/service-orders', icon: ClipboardList },
  { label: 'Müşteriler', href: '/customers', icon: Users },
  { label: 'Expertiz', href: '/inspections', icon: FileSearch },
  { label: 'Stok', href: '/inventory', icon: Package },
];

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
  { label: 'Servis Kayıtları', href: '/service-orders', icon: ClipboardList, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'Accountant', 'ReadOnly'] },
  { label: 'Expertiz', href: '/inspections', icon: FileSearch, roles: ['Owner', 'Admin', 'Manager', 'Inspector', 'ReadOnly'] },
  { label: 'Stok', href: '/inventory', icon: Package, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] },
  { label: 'Hizmetler', href: '/service-catalog', icon: BookOpen, roles: ['Owner', 'Admin', 'Manager', 'Technician', 'ReadOnly'] },
];

function SidebarContent({
  user,
  pathname,
  onLogout,
}: {
  user: { email: string; role: string; fullName?: string } | null;
  pathname: string;
  onLogout: () => void;
}) {
  const { hasRole } = useAuth();
  const visibleItems = NAV_ITEMS.filter((item) => hasRole(item.roles));

  return (
    <div className="flex h-full flex-col bg-white border-r border-slate-200">
      <div className="p-4 border-b border-slate-200">
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-lg bg-brand-600 flex items-center justify-center">
            <span className="text-white font-bold text-sm">B</span>
          </div>
          <span className="font-bold text-slate-900">BakımSuite</span>
        </div>
      </div>

      <nav className="flex-1 p-3 space-y-0.5 overflow-y-auto">
        {visibleItems.map((item) => {
          const Icon = item.icon;
          const isActive = pathname.startsWith(item.href);
          return (
            <Link
              key={item.href}
              href={item.href}
              className={clsx(
                'sidebar-link',
                isActive && 'sidebar-link-active'
              )}
            >
              <Icon size={16} />
              {item.label}
            </Link>
          );
        })}
      </nav>

      <div className="border-t border-slate-200 p-3">
        {hasRole(['Owner', 'Admin']) && (
          <Link href="/settings/business" className={clsx('sidebar-link mb-1', pathname.startsWith('/settings') && 'sidebar-link-active')}>
            <Settings size={16} />
            Ayarlar
          </Link>
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
  const { user, isAuthenticated, isLoading, logout } = useAuth();
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

  return (
    <div className="flex h-screen bg-slate-50 overflow-hidden">
      {/* Desktop Sidebar */}
      <div className="hidden lg:flex lg:w-64 lg:shrink-0 lg:flex-col">
        <SidebarContent user={user} pathname={pathname} onLogout={handleLogout} />
      </div>

      {/* Mobile Sidebar Overlay */}
      {sidebarOpen && (
        <div className="fixed inset-0 z-40 lg:hidden">
          <div className="fixed inset-0 bg-slate-900/50" onClick={() => setSidebarOpen(false)} />
          <div className="fixed inset-y-0 left-0 z-50 w-64">
            <SidebarContent user={user} pathname={pathname} onLogout={handleLogout} />
          </div>
        </div>
      )}

      {/* Main Content */}
      <div className="flex flex-1 flex-col overflow-hidden">
        {/* Topbar */}
        <header className="shrink-0 bg-white border-b border-slate-200 px-4 py-3 flex items-center gap-3 lg:px-6">
          <button
            onClick={() => setSidebarOpen(true)}
            className="lg:hidden p-1.5 rounded-lg text-slate-500 hover:bg-slate-100"
          >
            <Menu size={20} />
          </button>
          <Breadcrumbs items={getBreadcrumbs(pathname ?? '')} className="hidden md:flex" />
          <div className="flex-1" />
          <div className="flex items-center gap-2">
            <span className="text-sm text-slate-600 hidden sm:block">{user?.email}</span>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 overflow-auto">
          <div className="p-4 pb-16 lg:p-6 lg:pb-6">{children}</div>
        </main>
      </div>

      {/* Mobile Bottom Navigation */}
      <nav className="fixed bottom-0 left-0 right-0 z-30 bg-white border-t border-slate-200 flex lg:hidden">
        {BOTTOM_NAV_ITEMS.map((item) => {
          const Icon = item.icon;
          const isActive = (pathname ?? '').startsWith(item.href);
          return (
            <Link
              key={item.href}
              href={item.href}
              className={clsx(
                'flex flex-1 flex-col items-center py-2 text-xs gap-1',
                isActive ? 'text-brand-600' : 'text-slate-500'
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

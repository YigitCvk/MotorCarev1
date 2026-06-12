import { ALL_ROLES, isUserRole, type UserRole } from '@/shared/constants/roles';

export const ROLE_CAPABILITIES = {
  userManagement: ['Owner', 'Admin'],
  customerRead: ALL_ROLES,
  customerWrite: ['Owner', 'Admin', 'Receptionist'],
  serviceOrderRead: ['Owner', 'Admin', 'Receptionist', 'Technician', 'Accountant', 'ReadOnly', 'Manager'],
  serviceOrderWrite: ['Owner', 'Admin', 'Receptionist', 'Technician'],
  serviceOrderPayments: ['Owner', 'Admin', 'Receptionist', 'Accountant'],
  inspectionRead: ['Owner', 'Admin', 'Receptionist', 'Inspector', 'ReadOnly', 'Manager'],
  inspectionWrite: ['Owner', 'Admin', 'Inspector'],
  inventoryRead: ['Owner', 'Admin', 'Receptionist', 'Technician', 'ReadOnly', 'Manager'],
  inventoryWrite: ['Owner', 'Admin', 'Receptionist'],
  dashboardRead: ['Owner', 'Admin', 'Receptionist', 'Accountant', 'ReadOnly', 'Manager'],
  importOperations: ['Owner', 'Admin'],
} as const satisfies Record<string, readonly UserRole[]>;

type Capability = keyof typeof ROLE_CAPABILITIES;
type RoleValue = string | null | undefined;

export function hasCapability(role: RoleValue, capability: Capability): boolean {
  return isUserRole(role) && (ROLE_CAPABILITIES[capability] as readonly UserRole[]).includes(role);
}

export const canViewDashboard = (role: RoleValue): boolean => hasCapability(role, 'dashboardRead');
export const canViewCustomers = (role: RoleValue): boolean => hasCapability(role, 'customerRead');
export const canCreateCustomer = (role: RoleValue): boolean => hasCapability(role, 'customerWrite');
export const canEditCustomer = canCreateCustomer;
export const canCreateVehicle = canCreateCustomer;
export const canEditVehicle = canCreateCustomer;

export const canViewAppointments = (role: RoleValue): boolean => hasCapability(role, 'serviceOrderRead');
export const canCreateAppointment = (role: RoleValue): boolean => hasCapability(role, 'serviceOrderWrite');
export const canEditAppointment = canCreateAppointment;

export const canViewServiceOrders = (role: RoleValue): boolean => hasCapability(role, 'serviceOrderRead');
export const canCreateServiceOrder = (role: RoleValue): boolean => hasCapability(role, 'serviceOrderWrite');
export const canEditServiceOrder = canCreateServiceOrder;
export const canAddPayment = (role: RoleValue): boolean => hasCapability(role, 'serviceOrderPayments');

export const canViewInspections = (role: RoleValue): boolean => hasCapability(role, 'inspectionRead');
export const canManageInspection = (role: RoleValue): boolean => hasCapability(role, 'inspectionWrite');

export const canViewInventory = (role: RoleValue): boolean => hasCapability(role, 'inventoryRead');
export const canManageInventory = (role: RoleValue): boolean => hasCapability(role, 'inventoryWrite');
export const canViewServiceCatalog = canViewCustomers;
export const canManageServiceCatalog = canCreateCustomer;

export const canManageUsers = (role: RoleValue): boolean => hasCapability(role, 'userManagement');
export const canManageImports = (role: RoleValue): boolean => hasCapability(role, 'importOperations');
export const canManageBusinessSettings = (role: RoleValue): boolean => role === 'Owner';
export const canViewSecuritySettings = (role: RoleValue): boolean => isUserRole(role);

export function canAccessAppPath(role: RoleValue, pathname: string): boolean {
  if (!pathname) return true;

  if (pathname.startsWith('/settings/business')) return canManageBusinessSettings(role);
  if (pathname.startsWith('/settings/users')) return canManageUsers(role);
  if (pathname.startsWith('/settings/security')) return canViewSecuritySettings(role);
  if (pathname.startsWith('/imports')) return canManageImports(role);

  if (pathname === '/customers/new' || pathname === '/customers/create') return canCreateCustomer(role);
  if (/^\/customers\/[^/]+\/(edit|vehicles\/new)$/.test(pathname)) return canEditCustomer(role);
  if (pathname.startsWith('/customers')) return canViewCustomers(role);

  if (pathname === '/vehicles/new' || /^\/vehicles\/[^/]+\/edit$/.test(pathname)) return canCreateVehicle(role);
  if (pathname.startsWith('/vehicles')) return canViewCustomers(role);

  if (pathname === '/appointments/new' || /^\/appointments\/[^/]+\/edit$/.test(pathname)) {
    return canCreateAppointment(role);
  }
  if (pathname.startsWith('/appointments')) return canViewAppointments(role);

  if (pathname === '/service-orders/new') return canCreateServiceOrder(role);
  if (pathname.startsWith('/service-orders')) return canViewServiceOrders(role);

  if (pathname === '/inspections/new' || /^\/inspections\/[^/]+\/edit$/.test(pathname)) {
    return canManageInspection(role);
  }
  if (pathname.startsWith('/inspections')) return canViewInspections(role);

  if (
    pathname === '/inventory/new' ||
    pathname === '/inventory/create' ||
    /^\/inventory\/[^/]+\/edit$/.test(pathname)
  ) {
    return canManageInventory(role);
  }
  if (pathname.startsWith('/inventory')) return canViewInventory(role);

  if (
    pathname === '/service-catalog/new' ||
    pathname === '/service-catalog/create' ||
    /^\/service-catalog\/[^/]+\/edit$/.test(pathname)
  ) {
    return canManageServiceCatalog(role);
  }
  if (pathname.startsWith('/service-catalog')) return canViewServiceCatalog(role);
  if (pathname.startsWith('/dashboard')) return canViewDashboard(role);

  return isUserRole(role);
}

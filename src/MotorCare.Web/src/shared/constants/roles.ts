export const ALL_ROLES = [
  'Owner',
  'Admin',
  'Receptionist',
  'Technician',
  'Manager',
  'Inspector',
  'Accountant',
  'ReadOnly',
] as const;

export type UserRole = (typeof ALL_ROLES)[number];

export const ADMIN_ROLES = ['Owner', 'Admin'] as const satisfies readonly UserRole[];
export const MANAGER_ROLES = ['Owner', 'Admin', 'Manager'] as const satisfies readonly UserRole[];
export const STAFF_ROLES = [
  'Owner',
  'Admin',
  'Receptionist',
  'Manager',
  'Technician',
  'Inspector',
  'Accountant',
] as const satisfies readonly UserRole[];

export const ROLE_LABELS: Record<UserRole, string> = {
  Owner: 'Sahip',
  Admin: 'Yönetici',
  Receptionist: 'Resepsiyon',
  Manager: 'Müdür',
  Technician: 'Teknisyen',
  Inspector: 'Eksper',
  Accountant: 'Muhasebe',
  ReadOnly: 'Salt Okuma',
};

export const ROLE_VALUES: Record<UserRole, number> = {
  Owner: 1,
  Admin: 2,
  Receptionist: 3,
  Technician: 4,
  Manager: 5,
  Inspector: 6,
  Accountant: 7,
  ReadOnly: 8,
};

export const INVITABLE_ROLES = [
  'Admin',
  'Receptionist',
  'Manager',
  'Technician',
  'Inspector',
  'Accountant',
  'ReadOnly',
] as const satisfies readonly UserRole[];

export function isUserRole(role: string | null | undefined): role is UserRole {
  return Boolean(role && (ALL_ROLES as readonly string[]).includes(role));
}

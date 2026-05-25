// src/shared/constants/roles.ts

export const ADMIN_ROLES = ['Owner', 'Admin'] as const;
export const MANAGER_ROLES = ['Owner', 'Admin', 'Manager'] as const;
export const STAFF_ROLES = ['Owner', 'Admin', 'Manager', 'Technician', 'Inspector', 'Accountant'] as const;
export const ALL_ROLES = ['Owner', 'Admin', 'Manager', 'Technician', 'Inspector', 'Accountant', 'ReadOnly'] as const;

export const ROLE_LABELS: Record<string, string> = {
  Owner: 'Sahip',
  Admin: 'Yönetici',
  Manager: 'Müdür',
  Technician: 'Teknisyen',
  Inspector: 'Eksper',
  Accountant: 'Muhasebe',
  ReadOnly: 'Salt Okuma',
};

export const ROLE_VALUES: Record<string, number> = {
  Owner: 1,
  Admin: 2,
  Receptionist: 3,
  Technician: 4,
  Manager: 5,
  Inspector: 6,
  Accountant: 7,
  ReadOnly: 8,
};

export const INVITABLE_ROLES = ['Admin', 'Manager', 'Technician', 'Inspector', 'Accountant', 'ReadOnly'] as const;

const PACKAGE_TYPE_VALUES = {
  MechanicalAndRunningGear: 1,
  BodyAndFairing: 2,
  ObdAndElectrical: 3,
  Full: 4,
} as const;

const STATUS_VALUES = {
  Draft: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4,
} as const;

const RESULT_VALUES = {
  NotChecked: 0,
  Good: 1,
  Medium: 2,
  Bad: 3,
  NotAvailable: 4,
  Exists: 5,
  NotExists: 6,
  Damaged: 7,
  Painted: 8,
  Original: 9,
  Changed: 10,
  Scratched: 11,
  Missing: 12,
} as const;

const CATEGORY_VALUES = {
  MechanicalAndRunningGear: 1,
  BodyAndFairing: 2,
  ObdAndElectrical: 3,
  TestRide: 4,
  General: 5,
} as const;

export type InspectionPackageType = keyof typeof PACKAGE_TYPE_VALUES;
export type InspectionStatus = keyof typeof STATUS_VALUES;
export type InspectionResult = keyof typeof RESULT_VALUES;
export type InspectionCategory = keyof typeof CATEGORY_VALUES;

function enumNameFromApi<T extends string>(
  value: unknown,
  values: Record<T, number>,
  fallback: T,
): T {
  if (typeof value === 'string' && value in values) return value as T;
  const entry = Object.entries(values).find(([, apiValue]) => apiValue === value);
  return (entry?.[0] as T | undefined) ?? fallback;
}

export function inspectionPackageTypeToApi(value: string): number {
  return PACKAGE_TYPE_VALUES[value as InspectionPackageType] ?? PACKAGE_TYPE_VALUES.Full;
}

export function inspectionPackageTypeFromApi(value: unknown): InspectionPackageType {
  return enumNameFromApi(value, PACKAGE_TYPE_VALUES, 'Full');
}

export function inspectionStatusFromApi(value: unknown): InspectionStatus {
  return enumNameFromApi(value, STATUS_VALUES, 'Draft');
}

export function inspectionResultToApi(value: string): number {
  return RESULT_VALUES[value as InspectionResult] ?? RESULT_VALUES.NotChecked;
}

export function inspectionResultFromApi(value: unknown): InspectionResult {
  return enumNameFromApi(value, RESULT_VALUES, 'NotChecked');
}

export function inspectionCategoryFromApi(value: unknown): InspectionCategory {
  return enumNameFromApi(value, CATEGORY_VALUES, 'General');
}

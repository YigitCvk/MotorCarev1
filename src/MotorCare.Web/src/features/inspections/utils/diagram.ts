import type { DamageZone, VehicleDiagramKind } from '@/features/inspections/components';

export interface InspectionDiagramItem {
  category: string;
  categoryText: string;
  result: string;
  resultText?: string | null;
}

const ISSUE_RESULTS = new Set([
  'Bad',
  'Damaged',
  'Scratched',
  'Missing',
  'Fail',
  'Issue',
  'Başarısız',
  'Sorunlu',
]);

export function buildDamageZones(items: InspectionDiagramItem[] = []): DamageZone[] {
  const categoryMap = new Map<string, { category: string; hasIssue: boolean }>();

  for (const item of items) {
    const key = item.categoryText || item.category;
    const existing = categoryMap.get(key);
    const isIssue = ISSUE_RESULTS.has(item.result) || ISSUE_RESULTS.has(item.resultText ?? '');

    if (!existing) {
      categoryMap.set(key, { category: item.category, hasIssue: isIssue });
    } else if (isIssue) {
      existing.hasIssue = true;
    }
  }

  return Array.from(categoryMap.entries()).map(([label, value]) => ({
    id: value.category,
    label,
    hasIssue: value.hasIssue,
  }));
}

export function resolveVehicleDiagramKind(vehicleType?: string | null): VehicleDiagramKind {
  const value = (vehicleType ?? '').toLocaleLowerCase('tr-TR');

  if (
    value.includes('motorcycle') ||
    value.includes('motorbike') ||
    value.includes('motosiklet') ||
    value.includes('scooter')
  ) {
    return 'motorcycle';
  }

  return 'car';
}

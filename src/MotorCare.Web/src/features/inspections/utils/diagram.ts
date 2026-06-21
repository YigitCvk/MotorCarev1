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

const MOTORCYCLE_BASE_ZONES = ['Ön', 'Arka', 'Sol', 'Sağ', 'Motor'] as const;

function isIssueResult(item: InspectionDiagramItem): boolean {
  return ISSUE_RESULTS.has(item.result) || ISSUE_RESULTS.has(item.resultText ?? '');
}

function motorcycleZonesForItem(item: InspectionDiagramItem): string[] {
  const value = `${item.category} ${item.categoryText}`.toLocaleLowerCase('tr-TR');

  if (
    value.includes('mechanical') ||
    value.includes('running') ||
    value.includes('motor') ||
    value.includes('mekanik') ||
    value.includes('yürüyen')
  ) {
    return ['Motor', 'Ön', 'Arka'];
  }

  if (
    value.includes('body') ||
    value.includes('fairing') ||
    value.includes('cosmetic') ||
    value.includes('karenaj') ||
    value.includes('kaporta') ||
    value.includes('kozmetik')
  ) {
    return ['Sol', 'Sağ'];
  }

  if (value.includes('obd') || value.includes('electrical') || value.includes('elektrik')) {
    return ['Motor'];
  }

  if (value.includes('test') || value.includes('ride') || value.includes('sürüş')) {
    return ['Ön', 'Arka'];
  }

  if (value.includes('front') || value.includes('ön')) return ['Ön'];
  if (value.includes('rear') || value.includes('arka')) return ['Arka'];
  if (value.includes('left') || value.includes('sol')) return ['Sol'];
  if (value.includes('right') || value.includes('sağ')) return ['Sağ'];

  return ['Motor'];
}

export function buildDamageZones(
  items: InspectionDiagramItem[] = [],
  vehicleType: VehicleDiagramKind = 'car'
): DamageZone[] {
  if (vehicleType === 'motorcycle') {
    const zones = new Map<string, boolean>(MOTORCYCLE_BASE_ZONES.map((zone) => [zone, false]));

    for (const item of items) {
      const hasIssue = isIssueResult(item);
      for (const label of motorcycleZonesForItem(item)) {
        zones.set(label, Boolean(zones.get(label)) || hasIssue);
      }
    }

    return Array.from(zones.entries()).map(([label, hasIssue]) => ({
      id: label,
      label,
      hasIssue,
    }));
  }

  const categoryMap = new Map<string, { category: string; hasIssue: boolean }>();

  for (const item of items) {
    const key = item.categoryText || item.category;
    const existing = categoryMap.get(key);
    const hasIssue = isIssueResult(item);

    if (!existing) {
      categoryMap.set(key, { category: item.category, hasIssue });
    } else if (hasIssue) {
      existing.hasIssue = true;
    }
  }

  return Array.from(categoryMap.entries()).map(([label, value]) => ({
    id: value.category,
    label,
    hasIssue: value.hasIssue,
  }));
}

export function resolveVehicleDiagramKind(
  vehicleType?: string | null,
  fallback: VehicleDiagramKind = 'car'
): VehicleDiagramKind {
  const value = (vehicleType ?? '').toLocaleLowerCase('tr-TR');

  if (
    value.includes('motorcycle') ||
    value.includes('motorbike') ||
    value.includes('motosiklet') ||
    value.includes('scooter')
  ) {
    return 'motorcycle';
  }

  if (value.includes('car') || value.includes('auto') || value.includes('otomobil') || value.includes('araç')) {
    return 'car';
  }

  return fallback;
}

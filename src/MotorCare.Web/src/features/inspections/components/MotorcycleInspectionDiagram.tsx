'use client';

import { useState } from 'react';
import {
  motorcycleTypeFromApi,
  motorcycleTypeImages,
  motorcycleTypeLabels,
} from '@/shared/types/motorcycle-types';
import type { DamageZone } from './VehicleDiagram';

interface MotorcycleInspectionDiagramProps {
  motorcycleType?: string | null;
  zones?: DamageZone[];
  printable?: boolean;
  className?: string;
}

export function MotorcycleInspectionDiagram({
  motorcycleType,
  zones = [],
  printable = false,
  className,
}: MotorcycleInspectionDiagramProps) {
  const [imgError, setImgError] = useState(false);

  const type = motorcycleTypeFromApi(motorcycleType) ?? 'other';
  const label = motorcycleTypeLabels[type];
  const imgSrc = imgError
    ? '/assets/vehicles/motorcycles/default-motorcycle.svg'
    : motorcycleTypeImages[type];

  return (
    <div className={className}>
      <p className="mb-2 text-sm font-semibold text-slate-600">
        Motosiklet Diyagramı — {label}
      </p>

      <div className="flex justify-center rounded-lg border border-slate-200 bg-slate-50 p-4">
        <img
          src={imgSrc}
          alt={label}
          className="h-44 w-auto max-w-full object-contain"
          style={printable ? { printColorAdjust: 'exact', WebkitPrintColorAdjust: 'exact' } as React.CSSProperties : undefined}
          onError={() => setImgError(true)}
        />
      </div>

      {zones.length > 0 && (
        <div className="mt-3 flex flex-wrap gap-2">
          {zones.map((zone) => (
            <span
              key={zone.id}
              className={[
                'inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-xs font-semibold',
                zone.hasIssue
                  ? 'border-red-300 bg-red-50 text-red-700'
                  : 'border-green-300 bg-green-50 text-green-700',
              ].join(' ')}
            >
              <span
                className={[
                  'h-2 w-2 rounded-full',
                  zone.hasIssue ? 'bg-red-500' : 'bg-green-500',
                ].join(' ')}
              />
              {zone.label}
            </span>
          ))}
        </div>
      )}

      {zones.length > 0 && (
        <div className="mt-2 flex flex-wrap gap-3 text-xs text-slate-500">
          <span className="flex items-center gap-1">
            <span className="h-2.5 w-2.5 rounded-full bg-red-500" />
            Sorun var
          </span>
          <span className="flex items-center gap-1">
            <span className="h-2.5 w-2.5 rounded-full bg-green-500" />
            Sorun yok
          </span>
        </div>
      )}
    </div>
  );
}

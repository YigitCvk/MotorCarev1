export interface DamageZone {
  id: string;
  label: string;
  hasIssue: boolean;
}

export type VehicleDiagramKind = 'car' | 'motorcycle';

export interface VehicleDiagramProps {
  zones: DamageZone[];
  vehicleType?: VehicleDiagramKind;
}

type RectSpec = { x: number; y: number; w: number; h: number; labelX: number; labelY: number };

const CAR_ZONE_RECTS: Record<string, RectSpec> = {
  Ön: { x: 80, y: 22, w: 240, h: 40, labelX: 200, labelY: 47 },
  Arka: { x: 80, y: 138, w: 240, h: 40, labelX: 200, labelY: 163 },
  Sol: { x: 60, y: 62, w: 50, h: 76, labelX: 85, labelY: 103 },
  Sağ: { x: 290, y: 62, w: 50, h: 76, labelX: 315, labelY: 103 },
  Motor: { x: 155, y: 70, w: 90, h: 60, labelX: 200, labelY: 103 },
};

const MOTORCYCLE_ZONE_RECTS: Record<string, RectSpec> = {
  Ön: { x: 265, y: 82, w: 70, h: 48, labelX: 300, labelY: 106 },
  Arka: { x: 65, y: 82, w: 70, h: 48, labelX: 100, labelY: 106 },
  Sol: { x: 142, y: 50, w: 116, h: 40, labelX: 200, labelY: 72 },
  Sağ: { x: 142, y: 130, w: 116, h: 40, labelX: 200, labelY: 152 },
  Motor: { x: 165, y: 88, w: 70, h: 36, labelX: 200, labelY: 107 },
};

function ZoneOverlay({
  zone,
  fallbackIndex,
  rects,
}: {
  zone: DamageZone;
  fallbackIndex: number;
  rects: Record<string, RectSpec>;
}) {
  const fill = zone.hasIssue ? '#fecaca' : '#dcfce7';
  const stroke = zone.hasIssue ? '#ef4444' : '#22c55e';
  const textFill = zone.hasIssue ? '#b91c1c' : '#15803d';
  const rect = rects[zone.label];

  if (rect) {
    return (
      <g>
        <rect
          x={rect.x}
          y={rect.y}
          width={rect.w}
          height={rect.h}
          rx={6}
          ry={6}
          fill={fill}
          stroke={stroke}
          strokeWidth={1.5}
          fillOpacity={0.85}
        />
        <text
          x={rect.labelX}
          y={rect.labelY}
          textAnchor="middle"
          dominantBaseline="middle"
          fontSize={11}
          fontWeight={600}
          fill={textFill}
          fontFamily="system-ui, sans-serif"
        >
          {zone.label}
        </text>
      </g>
    );
  }

  const px = 10 + fallbackIndex * 90;
  const py = 190;

  return (
    <g>
      <rect
        x={px}
        y={py - 10}
        width={80}
        height={18}
        rx={9}
        fill={fill}
        stroke={stroke}
        strokeWidth={1}
        fillOpacity={0.85}
      />
      <text
        x={px + 40}
        y={py + 1}
        textAnchor="middle"
        dominantBaseline="middle"
        fontSize={9}
        fontWeight={600}
        fill={textFill}
        fontFamily="system-ui, sans-serif"
      >
        {zone.label}
      </text>
    </g>
  );
}

function Legend() {
  return (
    <g transform="translate(10,205)">
      <rect x={0} y={0} width={12} height={12} rx={3} fill="#fecaca" stroke="#ef4444" strokeWidth={1} />
      <text x={16} y={10} fontSize={10} fill="#64748b" fontFamily="system-ui, sans-serif">
        Sorun var
      </text>
      <rect x={80} y={0} width={12} height={12} rx={3} fill="#dcfce7" stroke="#22c55e" strokeWidth={1} />
      <text x={96} y={10} fontSize={10} fill="#64748b" fontFamily="system-ui, sans-serif">
        Sorun yok
      </text>
    </g>
  );
}

function CarDiagram({ zones }: { zones: DamageZone[] }) {
  let fallbackIdx = 0;

  return (
    <svg viewBox="0 0 400 220" xmlns="http://www.w3.org/2000/svg" className="h-auto w-full" aria-label="Araç diyagramı">
      <rect x={60} y={20} width={280} height={160} rx={24} ry={24} fill="#f1f5f9" stroke="#94a3b8" strokeWidth={2} />
      <rect x={100} y={10} width={200} height={20} rx={8} ry={8} fill="#e2e8f0" stroke="#94a3b8" strokeWidth={1.5} />
      <rect x={100} y={170} width={200} height={20} rx={8} ry={8} fill="#e2e8f0" stroke="#94a3b8" strokeWidth={1.5} />
      <rect x={130} y={42} width={140} height={36} rx={12} fill="#dbeafe" stroke="#93c5fd" strokeWidth={1.5} />
      <rect x={130} y={122} width={140} height={36} rx={12} fill="#dbeafe" stroke="#93c5fd" strokeWidth={1.5} />
      <ellipse cx={78} cy={48} rx={16} ry={22} fill="#cbd5e1" stroke="#64748b" strokeWidth={2} />
      <ellipse cx={322} cy={48} rx={16} ry={22} fill="#cbd5e1" stroke="#64748b" strokeWidth={2} />
      <ellipse cx={78} cy={152} rx={16} ry={22} fill="#cbd5e1" stroke="#64748b" strokeWidth={2} />
      <ellipse cx={322} cy={152} rx={16} ry={22} fill="#cbd5e1" stroke="#64748b" strokeWidth={2} />
      <line x1={200} y1={25} x2={200} y2={175} stroke="#cbd5e1" strokeWidth={1} strokeDasharray="4,3" />
      {zones.map((zone) => {
        const fi = zone.label in CAR_ZONE_RECTS ? 0 : fallbackIdx++;
        return <ZoneOverlay key={zone.id} zone={zone} fallbackIndex={fi} rects={CAR_ZONE_RECTS} />;
      })}
      <Legend />
    </svg>
  );
}

export function MotorcycleDiagram({ zones }: { zones: DamageZone[] }) {
  let fallbackIdx = 0;

  return (
    <svg
      viewBox="0 0 400 220"
      xmlns="http://www.w3.org/2000/svg"
      className="h-auto w-full"
      aria-label="Motosiklet diyagramı"
    >
      <ellipse cx={86} cy={110} rx={30} ry={44} fill="#e2e8f0" stroke="#64748b" strokeWidth={3} />
      <ellipse cx={314} cy={110} rx={30} ry={44} fill="#e2e8f0" stroke="#64748b" strokeWidth={3} />
      <circle cx={86} cy={110} r={10} fill="#94a3b8" />
      <circle cx={314} cy={110} r={10} fill="#94a3b8" />
      <path d="M105 110 L170 74 L230 110 L170 146 Z" fill="#f1f5f9" stroke="#94a3b8" strokeWidth={2.5} />
      <path d="M170 74 L205 74 L240 110" fill="none" stroke="#64748b" strokeWidth={4} strokeLinecap="round" />
      <path d="M230 110 L286 92 M230 110 L286 128" fill="none" stroke="#94a3b8" strokeWidth={3} strokeLinecap="round" />
      <path d="M270 80 L322 54 M270 80 L336 76" fill="none" stroke="#64748b" strokeWidth={4} strokeLinecap="round" />
      <rect x={168} y={94} width={66} height={30} rx={10} fill="#cbd5e1" stroke="#64748b" strokeWidth={1.5} />
      <rect x={150} y={55} width={88} height={20} rx={10} fill="#e2e8f0" stroke="#94a3b8" strokeWidth={1.5} />
      <path d="M132 136 C160 178 242 178 268 136" fill="none" stroke="#cbd5e1" strokeWidth={8} strokeLinecap="round" />
      {zones.map((zone) => {
        const fi = zone.label in MOTORCYCLE_ZONE_RECTS ? 0 : fallbackIdx++;
        return <ZoneOverlay key={zone.id} zone={zone} fallbackIndex={fi} rects={MOTORCYCLE_ZONE_RECTS} />;
      })}
      <Legend />
    </svg>
  );
}

export function VehicleDiagram({ zones, vehicleType = 'car' }: VehicleDiagramProps) {
  return (
    <div className="mx-auto w-full max-w-md overflow-hidden">
      {vehicleType === 'motorcycle' ? <MotorcycleDiagram zones={zones} /> : <CarDiagram zones={zones} />}
    </div>
  );
}

export default VehicleDiagram;

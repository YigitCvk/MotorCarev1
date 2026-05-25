export interface DamageZone {
  id: string;
  label: string;
  hasIssue: boolean;
}

export interface VehicleDiagramProps {
  zones: DamageZone[];
}

// Zone definitions: each zone has a rect area in the 400x200 viewBox
// The vehicle body is roughly x=60,y=20 w=280,h=160
// Front = top strip, Rear = bottom strip, Left = left strip, Right = right strip, Engine/Motor = center
const ZONE_RECTS: Record<string, { x: number; y: number; w: number; h: number; labelX: number; labelY: number }> = {
  Ön:      { x: 80,  y: 22,  w: 240, h: 40,  labelX: 200, labelY: 47  },
  Arka:    { x: 80,  y: 138, w: 240, h: 40,  labelX: 200, labelY: 163 },
  Sol:     { x: 60,  y: 62,  w: 50,  h: 76,  labelX: 85,  labelY: 103 },
  Sağ:     { x: 290, y: 62,  w: 50,  h: 76,  labelX: 315, labelY: 103 },
  Motor:   { x: 155, y: 70,  w: 90,  h: 60,  labelX: 200, labelY: 103 },
};

// Fallback rect for zones not in ZONE_RECTS (placed as a small indicator at the bottom)
function ZoneRect({
  zone,
  fallbackIndex,
}: {
  zone: DamageZone;
  fallbackIndex: number;
}) {
  const fill = zone.hasIssue ? '#fecaca' : '#dcfce7'; // red-200 / green-100
  const stroke = zone.hasIssue ? '#ef4444' : '#22c55e';
  const textFill = zone.hasIssue ? '#b91c1c' : '#15803d';

  const rect = ZONE_RECTS[zone.label];

  if (rect) {
    return (
      <g key={zone.id}>
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
          fontWeight="600"
          fill={textFill}
          fontFamily="system-ui, sans-serif"
        >
          {zone.label}
        </text>
      </g>
    );
  }

  // Fallback: render as a small pill below the diagram
  const px = 10 + fallbackIndex * 90;
  const py = 190;
  return (
    <g key={zone.id}>
      <rect x={px} y={py - 10} width={80} height={18} rx={9} fill={fill} stroke={stroke} strokeWidth={1} fillOpacity={0.85} />
      <text x={px + 40} y={py + 1} textAnchor="middle" dominantBaseline="middle" fontSize={9} fontWeight="600" fill={textFill} fontFamily="system-ui, sans-serif">
        {zone.label}
      </text>
    </g>
  );
}

export function VehicleDiagram({ zones }: VehicleDiagramProps) {
  // Track fallback index separately
  let fallbackIdx = 0;

  return (
    <div className="w-full max-w-md mx-auto">
      <svg
        viewBox="0 0 400 220"
        xmlns="http://www.w3.org/2000/svg"
        className="w-full h-auto"
        aria-label="Araç hasar haritası"
      >
        {/* ---- Vehicle outline (top-down view) ---- */}
        {/* Body */}
        <rect x={60} y={20} width={280} height={160} rx={24} ry={24} fill="#f1f5f9" stroke="#94a3b8" strokeWidth={2} />

        {/* Front bumper */}
        <rect x={100} y={10} width={200} height={20} rx={8} ry={8} fill="#e2e8f0" stroke="#94a3b8" strokeWidth={1.5} />

        {/* Rear bumper */}
        <rect x={100} y={170} width={200} height={20} rx={8} ry={8} fill="#e2e8f0" stroke="#94a3b8" strokeWidth={1.5} />

        {/* Front forks / handlebar area (motorcycle-style) */}
        <rect x={170} y={4} width={60} height={12} rx={4} fill="#cbd5e1" stroke="#94a3b8" strokeWidth={1} />

        {/* Wheels */}
        {/* Front-left */}
        <ellipse cx={78} cy={48}  rx={16} ry={22} fill="#cbd5e1" stroke="#64748b" strokeWidth={2} />
        {/* Front-right */}
        <ellipse cx={322} cy={48}  rx={16} ry={22} fill="#cbd5e1" stroke="#64748b" strokeWidth={2} />
        {/* Rear-left */}
        <ellipse cx={78} cy={152} rx={16} ry={22} fill="#cbd5e1" stroke="#64748b" strokeWidth={2} />
        {/* Rear-right */}
        <ellipse cx={322} cy={152} rx={16} ry={22} fill="#cbd5e1" stroke="#64748b" strokeWidth={2} />

        {/* Wheel hub dots */}
        <circle cx={78}  cy={48}  r={5} fill="#94a3b8" />
        <circle cx={322} cy={48}  r={5} fill="#94a3b8" />
        <circle cx={78}  cy={152} r={5} fill="#94a3b8" />
        <circle cx={322} cy={152} r={5} fill="#94a3b8" />

        {/* Center spine line */}
        <line x1={200} y1={25} x2={200} y2={175} stroke="#cbd5e1" strokeWidth={1} strokeDasharray="4,3" />

        {/* ---- Zone overlays ---- */}
        {zones.map((zone) => {
          const isKnown = zone.label in ZONE_RECTS;
          const fi = isKnown ? 0 : fallbackIdx++;
          return <ZoneRect key={zone.id} zone={zone} fallbackIndex={fi} />;
        })}

        {/* ---- Legend ---- */}
        <g transform="translate(10,205)">
          <rect x={0} y={0} width={12} height={12} rx={3} fill="#fecaca" stroke="#ef4444" strokeWidth={1} />
          <text x={16} y={10} fontSize={10} fill="#64748b" fontFamily="system-ui, sans-serif">Sorun var</text>
          <rect x={80} y={0} width={12} height={12} rx={3} fill="#dcfce7" stroke="#22c55e" strokeWidth={1} />
          <text x={96} y={10} fontSize={10} fill="#64748b" fontFamily="system-ui, sans-serif">Sorun yok</text>
        </g>
      </svg>
    </div>
  );
}

export default VehicleDiagram;

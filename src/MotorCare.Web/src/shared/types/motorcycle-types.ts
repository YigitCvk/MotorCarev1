export type MotorcycleType =
  | 'scooter'
  | 'sport'
  | 'sport-touring'
  | 'touring'
  | 'adventure'
  | 'cruiser'
  | 'naked'
  | 'enduro'
  | 'commuter'
  | 'other';

export const MOTORCYCLE_TYPES: MotorcycleType[] = [
  'scooter',
  'sport',
  'sport-touring',
  'touring',
  'adventure',
  'cruiser',
  'naked',
  'enduro',
  'commuter',
  'other',
];

export const motorcycleTypeLabels: Record<MotorcycleType, string> = {
  scooter: 'Scooter',
  sport: 'Sport',
  'sport-touring': 'Sport Touring',
  touring: 'Touring',
  adventure: 'Adventure',
  cruiser: 'Cruiser',
  naked: 'Naked',
  enduro: 'Enduro',
  commuter: 'Şehir / Commuter',
  other: 'Diğer',
};

export const motorcycleTypeImages: Record<MotorcycleType, string> = {
  scooter: '/assets/vehicles/motorcycles/scooter.svg',
  sport: '/assets/vehicles/motorcycles/sport.svg',
  'sport-touring': '/assets/vehicles/motorcycles/sport-touring.svg',
  touring: '/assets/vehicles/motorcycles/touring.svg',
  adventure: '/assets/vehicles/motorcycles/adventure.svg',
  cruiser: '/assets/vehicles/motorcycles/cruiser.svg',
  naked: '/assets/vehicles/motorcycles/naked.svg',
  enduro: '/assets/vehicles/motorcycles/enduro.svg',
  commuter: '/assets/vehicles/motorcycles/commuter.svg',
  other: '/assets/vehicles/motorcycles/default-motorcycle.svg',
};

// Backend sends "SportTouring", frontend uses "sport-touring"
const API_TO_FRONTEND: Record<string, MotorcycleType> = {
  Scooter: 'scooter',
  Sport: 'sport',
  SportTouring: 'sport-touring',
  Touring: 'touring',
  Adventure: 'adventure',
  Cruiser: 'cruiser',
  Naked: 'naked',
  Enduro: 'enduro',
  Commuter: 'commuter',
  Other: 'other',
};

const FRONTEND_TO_API: Record<MotorcycleType, string> = {
  scooter: 'Scooter',
  sport: 'Sport',
  'sport-touring': 'SportTouring',
  touring: 'Touring',
  adventure: 'Adventure',
  cruiser: 'Cruiser',
  naked: 'Naked',
  enduro: 'Enduro',
  commuter: 'Commuter',
  other: 'Other',
};

export function motorcycleTypeFromApi(value: string | null | undefined): MotorcycleType | null {
  if (!value) return null;
  return API_TO_FRONTEND[value] ?? null;
}

export function motorcycleTypeToApi(type: MotorcycleType | null | undefined): string | null {
  if (!type) return null;
  return FRONTEND_TO_API[type] ?? null;
}

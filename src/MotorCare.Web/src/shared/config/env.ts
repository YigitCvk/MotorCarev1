export const appConfig = {
  appName: process.env.NEXT_PUBLIC_APP_NAME ?? 'GarajPass',
  publicAppUrl:
    process.env.NEXT_PUBLIC_PUBLIC_APP_URL ??
    process.env.NEXT_PUBLIC_APP_URL ??
    'http://localhost:3000',
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL ?? '',
  monthlyDashboardEnabled: process.env.NEXT_PUBLIC_ENABLE_MONTHLY_DASHBOARD === 'true',
} as const;

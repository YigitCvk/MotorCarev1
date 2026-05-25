export const appConfig = {
  appName: process.env.NEXT_PUBLIC_APP_NAME ?? 'BakımSuite',
  appUrl: process.env.NEXT_PUBLIC_APP_URL ?? 'http://localhost:3000',
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL ?? 'https://api.bakimsuite.com',
} as const;

// src/core/auth/storage.ts
// Storage adapter pattern: web uses localStorage, future mobile can swap.

export interface StorageAdapter {
  get(key: string): string | null;
  set(key: string, value: string): void;
  remove(key: string): void;
  clear(): void;
}

const KEYS = {
  ACCESS_TOKEN: 'mc.accessToken',
  REFRESH_TOKEN: 'mc.refreshToken',
  CURRENT_USER: 'mc.currentUser',
  CLIENT_KEY: 'mc.clientKey',
} as const;

function isBrowser(): boolean {
  return typeof window !== 'undefined';
}

export function getAccessToken(): string | null {
  if (!isBrowser()) return null;
  return localStorage.getItem(KEYS.ACCESS_TOKEN);
}

export function getRefreshToken(): string | null {
  if (!isBrowser()) return null;
  return localStorage.getItem(KEYS.REFRESH_TOKEN);
}

export function setTokens(accessToken: string, refreshToken: string): void {
  if (!isBrowser()) return;
  localStorage.setItem(KEYS.ACCESS_TOKEN, accessToken);
  localStorage.setItem(KEYS.REFRESH_TOKEN, refreshToken);
}

export function clearTokens(): void {
  if (!isBrowser()) return;
  localStorage.removeItem(KEYS.ACCESS_TOKEN);
  localStorage.removeItem(KEYS.REFRESH_TOKEN);
  localStorage.removeItem(KEYS.CURRENT_USER);
}

export function getCurrentUserFromStorage<T>(): T | null {
  if (!isBrowser()) return null;
  try {
    const raw = localStorage.getItem(KEYS.CURRENT_USER);
    return raw ? (JSON.parse(raw) as T) : null;
  } catch {
    return null;
  }
}

export function setCurrentUserInStorage<T>(user: T): void {
  if (!isBrowser()) return;
  localStorage.setItem(KEYS.CURRENT_USER, JSON.stringify(user));
}

export function getClientKey(): string {
  if (!isBrowser()) return '';
  let key = localStorage.getItem(KEYS.CLIENT_KEY);
  if (!key) {
    key = createClientKey();
    localStorage.setItem(KEYS.CLIENT_KEY, key);
  }
  return key;
}

function createClientKey(): string {
  const webCrypto = window.crypto;

  if (typeof webCrypto?.randomUUID === 'function') {
    return webCrypto.randomUUID();
  }

  if (typeof webCrypto?.getRandomValues === 'function') {
    const bytes = new Uint8Array(16);
    webCrypto.getRandomValues(bytes);
    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;
    const hex = Array.from(bytes, (byte) => byte.toString(16).padStart(2, '0'));
    return [
      hex.slice(0, 4).join(''),
      hex.slice(4, 6).join(''),
      hex.slice(6, 8).join(''),
      hex.slice(8, 10).join(''),
      hex.slice(10, 16).join(''),
    ].join('-');
  }

  return `client-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 12)}`;
}

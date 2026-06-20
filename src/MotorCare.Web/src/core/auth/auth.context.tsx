'use client';

// src/core/auth/auth.context.tsx

import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { authService } from '@/core/auth/auth.service';
import { clearTokens, getAccessToken, getCurrentUserFromStorage, getRefreshToken } from '@/core/auth/storage';
import type { CurrentUser, LoginRequest, LoginResponse } from '@/shared/types/api.types';

interface AuthContextValue {
  user: CurrentUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (request: LoginRequest) => Promise<LoginResponse>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
  hasRole: (roles: string[]) => boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(() => getCurrentUserFromStorage<CurrentUser>());
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = getAccessToken();
    if (!token) {
      setUser(null);
      setIsLoading(false);
      return;
    }

    authService.loadCurrentUser()
      .then((u) => {
        if (u) setUser(u);
        else {
          clearTokens();
          setUser(null);
        }
      })
      .finally(() => setIsLoading(false));
  }, []);

  const login = useCallback(async (request: LoginRequest): Promise<LoginResponse> => {
    const response = await authService.login(request);
    if (!response.requiresTwoFactor) {
      setUser(getCurrentUserFromStorage<CurrentUser>());
    }
    return response;
  }, []);

  const logout = useCallback(async (): Promise<void> => {
    const refreshToken = getRefreshToken();
    if (refreshToken) {
      await authService.logout(refreshToken).catch(() => undefined);
    } else {
      clearTokens();
    }
    setUser(null);
  }, []);

  const refreshUser = useCallback(async (): Promise<void> => {
    const u = await authService.loadCurrentUser();
    setUser(u);
  }, []);

  const hasRole = useCallback(
    (roles: string[]): boolean => {
      if (roles.length === 0) return true;
      return !!user?.role && roles.includes(user.role);
    },
    [user],
  );

  return (
    <AuthContext.Provider
      value={{ user, isAuthenticated: !!user, isLoading, login, logout, refreshUser, hasRole }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}

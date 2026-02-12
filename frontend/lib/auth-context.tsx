"use client";

import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { apiRequest } from "@/lib/api-client";
import { getApiBaseUrl } from "@/lib/api-client";
import type { ApiAuthResponse, ApiAuthSessionResponse, ApiUserRole } from "@/lib/api-types";

interface User {
  id: string;
  name: string;
  email: string;
  phone?: string | null;
  department?: string | null;
  role: "client" | "engineer" | "admin";
  avatar?: string | null;
  isSupervisor?: boolean;
}

type AuthMode = "auto" | "lan" | "external";

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (email: string, password: string) => Promise<boolean>;
  register: (userData: {
    name: string;
    email: string;
    phone: string;
    department: string;
    role: string;
    password: string;
  }) => Promise<boolean>;
  logout: () => void;
  updateProfile: (updates: Partial<Omit<User, "id" | "role">>) => Promise<boolean>;
  changePassword: (currentPassword: string, newPassword: string, confirmNewPassword: string) => Promise<boolean>;
  refreshSession: () => Promise<void>;
  startExternalLogin: () => Promise<void>;
  isLoading: boolean;
  authMode: AuthMode;
  isExternalMode: boolean;
  isLanMode: boolean;
}

const TOKEN_STORAGE_KEY = "ticketing.auth.token";
const USER_STORAGE_KEY = "ticketing.auth.user";
const COOKIE_SESSION_TOKEN = "__cookie_session__";
const AUTH_MODE = (process.env.NEXT_PUBLIC_AUTH_MODE?.toLowerCase() ?? "auto") as AuthMode;

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const roleFromApi = (role: ApiUserRole | string): User["role"] => {
  const normalized = role.toString().toLowerCase();
  if (normalized === "admin") return "admin";
  if (normalized === "technician" || normalized === "engineer") return "engineer";
  return "client";
};

const roleFromSession = (session: ApiAuthSessionResponse): User["role"] => {
  const role = session.tikqRoles?.[0] ?? "Client";
  return roleFromApi(role);
};

const roleToApi = (role: string): ApiUserRole => {
  switch (role) {
    case "admin":
      return "Admin";
    case "engineer":
      return "Technician";
    default:
      return "Client";
  }
};

const mapSessionToUser = (session: ApiAuthSessionResponse): User => ({
  id: session.userId ?? "",
  name: session.displayName ?? session.email ?? "Unknown User",
  email: session.email ?? "",
  role: roleFromSession(session),
  phone: null,
  department: null,
  avatar: null,
  isSupervisor: session.isSupervisor ?? false,
});

function persistSession(token: string | null, user: User) {
  if (typeof window === "undefined") return;
  if (token && token !== COOKIE_SESSION_TOKEN) {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
  } else {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
  }

  localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));
  localStorage.setItem("userEmail", user.email);
  localStorage.setItem("userName", user.name);
}

function clearSession() {
  if (typeof window === "undefined") return;
  localStorage.removeItem(TOKEN_STORAGE_KEY);
  localStorage.removeItem(USER_STORAGE_KEY);
  localStorage.removeItem("userEmail");
  localStorage.removeItem("userName");
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const authMode: AuthMode = useMemo(() => {
    if (AUTH_MODE === "lan" || AUTH_MODE === "external" || AUTH_MODE === "auto") {
      return AUTH_MODE;
    }

    return "auto";
  }, []);

  const isExternalMode = authMode === "external";
  const isLanMode = authMode === "lan" || authMode === "auto";

  const refreshSession = async () => {
    const storedToken = typeof window !== "undefined" ? localStorage.getItem(TOKEN_STORAGE_KEY) : null;
    try {
      const session = await apiRequest<ApiAuthSessionResponse>("/api/auth/me", {
        token: storedToken,
        silent: true,
      });

      if (!session?.isAuthenticated) {
        setUser(null);
        setToken(null);
        clearSession();
        return;
      }

      const mapped = mapSessionToUser(session);
      setUser(mapped);
      const effectiveToken = storedToken ?? COOKIE_SESSION_TOKEN;
      setToken(effectiveToken);
      persistSession(effectiveToken, mapped);
    } catch (error: any) {
      if (error?.status === 401 || error?.status === 403) {
        setUser(null);
        if (!storedToken) {
          setToken(null);
        }
        if (error?.status === 403) {
          clearSession();
          setToken(null);
          throw new Error(error?.message || "No access to TikQ");
        }
        return;
      }

      throw error;
    }
  };

  useEffect(() => {
    if (typeof window === "undefined") {
      setIsLoading(false);
      return;
    }

    const storedToken = localStorage.getItem(TOKEN_STORAGE_KEY);
    const storedUser = localStorage.getItem(USER_STORAGE_KEY);
    if (storedToken) {
      setToken(storedToken);
    }

    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch {
        localStorage.removeItem(USER_STORAGE_KEY);
      }
    }

    const init = async () => {
      try {
        await refreshSession();
      } catch {
        // Initial load can start unauthenticated or forbidden.
      } finally {
        setIsLoading(false);
      }
    };

    void init();
  }, []);

  const login = async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await apiRequest<ApiAuthResponse>("/api/auth/login", {
        method: "POST",
        body: { email, password },
      });

      const mapped: User = {
        id: response.user.id,
        name: response.user.fullName,
        email: response.user.email,
        role: roleFromApi(response.user.role),
        phone: response.user.phoneNumber ?? null,
        department: response.user.department ?? null,
        avatar: response.user.avatarUrl ?? null,
        isSupervisor: response.user.isSupervisor ?? false,
      };

      setUser(mapped);
      setToken(response.token);
      persistSession(response.token, mapped);
      return true;
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (userData: {
    name: string;
    email: string;
    phone: string;
    department: string;
    role: string;
    password: string;
  }) => {
    setIsLoading(true);
    try {
      const response = await apiRequest<ApiAuthResponse>("/api/auth/register", {
        method: "POST",
        body: {
          fullName: userData.name,
          email: userData.email,
          password: userData.password,
          role: roleToApi(userData.role),
          phoneNumber: userData.phone,
          department: userData.department,
        },
      });

      const mapped: User = {
        id: response.user.id,
        name: response.user.fullName,
        email: response.user.email,
        role: roleFromApi(response.user.role),
        phone: response.user.phoneNumber ?? null,
        department: response.user.department ?? null,
        avatar: response.user.avatarUrl ?? null,
        isSupervisor: response.user.isSupervisor ?? false,
      };

      setUser(mapped);
      setToken(response.token);
      persistSession(response.token, mapped);
      return true;
    } catch {
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    void apiRequest("/api/auth/adfs/logout", {
      method: "POST",
      token,
      silent: true,
    }).catch(() => {});

    setUser(null);
    setToken(null);
    clearSession();
  };

  const updateProfile = async (updates: Partial<Omit<User, "id" | "role">>) => {
    try {
      const payload: Record<string, unknown> = {};
      if (updates.name) payload.fullName = updates.name;
      if (updates.email) payload.email = updates.email;
      if (typeof updates.phone !== "undefined") payload.phoneNumber = updates.phone;
      if (typeof updates.department !== "undefined") payload.department = updates.department;
      if (typeof updates.avatar !== "undefined") payload.avatarUrl = updates.avatar;

      await apiRequest("/api/auth/me", {
        method: "PUT",
        token,
        body: payload,
      });

      await refreshSession();
      return true;
    } catch {
      return false;
    }
  };

  const changePassword = async (
    currentPassword: string,
    newPassword: string,
    confirmNewPassword: string,
  ) => {
    await apiRequest<{ success: boolean; message: string }>("/api/auth/change-password", {
      method: "POST",
      token,
      body: {
        currentPassword,
        newPassword,
        confirmNewPassword,
      },
    });
    return true;
  };

  const startExternalLogin = async () => {
    const apiBaseUrl = await getApiBaseUrl();
    const returnUrl = encodeURIComponent("/");
    window.location.href = `${apiBaseUrl}/api/auth/adfs/login?returnUrl=${returnUrl}`;
  };

  const value: AuthContextType = {
    user,
    token,
    login,
    register,
    logout,
    updateProfile,
    changePassword,
    refreshSession,
    startExternalLogin,
    isLoading,
    authMode,
    isExternalMode,
    isLanMode,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return context;
}

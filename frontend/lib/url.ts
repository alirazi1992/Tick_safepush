// lib/url.ts
// Canonical URL construction utilities to prevent double-adding /api and trailing slashes

/**
 * Normalizes a base URL by:
 * - Trimming whitespace
 * - Removing trailing slashes
 * - Removing trailing "/api" suffix if present
 */
export function normalizeBaseUrl(base: string | null | undefined): string {
  if (!base) {
    return "";
  }

  let normalized = base.trim();

  // Remove trailing slashes
  normalized = normalized.replace(/\/+$/, "");

  // Remove trailing "/api" if present
  if (normalized.endsWith("/api")) {
    normalized = normalized.slice(0, -4);
  }

  return normalized;
}

/**
 * Joins a base URL with an API path, ensuring exactly one slash between them.
 * The path must start with "/api/..."
 * 
 * @param base - Base URL (e.g., "http://localhost:5000" or empty string for same-origin)
 * @param path - API path that must start with "/api/..." (e.g., "/api/health", "/api/tickets")
 * @returns Full URL with exactly one slash between base and path
 */
export function joinApi(base: string, path: string): string {
  // Normalize base first
  const normalizedBase = normalizeBaseUrl(base);

  // GUARD: Base must be absolute URL or empty (will use default)
  if (normalizedBase && !normalizedBase.startsWith("http://") && !normalizedBase.startsWith("https://")) {
    throw new Error(`[url] joinApi: Base URL must be absolute or empty. Got: "${base}" (normalized: "${normalizedBase}")`);
  }

  // Ensure path starts with "/api/"
  if (!path.startsWith("/api/")) {
    // If path doesn't start with /api/, add it
    const cleanPath = path.startsWith("/") ? path.slice(1) : path;
    path = `/api/${cleanPath}`;
  }

  // Remove leading slash from path for joining
  const cleanPath = path.startsWith("/") ? path : `/${path}`;

  // If base is empty, use default
  if (!normalizedBase) {
    const defaultBase = "http://localhost:5000";
    console.warn(`[url] joinApi: Base URL is empty, using default: ${defaultBase}`);
    return `${defaultBase}${cleanPath}`;
  }

  // Join base and path with exactly one slash
  return `${normalizedBase}${cleanPath}`;
}

/**
 * Gets the effective API base URL from environment or defaults
 */
export function getEffectiveApiBaseUrl(): string {
  const envUrl = process.env.NEXT_PUBLIC_API_BASE_URL;
  return normalizeBaseUrl(envUrl) || "";
}
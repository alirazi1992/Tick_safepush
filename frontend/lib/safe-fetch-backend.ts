export async function safeFetchBackend(
  url: string | URL | Request,
  init: RequestInit = {}
): Promise<Response> {
  // Merge headers safely (supports Headers, arrays, and plain objects)
  const mergedHeaders = new Headers(init.headers || undefined);

  return fetch(url, {
    ...init,
    headers: mergedHeaders,
    credentials: "include",
    mode: "cors",
    cache: "no-store",
  });
}

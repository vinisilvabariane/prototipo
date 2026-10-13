import "server-only";
import { redirect, unstable_rethrow } from "next/navigation";
import { apiUrl } from "@/lib/http/env";
import { failure, failureFromResponse, ok, type Result } from "@/lib/result";
import { getToken } from "./session";

const TIMEOUT_MS = 15_000;

type Options = { anonymous?: boolean };

export abstract class BaseService {
  constructor(protected readonly basePath: string) {}

  protected async request<T>(path: string, init: RequestInit = {}, { anonymous }: Options = {}): Promise<Result<T>> {
    const method = init.method ?? "GET";
    const headers = new Headers(init.headers);
    if (init.body) headers.set("Content-Type", "application/json");

    if (!anonymous) {
      const token = await getToken();
      if (!token) redirect("/login");
      headers.set("Authorization", `Bearer ${token}`);
    }

    let res: Response;
    try {
      res = await fetch(`${apiUrl()}/api/v1${this.basePath}${path}`, {
        ...init,
        headers,
        signal: AbortSignal.timeout(TIMEOUT_MS),
      });
    } catch (e) {
      unstable_rethrow(e);
      const timedOut = e instanceof DOMException && e.name === "TimeoutError";
      console.error(`[api] ${method} ${this.basePath}${path} -> ${timedOut ? "timeout" : "sem resposta"}`, e);
      return failure(timedOut ? "timeout" : "network");
    }

    if (res.status === 401 && !anonymous) redirect("/login");

    if (!res.ok) {
      const body = await res.json().catch(() => null);
      if (res.status >= 500) console.error(`[api] ${method} ${this.basePath}${path} -> ${res.status}`, body);
      return failureFromResponse(res.status, body);
    }

    if (res.status === 204) return ok(undefined as T);

    try {
      return ok((await res.json()) as T);
    } catch {
      console.error(`[api] ${method} ${this.basePath}${path} -> ${res.status} com corpo que não é JSON`);
      return failure("unexpected", { status: res.status });
    }
  }

  protected get<T>(path = "") {
    return this.request<T>(path);
  }

  protected post<T = void>(path: string, body?: unknown, options?: Options) {
    return this.request<T>(path, { method: "POST", body: JSON.stringify(body) }, options);
  }

  protected put<T = void>(path: string, body: unknown) {
    return this.request<T>(path, { method: "PUT", body: JSON.stringify(body) });
  }

  protected del(path: string) {
    return this.request<void>(path, { method: "DELETE" });
  }
}

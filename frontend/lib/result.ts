export type ErrorCode =
  | "badRequest"
  | "unauthorized"
  | "forbidden"
  | "notFound"
  | "tooManyRequests"
  | "unavailable"
  | "server"
  | "network"
  | "timeout"
  | "unexpected";

export type AppError = {
  code: ErrorCode;
  status: number | null;
  title: string;
  messages: string[];
};

export type Success<T> = { readonly ok: true; readonly value: T };
export type Failure = { readonly ok: false; readonly error: AppError };
export type Result<T = void> = Success<T> | Failure;

const DEFAULTS: Record<ErrorCode, { title: string; message: string }> = {
  badRequest: { title: "Dados inválidos", message: "Verifique as informações e tente novamente." },
  unauthorized: { title: "Sessão inválida", message: "Entre novamente para continuar." },
  forbidden: { title: "Sem permissão", message: "Você não tem permissão para realizar esta ação." },
  notFound: { title: "Não encontrado", message: "O registro não existe ou foi removido." },
  tooManyRequests: { title: "Muitas tentativas", message: "Aguarde um pouco e tente de novo." },
  unavailable: { title: "Serviço indisponível", message: "O servidor não está respondendo. Tente em instantes." },
  server: { title: "Erro no servidor", message: "Algo deu errado do nosso lado. Tente novamente." },
  network: { title: "Sem conexão com o servidor", message: "Não foi possível alcançar a API." },
  timeout: { title: "Tempo esgotado", message: "O servidor demorou demais para responder." },
  unexpected: { title: "Erro inesperado", message: "Algo deu errado. Tente novamente." },
};

export function ok(): Success<void>;
export function ok<T>(value: T): Success<T>;
export function ok(value?: unknown): Success<unknown> {
  return { ok: true, value };
}

type FailureOptions = { status?: number; title?: string; messages?: string[] };

export function failure(code: ErrorCode, { status, title, messages }: FailureOptions = {}): Failure {
  const defaults = DEFAULTS[code];
  return {
    ok: false,
    error: {
      code,
      status: status ?? null,
      title: title?.trim() || defaults.title,
      messages: messages?.length ? messages : [defaults.message],
    },
  };
}

export function codeFromStatus(status: number): ErrorCode {
  if (status === 401) return "unauthorized";
  if (status === 403) return "forbidden";
  if (status === 404) return "notFound";
  if (status === 429) return "tooManyRequests";
  if (status === 502 || status === 503 || status === 504) return "unavailable";
  if (status >= 500) return "server";
  if (status >= 400) return "badRequest";
  return "unexpected";
}

export function failureFromResponse(status: number, body: unknown): Failure {
  const code = codeFromStatus(status);

  if (code === "server") return failure(code, { status });

  const record = body && typeof body === "object" ? (body as Record<string, unknown>) : {};
  const messages: string[] = [];

  if (typeof record.details === "string") messages.push(...record.details.split("\n"));
  if (typeof record.detail === "string") messages.push(record.detail);
  if (record.errors && typeof record.errors === "object") {
    for (const value of Object.values(record.errors)) {
      if (Array.isArray(value)) messages.push(...value.filter((m): m is string => typeof m === "string"));
    }
  }

  return failure(code, {
    status,
    title: typeof record.error === "string" ? record.error : undefined,
    messages: messages.map((m) => m.trim()).filter(Boolean),
  });
}

type Values<T extends Record<string, Result<unknown>>> = {
  [K in keyof T]: T[K] extends Result<infer V> ? V : never;
};

export function all<T extends Record<string, Result<unknown>>>(results: T): Result<Values<T>> {
  const values: Record<string, unknown> = {};
  for (const [key, result] of Object.entries(results)) {
    if (!result.ok) return result;
    values[key] = result.value;
  }
  return ok(values as Values<T>);
}

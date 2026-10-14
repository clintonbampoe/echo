// One shape for every failed request. Screens branch on errorCode, never on message text.

export type FieldErrors = Record<string, string>;

// The one class in the codebase: Error subclasses are how JS carries a stack trace.
export class ApiError extends Error {
  readonly status: number;
  readonly errorCode: string;
  readonly fieldErrors: FieldErrors;

  constructor(
    status: number,
    errorCode: string,
    message: string,
    fieldErrors: FieldErrors = {},
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errorCode = errorCode;
    this.fieldErrors = fieldErrors;
  }
}

export function isApiError(e: unknown): e is ApiError {
  return e instanceof ApiError;
}

type Problem = {
  title?: string;
  detail?: string;
  errorCode?: string;
  errors?: Record<string, string[] | string>;
};

// "userDto.EmailAddress" -> "userDto.emailAddress". The full path is kept, because a form can
// hold two fields with the same last name (the church email and the administrator's email).
function camelPath(path: string): string {
  return path
    .split(".")
    .map((part) => part.charAt(0).toLowerCase() + part.slice(1))
    .join(".");
}

function toFieldErrors(errors: Problem["errors"]): FieldErrors {
  const out: FieldErrors = {};
  if (!errors) return out;
  for (const [path, value] of Object.entries(errors)) {
    const message = Array.isArray(value) ? value[0] : value;
    if (message) out[camelPath(path)] = message;
  }
  return out;
}

export function parseProblem(status: number, body: unknown): ApiError {
  const p = (typeof body === "object" && body !== null ? body : {}) as Problem;
  const code =
    p.errorCode ??
    (status === 400 && p.errors ? "VALIDATION_ERROR" : `HTTP_${status}`);
  const message =
    p.detail ?? p.title ?? "Something went wrong. Please try again.";
  return new ApiError(status, code, message, toFieldErrors(p.errors));
}

export function networkError(): ApiError {
  return new ApiError(
    0,
    "NETWORK",
    "Could not reach the server. Check your connection and try again.",
  );
}

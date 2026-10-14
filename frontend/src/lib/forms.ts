import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { isApiError, type ApiError } from "@/lib/api";

// Puts the server's per-field messages onto the matching form fields.
//
// `fields` maps the API's field path to the form's field name, e.g.
//   { 'userDto.emailAddress': 'email', 'congregationDto.emailAddress': 'churchEmail' }
// Returns true if at least one message landed on a field. If it returns false the error
// has nothing to point at, so show it in an Alert instead.
export function applyFieldErrors<T extends FieldValues>(
  error: ApiError,
  setError: UseFormSetError<T>,
  fields: Record<string, Path<T>>,
): boolean {
  let placed = false;
  for (const [apiPath, message] of Object.entries(error.fieldErrors)) {
    const field = fields[apiPath];
    if (!field) continue;
    setError(field, { message });
    placed = true;
  }
  return placed;
}

// The catch block of a submit handler. Field messages go onto their fields; anything else is
// handed to `show` for an Alert. Errors that are not API errors are bugs, so they rethrow.
export function handleSubmitError<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fields: Record<string, Path<T>>,
  show: (error: ApiError) => void,
): void {
  if (!isApiError(error)) throw error;
  if (!applyFieldErrors(error, setError, fields)) show(error);
}

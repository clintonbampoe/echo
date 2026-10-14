import type { Option } from "./selectTypes";

// A list where each value is its own label: plainOptions(['Male', 'Female']).
export const plainOptions = (values: readonly string[]): Option[] =>
  values.map((value) => ({ value, label: value }));

// A filter's list: the choices with "Any ..." in front, whose value is empty (meaning no filter).
export const withAny = (
  label: string,
  options: readonly Option[],
): Option[] => [{ value: "", label }, ...options];

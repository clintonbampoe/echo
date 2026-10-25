// The choices of a dropdown, and the two ways lists of them are usually made. Plain TypeScript, so the rules that
// build a list (in a feature's model/) need nothing from the screen.
export type Option = { value: string; label: string };

// A list where each value is its own label: plainOptions(['Male', 'Female']).
export const plainOptions = (values: readonly string[]): Option[] =>
  values.map((value) => ({ value, label: value }));

// A filter's list: the choices with "Any ..." in front, whose value is empty (meaning no filter).
export const withAny = (
  label: string,
  options: readonly Option[],
): Option[] => [{ value: "", label }, ...options];

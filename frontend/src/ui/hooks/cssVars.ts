import type { CSSProperties } from "react";

// Style props with CSS variables in them, e.g. cssVars({ "--i": 3 }), which React's types do not allow directly.
export const cssVars = (
  vars: Record<`--${string}`, string | number>,
  rest?: CSSProperties,
): CSSProperties => ({ ...rest, ...vars }) as CSSProperties;

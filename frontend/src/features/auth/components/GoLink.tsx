import { Link, type LinkProps } from "react-router-dom";
import { useAuthGo } from "../hooks/useAuthGo";

type Props = Omit<LinkProps, "to" | "state"> & {
  to: string;
  // Read at the moment of the click, for values the page does not re-render on (like a typed email).
  stateAt?: () => unknown;
};

// A link between auth screens: the title and card travel (see useAuthGo). Ctrl, Cmd, Shift and
// middle clicks keep their usual meaning, so "open in a new tab" still works.
export function GoLink({ to, stateAt, ...rest }: Props) {
  const go = useAuthGo();
  return (
    <Link
      to={to}
      {...rest}
      onClick={(e) => {
        if (e.metaKey || e.ctrlKey || e.shiftKey || e.button !== 0) return;
        e.preventDefault();
        go(to, stateAt ? { state: stateAt() } : undefined);
      }}
    />
  );
}

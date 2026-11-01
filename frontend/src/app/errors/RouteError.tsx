import { useRouteError } from "react-router-dom";
import { classify } from "./errorView";
import { ErrorScreen } from "./ErrorScreen";

// The router's errorElement for a page that failed to load or threw while drawing. Two of them: one that keeps
// the sidebar and one that is a page of its own. The error goes to the console for whoever is debugging.
function Caught({ inShell }: { inShell?: boolean }) {
  const error = useRouteError();
  console.error(error);
  return <ErrorScreen kind={classify(error)} inShell={inShell} />;
}

export const PageRouteError = () => <Caught />;
export const ShellRouteError = () => <Caught inShell />;

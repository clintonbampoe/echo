import { useRouteError } from "react-router-dom";
import { classify } from "./errorView";
import { ErrorScreen } from "./ErrorScreen";

// The router's errorElement for a page that failed to load or threw while drawing. It sits at the root, so
// whatever failed, the person gets the same page of the website. The error goes to the console for whoever is
// debugging.
export function RouteError() {
  const error = useRouteError();
  console.error(error);
  return <ErrorScreen kind={classify(error)} />;
}

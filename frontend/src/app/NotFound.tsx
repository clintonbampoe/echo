import { ErrorScreen } from "./errors";

// Any address nobody owns: a page of the website, with a way home.
export function NotFound() {
  return <ErrorScreen kind="notFound" />;
}

import { useIsSignedIn } from "@/features/auth";
import { ErrorScreen } from "./errors";
import { AppShell } from "./shell";

// Any address nobody owns. Someone signed in sees it inside the app, with the sidebar to leave by.
export function NotFound() {
  const signedIn = useIsSignedIn();
  if (!signedIn) return <ErrorScreen kind="notFound" />;
  return (
    <AppShell>
      <ErrorScreen kind="notFound" inShell />
    </AppShell>
  );
}

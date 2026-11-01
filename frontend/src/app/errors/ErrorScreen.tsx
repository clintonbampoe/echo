import { useNavigate } from "react-router-dom";
import { useIsSignedIn } from "@/features/auth";
import {
  Button,
  LinkButton,
  Logo,
  ScreenMessage,
  usePublicSurface,
} from "@/ui";
import { errorViews, type ErrorKind } from "./errorView";
import styles from "./ErrorScreen.module.css";

function Message({ kind }: { kind: ErrorKind }) {
  const view = errorViews[kind];
  const navigate = useNavigate();
  const signedIn = useIsSignedIn();
  const home = signedIn ? "/dashboard" : "/";
  const homeLabel = signedIn ? "Go to the dashboard" : "Go to the home page";
  const actions =
    view.fix === "reload" ? (
      <>
        <Button size="lg" pill onClick={() => window.location.reload()}>
          {view.reload}
        </Button>
        <LinkButton to={home} variant="ghost" size="lg" pill>
          {homeLabel}
        </LinkButton>
      </>
    ) : (
      <>
        <LinkButton to={home} size="lg" pill>
          {homeLabel}
        </LinkButton>
        <Button variant="ghost" size="lg" pill onClick={() => navigate(-1)}>
          Go back
        </Button>
      </>
    );
  return (
    <ScreenMessage
      icon={view.icon}
      tone={view.broken ? "neg" : undefined}
      title={view.title}
      actions={actions}
      top={<Logo />}
    >
      {view.text}
    </ScreenMessage>
  );
}

// The screen for something that went wrong or is not there. It is a page of the website, on the public
// surface, never drawn inside the app shell: the app around it may be the very thing that failed, and the
// person may not even be signed in.
export function ErrorScreen({ kind }: { kind: ErrorKind }) {
  usePublicSurface();
  return (
    <main className={styles.page}>
      <Message kind={kind} />
    </main>
  );
}

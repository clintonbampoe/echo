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

type Props = {
  kind: ErrorKind;
  // Drawn inside the app (the sidebar stays), not as a page of its own.
  inShell?: boolean;
};

function Message({ kind, inShell }: Props) {
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
      top={inShell ? undefined : <Logo />}
    >
      {view.text}
    </ScreenMessage>
  );
}

// The screen for something that went wrong or is not there. Inside the app it sits where the page would have
// been, so the sidebar is still there to leave by. Outside, it is a page of its own.
export function ErrorScreen({ kind, inShell }: Props) {
  usePublicSurface();
  if (inShell) return <Message kind={kind} inShell />;
  return (
    <main className={styles.page}>
      <Message kind={kind} />
    </main>
  );
}

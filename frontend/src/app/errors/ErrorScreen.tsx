import { useNavigate } from "react-router-dom";
import { Kicker, SiteFrame, Stop } from "@/features/website";
import { useIsSignedIn } from "@/features/auth";
import { Button, LinkButton } from "@/ui";
import { PulseArt, WanderArt } from "./ErrorArt";
import { errorViews, type ErrorKind } from "./errorView";
import styles from "./ErrorScreen.module.css";

function Actions({ kind }: { kind: ErrorKind }) {
  const view = errorViews[kind];
  const navigate = useNavigate();
  const signedIn = useIsSignedIn();
  const home = signedIn ? "/dashboard" : "/";
  const homeLabel = signedIn ? "Go to the dashboard" : "Back to home";
  if (view.fix === "reload")
    return (
      <>
        <Button size="xl" pill onClick={() => window.location.reload()}>
          {view.reload}
        </Button>
        <LinkButton to={home} variant="secondary" size="xl" pill>
          {homeLabel}
        </LinkButton>
      </>
    );
  return (
    <>
      <LinkButton to={home} size="xl" pill>
        {homeLabel}
      </LinkButton>
      <Button variant="secondary" size="xl" pill onClick={() => navigate(-1)}>
        Go back
      </Button>
    </>
  );
}

// The screen for something that went wrong or is not there. It is a page of the website, inside the
// website's own frame, never drawn inside the app shell: the app around it may be the very thing that
// failed, and the person may not even be signed in.
export function ErrorScreen({ kind }: { kind: ErrorKind }) {
  const view = errorViews[kind];
  return (
    <SiteFrame>
      <div className={styles.page}>
        <div>
          <Kicker>{view.kicker}</Kicker>
          <h1 className={styles.title}>
            {view.title}
            <Stop />
          </h1>
          <p className={styles.lead}>{view.text}</p>
          <div className={styles.actions}>
            <Actions kind={kind} />
          </div>
        </div>
        {view.broken ? <PulseArt /> : <WanderArt />}
      </div>
    </SiteFrame>
  );
}

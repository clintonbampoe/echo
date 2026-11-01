import { Alert, Button, buttonClass, Icon, LinkButton } from "@/ui";
import { ResendLine } from "../components/ResendLine";
import type { MailProvider } from "../model/mailProvider";
import type { VerifyPhase } from "../model/verifyPhase";
import styles from "./VerifySentPage.module.css";

type Props = {
  phase: Exclude<VerifyPhase, "verified">;
  provider: MailProvider | undefined;
  wait: number;
  signedIn: boolean;
  leave: string;
  onSend: () => void;
};

const STATUS = {
  sending: "Sending the link…",
  waiting: "Waiting for you to open the link",
  slow: "Still waiting. Check spam, or send another link.",
} as const;

// What sits under the address while the account is not verified yet: how things stand, the way to the inbox,
// another link, and the way back to the app. Leaving is always allowed.
export function VerifyWaiting({
  phase,
  provider,
  wait,
  signedIn,
  leave,
  onSend,
}: Props) {
  return (
    <>
      {phase === "failed" ? (
        <Alert title="We could not send the link">
          Check your connection and try again.
        </Alert>
      ) : (
        <p className={styles.status} role="status">
          <span>{STATUS[phase]}</span>
          {phase !== "sending" && <Dots />}
        </p>
      )}
      {phase === "failed" ? (
        <Button size="lg" pill block onClick={onSend}>
          Try again
        </Button>
      ) : (
        provider && <MailLink provider={provider} quiet={phase === "slow"} />
      )}
      {phase !== "failed" && <ResendLine wait={wait} onSend={onSend} />}
      <div className={styles.skip}>
        <LinkButton to={leave} variant="ghost" size="lg" pill>
          {signedIn ? "Back to the app" : "Skip for now"}
          <Icon name="arrow-right" size={16} />
        </LinkButton>
        <p className={styles.note}>
          {signedIn
            ? "Leaving is fine. We keep the reminder until you verify your account."
            : "You can sign in now. We keep a reminder at the top of the app until you verify your account."}
        </p>
      </div>
    </>
  );
}

const Dots = () => (
  <span className={styles.dots} aria-hidden="true">
    <i />
    <i />
    <i />
  </span>
);

function MailLink({
  provider,
  quiet,
}: {
  provider: MailProvider;
  quiet: boolean;
}) {
  return (
    <a
      href={provider.url}
      target="_blank"
      rel="noopener noreferrer"
      className={buttonClass({
        variant: quiet ? "secondary" : undefined,
        size: "lg",
        block: true,
      })}
    >
      Open {provider.name}
      <Icon name="arrow-up" size={16} className={styles.out} />
    </a>
  );
}

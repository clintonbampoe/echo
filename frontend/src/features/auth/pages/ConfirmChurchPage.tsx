import { useNavigate, useParams } from "react-router-dom";
import { Button } from "@/ui";
import { BackLink } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import { InvitationCard } from "../components/InvitationCard";
import { InvitationGate } from "../components/InvitationGate";
import styles from "./form.module.css";

// /join/:code. The code (typed in, or from the emailed link) names a church. Before asking
// for anything, show which church that is so a mistyped or wrong code is caught here.
export function ConfirmChurchPage() {
  const code = useParams().code ?? "";
  const navigate = useNavigate();
  return (
    <InvitationGate code={code}>
      {(invitation) => (
        <AuthScreen
          title="Is this your church?"
          text="We found this church from your invitation. Check that it is the right one before you join."
          links={<BackLink to="/join">Use a different code</BackLink>}
        >
          <InvitationCard invitation={invitation} />
          <Button
            size="lg"
            pill
            block
            className={styles.submit}
            onClick={() =>
              navigate(`/join/${encodeURIComponent(code)}/account`)
            }
          >
            Yes, continue
          </Button>
          <Button
            variant="secondary"
            size="lg"
            pill
            block
            onClick={() => navigate("/join")}
          >
            This is not my church
          </Button>
        </AuthScreen>
      )}
    </InvitationGate>
  );
}

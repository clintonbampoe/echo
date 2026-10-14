import { useNavigate } from "react-router-dom";
import { Button, EmptyState, type IconName } from "@/ui";
import styles from "./ComingSoon.module.css";

type Props = { icon: IconName; title: string };

// Stands in for a page that is not built yet, so links to it already work. When the page ships, its
// feature registers the real route and this one is deleted from routes.tsx.
export function ComingSoon({ icon, title }: Props) {
  const navigate = useNavigate();
  return (
    <div className={styles.wrap}>
      <EmptyState
        icon={icon}
        title={title}
        action={
          <Button variant="secondary" onClick={() => void navigate(-1)}>
            Go back
          </Button>
        }
      >
        This page is on its way. Until then, you can carry on from where you
        were.
      </EmptyState>
    </div>
  );
}

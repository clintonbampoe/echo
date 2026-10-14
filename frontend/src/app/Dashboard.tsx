import { useCongregation, useMe } from "@/features/account";
import { firstName } from "@/lib/format";
import { greetingFor } from "./greeting";
import styles from "./Dashboard.module.css";

const today = () =>
  new Intl.DateTimeFormat("en-GB", {
    weekday: "long",
    day: "numeric",
    month: "long",
  }).format(new Date());

// Placeholder until the real dashboard is built. The shell around it is already final.
export function Dashboard() {
  const me = useMe().data;
  const church = useCongregation().data;
  const greeting = `${greetingFor(new Date().getHours())}${me ? `, ${firstName(me.name)}` : ""}`;
  return (
    <div className="anim-rise">
      <h1 className={styles.title}>{greeting}</h1>
      <p className={styles.text}>
        {today()}.{church ? ` Here is how ${church.name} is doing.` : ""}
      </p>
      <p className={styles.soon}>Your numbers will appear here soon.</p>
    </div>
  );
}

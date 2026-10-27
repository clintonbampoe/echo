import { paths } from "@/lib/paths";
import { Button, EmptyState, LinkButton } from "@/ui";
import { dayMonth, dayName, shortDay } from "../../model/day";

type Props = {
  day: string;
  noTypes: boolean;
  nearest: string | null;
  onPick: (day: string) => void;
  onNew: () => void;
};

// What a day with no sessions says. With no session types at all it is the first run, so it points at
// setting those up; otherwise it offers the next day that has something, and a new session for this one.
export function EmptyDay({ day, noTypes, nearest, onPick, onNew }: Props) {
  if (noTypes) {
    return (
      <EmptyState
        icon="calendar-check"
        title="Create your first session"
        action={
          <>
            <LinkButton to={paths.sessionTypes} variant="secondary">
              Set up session types
            </LinkButton>
            <Button onClick={onNew}>New session</Button>
          </>
        }
      >
        A session is one gathering, like this Sunday service. Create it, then
        mark who is present. Session types such as Service or Bible Study come
        first, so you can reuse them.
      </EmptyState>
    );
  }
  return (
    <EmptyState
      icon="calendar"
      title="No sessions on this day"
      action={
        <>
          {nearest && (
            <Button variant="secondary" onClick={() => onPick(nearest)}>
              Go to {shortDay(nearest)}
            </Button>
          )}
          <Button onClick={onNew}>New session on {dayMonth(day)}</Button>
        </>
      }
    >
      Nothing is scheduled for {dayName(day)}. Create a session for this date
      {nearest ? ", or jump to the nearest one" : ""}.
    </EmptyState>
  );
}

import { AttendedEvents } from "./AttendedEvents";
import { RegisteredEvents } from "./RegisteredEvents";

// What a person did with events: the ones they signed up for, and the ones they walked into.
export function PersonEvents({ personId }: { personId: string }) {
  return (
    <>
      <RegisteredEvents personId={personId} />
      <AttendedEvents personId={personId} />
    </>
  );
}

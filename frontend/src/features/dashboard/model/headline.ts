import { firstName, plural } from "@/lib/format";
import { greetingFor } from "./greeting";
import { stepsLeft } from "./setup";

// What the dashboard header says: a greeting and a line under it.
type Input = {
  // Set for a church that has recorded nothing: it is welcomed rather than greeted.
  welcomeLeft?: number;
  phone: boolean;
  hour: number;
  fullName?: string;
  church?: string;
  atRisk?: number;
  newVisitors?: number;
};

// What needs a look: projects at risk and visitors of the last month.
const notesOf = (atRisk?: number, newVisitors?: number) =>
  [
    atRisk ? `${plural(atRisk, "project")} at risk` : "",
    newVisitors ? `${plural(newVisitors, "new visitor")} this month` : "",
  ].filter(Boolean);

export function headline(i: Input) {
  const name = i.fullName ? `, ${firstName(i.fullName)}` : "";
  const church = i.church ?? "";
  if (i.welcomeLeft !== undefined) {
    const left = stepsLeft(i.welcomeLeft);
    const rest = i.phone
      ? `${left} to go.`
      : `${left} and Echo starts giving answers back.`;
    return {
      title: `Welcome${name}`,
      line: `${church || "Your church"} is set up. ${rest}`,
    };
  }
  const notes = notesOf(i.atRisk, i.newVisitors);
  return {
    title: `${greetingFor(i.hour)}${name}`,
    line: notes.length > 0 ? notes.join(" · ") : church,
  };
}

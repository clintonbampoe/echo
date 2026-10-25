// What the session forms accept, and the words shown when they do not. The server has the final say; this
// catches the obvious mistakes first. No React in this file.
import { z } from "zod";
import type { Option } from "@/ui";
import type { Session, SessionInput, SessionUpdate } from "../api/sessions";
import type { SessionCategory, SessionLabelInput } from "../api/sessionLabels";

export const NAME_MAX = 200;
export const TOPIC_MAX = 500;
export const NOTES_MAX = 2000;
export const LABEL_MAX = 100;

// A time already set can be changed, not taken away: the API has no way to clear it.
export type Locked = { startsAt: boolean; endsAt: boolean };
const NONE: Locked = { startsAt: false, endsAt: false };

export const sessionSchema = (locked: Locked = NONE) =>
  z
    .object({
      sessionLabelId: z.string().min(1, "Choose a session type"),
      date: z.string().min(1, "Enter the date of the session"),
      name: z.string().max(NAME_MAX),
      topic: z.string().max(TOPIC_MAX),
      startsAt: z.string(),
      endsAt: z.string(),
      notes: z.string().max(NOTES_MAX),
    })
    .superRefine((v, ctx) => {
      for (const key of ["startsAt", "endsAt"] as const) {
        if (locked[key] && v[key] === "")
          ctx.addIssue({
            code: "custom",
            path: [key],
            message: "A time can be changed, not removed",
          });
      }
      if (v.startsAt && v.endsAt && v.endsAt <= v.startsAt)
        ctx.addIssue({
          code: "custom",
          path: ["endsAt"],
          message: "The end must be after the start",
        });
    });

export type SessionValues = z.infer<ReturnType<typeof sessionSchema>>;

export const emptySession = (day: string): SessionValues => ({
  sessionLabelId: "",
  date: day,
  name: "",
  topic: "",
  startsAt: "",
  endsAt: "",
  notes: "",
});

// "08:00:00" in the API, "08:00" in a time box.
const toBox = (time?: string | null) => (time ?? "").slice(0, 5);
const toApi = (time: string) => (time ? `${time}:00` : undefined);

export function fromSession(s: Session): SessionValues {
  return {
    sessionLabelId: String(s.sessionLabelId),
    date: s.date,
    name: s.name ?? "",
    topic: s.topic ?? "",
    startsAt: toBox(s.startsAt),
    endsAt: toBox(s.endsAt),
    notes: s.notes ?? "",
  };
}

export const lockedTimes = (
  s: Pick<Session, "startsAt" | "endsAt">,
): Locked => ({ startsAt: Boolean(s.startsAt), endsAt: Boolean(s.endsAt) });

export function toSessionInput(v: SessionValues): SessionInput {
  return {
    sessionLabelId: Number(v.sessionLabelId),
    date: v.date,
    name: v.name.trim() || undefined,
    topic: v.topic.trim() || undefined,
    notes: v.notes.trim() || undefined,
    startsAt: toApi(v.startsAt),
    endsAt: toApi(v.endsAt),
  };
}

// Saving changes: the type and the day cannot change. Text is cleared by sending it empty; a time is sent only
// when there is one.
export function toSessionUpdate(v: SessionValues): SessionUpdate {
  return {
    name: v.name.trim(),
    topic: v.topic.trim(),
    notes: v.notes.trim(),
    startsAt: toApi(v.startsAt),
    endsAt: toApi(v.endsAt),
  };
}

export const sessionFieldMap = Object.fromEntries(
  [
    "sessionLabelId",
    "date",
    "name",
    "topic",
    "startsAt",
    "endsAt",
    "notes",
  ].map((k) => [k, k]),
) as Record<keyof SessionValues, keyof SessionValues>;

// A session type: a name, and the category that groups it in reports.
export const categories: readonly SessionCategory[] = [
  "Service",
  "Meeting",
  "Study",
  "Prayer",
  "Practice",
  "Outreach",
  "Class",
  "Special",
];
export const categoryOptions: readonly Option[] = categories.map((c) => ({
  value: c,
  label: c,
}));

export const labelSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Enter a name")
    .max(LABEL_MAX, "Keep the name under 100 letters"),
  category: z
    .string()
    .refine(
      (v) => categories.includes(v as SessionCategory),
      "Choose a category",
    ),
});

export type LabelValues = z.infer<typeof labelSchema>;

export const emptyLabel: LabelValues = { name: "", category: "" };

export const toLabelInput = (v: LabelValues): SessionLabelInput => ({
  name: v.name.trim(),
  category: v.category as SessionCategory,
});

export const labelFieldMap = { name: "name", category: "category" } as const;

// The types as a dropdown, starting on the "nothing chosen yet" line.
export const typeOptions = (
  labels: readonly { id: number; name: string }[],
): Option[] => [
  { value: "", label: "Choose a type" },
  ...labels.map((l) => ({ value: String(l.id), label: l.name })),
];

// The time on a check-in, between the form's "09:04" and the API's date and time. No React in this file.
import { z } from "zod";

export const checkInSchema = z.object({
  time: z.string().min(1, "Enter the time they arrived"),
});
export type CheckInValues = z.infer<typeof checkInSchema>;

// "2026-10-11T09:04:00" -> "09:04", read in the viewer's own time zone (as the lists write it).
export function timeOf(checkedInAt: string): string {
  const at = new Date(checkedInAt);
  return `${String(at.getHours()).padStart(2, "0")}:${String(at.getMinutes()).padStart(2, "0")}`;
}

// The session's day and the time typed, as the moment the API stores.
export const toCheckedInAt = (day: string, time: string) =>
  new Date(`${day}T${time}:00`).toISOString();

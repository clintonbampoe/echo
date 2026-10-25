import type { ChipTone } from "@/ui";
import type { EventStatus } from "./display";

export const statusLabel: Record<EventStatus, string> = {
  attended: "Attended",
  upcoming: "Upcoming",
  missed: "Did not attend",
};
export const statusTone: Record<EventStatus, ChipTone> = {
  attended: "positive",
  upcoming: "neutral",
  missed: "warning",
};

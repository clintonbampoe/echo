import type { ChipTone } from "@/ui";
import type { ProjectStatus } from "./api";

export const statusLabel: Record<ProjectStatus, string> = {
  Planning: "Planning",
  OnTrack: "On track",
  AtRisk: "At risk",
  Complete: "Complete",
  Missed: "Missed",
};

export const statusTone: Record<ProjectStatus, ChipTone> = {
  Planning: "neutral",
  OnTrack: "positive",
  AtRisk: "warning",
  Complete: "positive",
  Missed: "negative",
};

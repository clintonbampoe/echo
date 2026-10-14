import type { ChipTone } from "@/ui";
import type { MemberStatus } from "./api";

// Active is good news, Inactive needs a look, the rest are simply set aside.
export const statusTone: Record<MemberStatus, ChipTone> = {
  Active: "positive",
  Inactive: "warning",
  Archived: "neutral",
  Transferred: "neutral",
};

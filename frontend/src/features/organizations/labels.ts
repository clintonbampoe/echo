import type { ChipTone } from "@/ui";
import type { OrganizationRole } from "./api";

export const roleLabel: Record<OrganizationRole, string> = {
  Member: "Member",
  Secretary: "Secretary",
  Leader: "Leader",
};

// The leader stands out, the secretary is marked in green, an ordinary member is quiet.
export const roleTone: Record<OrganizationRole, ChipTone> = {
  Leader: "strong",
  Secretary: "positive",
  Member: "neutral",
};

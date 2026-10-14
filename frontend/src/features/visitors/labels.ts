import type { ChipTone } from "@/ui";
import type { VisitorKind } from "./view";

// Coming back is the good news. A first visit is simply new.
export const kindTone: Record<VisitorKind, ChipTone> = {
  New: "neutral",
  Returning: "positive",
};

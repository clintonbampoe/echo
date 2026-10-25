import type { ChipTone } from "@/ui";
import type { VisitorKind } from "./display";

// Coming back is the good news. A first visit is simply new.
export const kindTone: Record<VisitorKind, ChipTone> = {
  New: "neutral",
  Returning: "positive",
};

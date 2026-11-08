import { useState } from "react";
import type { UseFormReturn } from "react-hook-form";
import { useMedia } from "@/ui";
import type { ChurchValues } from "../forms/schema";
import { CHURCH_PARTS } from "../model/churchParts";

// A phone asks for the church in three short parts, one at a time; a larger screen shows the whole form.
export function useChurchParts(form: UseFormReturn<ChurchValues>) {
  const phone = useMedia("(max-width: 599px)");
  const [part, setPart] = useState(0);
  const partial = phone && part < CHURCH_PARTS.length - 1;

  async function next() {
    const ok = await form.trigger(CHURCH_PARTS[part]);
    if (ok) setPart(part + 1);
  }

  // After the API objects to something, take them back to the first part that has an error.
  function showFirstError() {
    const bad = CHURCH_PARTS.findIndex((names) =>
      names.some((n) => form.getFieldState(n).error),
    );
    if (bad >= 0) setPart(bad);
  }

  return { phone, part, setPart, partial, next, showFirstError };
}

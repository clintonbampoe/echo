import { z } from "zod";

// A text field that must not be empty or only spaces. The message is what the person reads.
export const required = (message: string) => z.string().trim().min(1, message);

// What an amount field accepts: its smallest and largest value, and what to say when it is empty or out of range.
export type AmountRule = {
  min: number;
  max: number;
  empty: string;
  range: string;
};

// The first thing wrong with an amount as typed, or nothing. Commas are allowed ("1,250.50"), and so are up to two
// pesewas.
export function amountProblem(text: string, rule: AmountRule): string | null {
  if (text === "") return rule.empty;
  const plain = text.replace(/,/g, "");
  if (!/^\d+(\.\d{1,2})?$/.test(plain)) return rule.range;
  const value = Number(plain);
  return value < rule.min || value > rule.max ? rule.range : null;
}

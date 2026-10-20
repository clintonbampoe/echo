// What the Tithes card says under its chart. Pure.
import { formatMoney, formatNumber } from "@/lib/format";
import type { TitheFigures } from "./api";
import { change } from "./cards";
import type { Unit } from "./trend";

export type TitheView = {
  total?: string;
  tithers?: string;
  average?: string;
  footer: string;
};

// "6.2% up on the week before". Nothing when the stretch before gave nothing to compare with.
export function footerOf(now: number, before: number, unit: Unit): string {
  const chip = change(now, before);
  if (!chip) return "";
  if (!chip.arrow) return `Same as the ${unit} before`;
  return `${chip.text} ${chip.arrow} on the ${unit} before`;
}

// The three figures for the stretch picked (the latest when none is), as text, and the comparison
// with the stretch before it.
export function titheView(
  values: readonly TitheFigures[] | undefined,
  unit: Unit,
  picked?: number,
): TitheView {
  const index = picked ?? (values?.length ?? 1) - 1;
  const at = values?.[index];
  const before = values?.[index - 1];
  if (!at) return { footer: "" };
  return {
    total: formatMoney(at.total),
    tithers: formatNumber(at.tithers),
    average: formatMoney(Math.round(at.average)),
    footer: before ? footerOf(at.total, before.total, unit) : "",
  };
}

// What the Tithes card says under its chart. Pure.
import { formatMoney, formatNumber } from "@/lib/format";
import type { TitheFigures } from "./api";
import { change } from "./cards";
import type { Unit } from "./trend";

export type TitheView = {
  last?: string;
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

// The three figures for the latest stretch, as text, and the comparison with the one before it.
export function titheView(
  values: readonly TitheFigures[] | undefined,
  unit: Unit,
): TitheView {
  const last = values?.[values.length - 1];
  const before = values?.[values.length - 2];
  if (!last) return { footer: "" };
  return {
    last: formatMoney(last.total),
    tithers: formatNumber(last.tithers),
    average: formatMoney(Math.round(last.average)),
    footer: before ? footerOf(last.total, before.total, unit) : "",
  };
}

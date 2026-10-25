import { StatStrip, type Stat } from "@/ui";
import { buildCards, type CardView } from "../cards";
import { useNumbers } from "../hooks";
import { periodOf, spanOf, type PeriodKey } from "../periods";

type Props = { period: PeriodKey; today: string };

// The strip's chip: the move against the stretch before, or how many are new.
const changeOf = (c: CardView) =>
  c.chip && (c.chip.arrow === "down" ? `−${c.chip.text}` : c.chip.text);

const money = new Set(["tithes", "net"]);

// Members, Present, Tithes and Net income for the period picked, in one strip.
export function NumberCards({ period, today }: Props) {
  const numbers = useNumbers(spanOf(period, today), today);
  const cards = buildCards(numbers, periodOf(period));
  const stats: Stat[] = cards.map((c) => ({
    label: c.label,
    value: money.has(c.key) ? c.value.replace(/^GH₵\s*/, "") : c.value,
    unit: money.has(c.key) ? "GHS" : undefined,
    note: c.note,
    change: changeOf(c),
  }));
  return (
    <StatStrip
      label="Numbers for the period"
      stats={stats}
      busy={cards.some((c) => c.state === "loading")}
    />
  );
}

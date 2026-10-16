import { useActiveMemberCount } from "@/features/members";
import { StatStrip } from "@/ui";
import type { TitheSummary } from "../api";
import { titheStats } from "../view";

type Props = { summary?: TitheSummary; busy: boolean; collected: string };

// The four numbers above the list. They follow the same filters as the list.
export function TitheStats({ summary, busy, collected }: Props) {
  const active = useActiveMemberCount();
  return (
    <StatStrip
      label="Tithe summary"
      stats={titheStats(summary, active, collected)}
      busy={busy}
    />
  );
}

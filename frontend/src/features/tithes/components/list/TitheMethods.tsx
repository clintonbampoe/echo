import { paymentLabel, type PaymentMethod } from "@/features/transactions";
import { Aside, DashCard, EmptyState, RankedBars } from "@/ui";
import type { TitheFilters } from "../../api/tithes";
import { useTitheByMethod } from "../../hooks/queries";
import { methodSlices } from "../../model/display";

const METHODS = Object.keys(paymentLabel) as PaymentMethod[];

type Props = {
  filters: TitheFilters;
  // What the card says it covers ("October 2026").
  period: string;
  // The method the list is narrowed to, and what choosing a row does.
  selected: string;
  onPick: (method: string) => void;
};

// "How it was paid": what each way of paying brought in under the page's filters. Choosing a row narrows the list to it.
export function TitheMethods({ filters, period, selected, onPick }: Props) {
  const results = useTitheByMethod(filters, METHODS);
  const loaded = results.every((r) => r.data);
  const rows = methodSlices(results.flatMap((r) => (r.data ? [r.data] : [])));
  return (
    <DashCard title="How it was paid" control={<Aside>{period}</Aside>}>
      {!loaded ? null : rows.length === 0 ? (
        <EmptyState compact icon="tithes" title="Nothing paid">
          No tithes in this range.
        </EmptyState>
      ) : (
        <RankedBars
          label="Tithes by way of paying"
          rows={rows}
          selected={selected}
          onPick={(key) => onPick(key === selected ? "" : key)}
        />
      )}
    </DashCard>
  );
}

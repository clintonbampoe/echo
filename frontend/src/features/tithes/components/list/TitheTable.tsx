import { paymentLabel } from "@/features/transactions";
import { formatDate, formatMoney } from "@/lib/format";
import { paths } from "@/lib/paths";
import { RecordTable } from "@/ui";
import type { TitheFilters } from "../../api/tithes";
import { useMemberTithes, useTitheSummary } from "../../hooks/queries";

type Props = { memberId: string; filters: TitheFilters; narrowed: boolean };

// The tithes a member gave that match the filters, newest first, with what they add up to under it. A row opens
// that tithe on the Tithes page.
export function TitheTable({ memberId, filters, narrowed }: Props) {
  const list = useMemberTithes(memberId, filters);
  const total = useTitheSummary({ memberId, ...filters });
  return (
    <RecordTable
      label="Tithes"
      title="Tithes"
      list={list}
      rowKey={(t) => t.id}
      large
      href={(t) => paths.tithe(t.id)}
      empty={
        narrowed
          ? {
              icon: "search",
              title: "No tithes match",
              text: "Try a different method, or a wider time range.",
            }
          : {
              icon: "tithes",
              title: "No tithes recorded",
              text: "Tithes are recorded on the Tithes page. Once one is, it shows here with the month it is for.",
            }
      }
      noun={["tithe recorded", "tithes recorded"]}
      total={
        total.data && (
          <>
            Total <strong>{formatMoney(total.data.totalCollected)}</strong>
          </>
        )
      }
      columns={[
        { header: "For month", cell: (t) => `${t.forMonth} ${t.forYear}` },
        {
          header: "Amount",
          cell: (t) => <strong>{formatMoney(t.amount)}</strong>,
          end: true,
        },
        { header: "Method", cell: (t) => paymentLabel[t.paymentMethod] },
        { header: "Collected", cell: (t) => formatDate(t.collectionDate) },
      ]}
    />
  );
}

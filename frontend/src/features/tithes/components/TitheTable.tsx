import { PaymentChip } from "@/features/transactions";
import { formatDate, formatMoney } from "@/lib/format";
import { RecordTable } from "@/ui";
import { useMemberTithes, useTitheTotalEver } from "../hooks";

// Every tithe a member gave, newest first, with the total they have ever given under it.
export function TitheTable({ memberId }: { memberId: string }) {
  const list = useMemberTithes(memberId);
  const total = useTitheTotalEver(memberId, list.rows.length > 0);
  return (
    <RecordTable
      label="Tithes"
      list={list}
      rowKey={(t) => t.id}
      large
      empty={{
        icon: "tithes",
        title: "No tithes recorded",
        text: "Tithes are recorded on the Tithes page. Once one is, it shows here with the month it is for.",
      }}
      noun={["tithe recorded", "tithes recorded"]}
      summary={
        total.data && (
          <>
            {" "}
            · Total <strong>{formatMoney(total.data.totalCollected)}</strong>
          </>
        )
      }
      columns={[
        { header: "For month", cell: (t) => `${t.forMonth} ${t.forYear}` },
        {
          header: "Method",
          cell: (t) => <PaymentChip method={t.paymentMethod} />,
        },
        { header: "Collected", cell: (t) => formatDate(t.collectionDate) },
        {
          header: "Amount",
          cell: (t) => <strong>{formatMoney(t.amount)}</strong>,
          end: true,
        },
      ]}
    />
  );
}

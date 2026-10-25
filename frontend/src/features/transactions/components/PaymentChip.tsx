import { Chip } from "@/ui";
import type { PaymentMethod } from "../api/transactions";
import { paymentLabel } from "../model/labels";

// How something was paid, as a small label. Every list that shows money uses this one.
export function PaymentChip({ method }: { method: PaymentMethod }) {
  return <Chip>{paymentLabel[method]}</Chip>;
}

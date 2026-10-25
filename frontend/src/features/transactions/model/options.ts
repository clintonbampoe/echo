// The payment methods as a dropdown list, for any form that records money.
import type { Option } from "@/ui";
import type { PaymentMethod } from "../api/transactions";
import { paymentLabel } from "./labels";

export const paymentOptions: readonly Option[] = (
  Object.keys(paymentLabel) as PaymentMethod[]
).map((value) => ({
  value,
  label: paymentLabel[value],
}));

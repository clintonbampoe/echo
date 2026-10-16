import type { PaymentMethod } from "./api";

export const paymentLabel: Record<PaymentMethod, string> = {
  Cash: "Cash",
  MobileMoney: "Mobile money",
  Cheque: "Cheque",
  BankTransfer: "Bank transfer",
  CreditCard: "Credit card",
};

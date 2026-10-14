import type { PaymentMethod } from "./api";

export const paymentLabel: Record<PaymentMethod, string> = {
  Cash: "Cash",
  Cheque: "Cheque",
  CreditCard: "Credit card",
  MobileMoney: "Mobile money",
  BankTransfer: "Bank transfer",
};

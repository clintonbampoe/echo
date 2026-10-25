import type { ChipTone } from "@/ui";
import type { PaymentMethod, TransactionType } from "../api/transactions";

export const paymentLabel: Record<PaymentMethod, string> = {
  Cash: "Cash",
  MobileMoney: "Mobile money",
  Cheque: "Cheque",
  BankTransfer: "Bank transfer",
  CreditCard: "Credit card",
};

export const typeLabel: Record<TransactionType, string> = {
  Income: "Income",
  Expense: "Expense",
};

// Money in is marked in green, money out stays quiet: an expense is ordinary, not a warning.
export const typeTone: Record<TransactionType, ChipTone> = {
  Income: "positive",
  Expense: "neutral",
};

// What every money record shares: how it was paid. Tithes, project contributions, offerings and expenses
// all carry one of these. The transaction endpoints will live here too. No React in this file.

export type PaymentMethod =
  "Cash" | "Cheque" | "CreditCard" | "MobileMoney" | "BankTransfer";

import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { formatMoney } from "@/lib/format";
import { FormSheet } from "@/ui";
import { useCategories, useCreateTransaction } from "../hooks";
import {
  asType,
  emptyTransaction,
  nextTransaction,
  toTransactionInput,
  transactionFieldMap,
  transactionSchema,
  type TransactionValues,
} from "../transactionForm";
import { TransactionFields } from "./TransactionFields";

type Props = { open: boolean; today: string; onClose: () => void };

// The "Record transaction" sheet. The form only exists while the sheet is open, so it starts fresh every time.
export function TransactionSheet({ open, today, onClose }: Props) {
  return open ? <RecordTransaction today={today} onClose={onClose} /> : null;
}

function RecordTransaction({ today, onClose }: Omit<Props, "open">) {
  const create = useCreateTransaction();
  const categories = useCategories();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const [recorded, setRecorded] = useState<{ count: number; note: string }>({
    count: 0,
    note: "",
  });
  const form = useForm<TransactionValues>({
    resolver: zodResolver(transactionSchema),
    defaultValues: emptyTransaction(today),
  });

  async function save(values: TransactionValues, another: boolean) {
    setProblem(null);
    try {
      const saved = await create.mutateAsync(toTransactionInput(values));
      if (!another) return onClose();
      const note = `Recorded ${formatMoney(saved.amount)} as ${asType(saved.transactionType)} under ${saved.categoryName}. Type and date are kept for the next one.`;
      setRecorded({ count: recorded.count + 1, note });
      form.reset(nextTransaction(values));
    } catch (e) {
      handleSubmitError(e, form.setError, transactionFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="Record transaction"
      subtitle="Money in or money out, apart from tithes."
      onClose={onClose}
      formId="transaction-form"
      onSubmit={form.handleSubmit((v) => save(v, false))}
      submitLabel="Record transaction"
      pending={create.isPending}
      failures={Object.keys(form.formState.errors).length}
      note={recorded.note || undefined}
      problem={
        problem && {
          title: "We could not record this transaction",
          message: problem.message,
        }
      }
      another={{
        label: "Save and add another",
        onClick: () => void form.handleSubmit((v) => save(v, true))(),
      }}
    >
      <TransactionFields
        key={recorded.count}
        form={form}
        categories={categories.data}
      />
    </FormSheet>
  );
}

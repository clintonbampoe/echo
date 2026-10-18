import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { FormSheet } from "@/ui";
import type { Transaction } from "../api";
import { useCategories, useUpdateTransaction } from "../hooks";
import {
  fromTransaction,
  toTransactionUpdate,
  transactionFieldMap,
  transactionSchema,
  type TransactionValues,
} from "../transactionForm";
import { TransactionFields } from "./TransactionFields";

type Props = { transaction: Transaction; onClose: () => void };

// The "Edit transaction" sheet: the same form as Record transaction, filled with what was recorded.
export function EditTransactionSheet({ transaction, onClose }: Props) {
  const update = useUpdateTransaction();
  const categories = useCategories();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<TransactionValues>({
    resolver: zodResolver(transactionSchema),
    defaultValues: fromTransaction(transaction),
  });

  async function save(values: TransactionValues) {
    setProblem(null);
    try {
      await update.mutateAsync({
        id: transaction.id,
        ...toTransactionUpdate(values),
      });
      onClose();
    } catch (e) {
      handleSubmitError(e, form.setError, transactionFieldMap, setProblem);
    }
  }

  return (
    <FormSheet
      title="Edit transaction"
      subtitle="Changes update the totals straight away."
      onClose={onClose}
      formId="transaction-edit-form"
      onSubmit={form.handleSubmit(save)}
      submitLabel="Save changes"
      pending={update.isPending}
      failures={Object.keys(form.formState.errors).length}
      problem={
        problem && {
          title: "We could not save this transaction",
          message: problem.message,
        }
      }
    >
      <TransactionFields form={form} categories={categories.data} />
    </FormSheet>
  );
}

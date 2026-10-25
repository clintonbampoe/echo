import { useState } from "react";
import { Link } from "react-router-dom";
import { useWatch, type UseFormReturn } from "react-hook-form";
import { paths } from "@/lib/paths";
import {
  ControlledDate,
  ControlledSelect,
  Disclosure,
  Field,
  Icon,
  SegmentedField,
  TextAreaField,
} from "@/ui";
import type { Category, TransactionType } from "../api";
import {
  categoryChoices,
  clashesWith,
  NOTE_MAX,
  type TransactionValues,
} from "../transactionForm";
import styles from "./TransactionFields.module.css";

type Props = {
  form: UseFormReturn<TransactionValues>;
  categories: Category[] | undefined;
};

const types = [
  { value: "Income", label: "Income" },
  { value: "Expense", label: "Expense" },
] as const;

// Every input of the transaction form. Record and edit use the same fields. Choosing the other type clears a category
// that belongs to the first one, and says so, because an income category can hold no expense.
export function TransactionFields({ form, categories }: Props) {
  const {
    register,
    setValue,
    control,
    getValues,
    formState: { errors: e },
  } = form;
  const type = useWatch({ control, name: "transactionType" });
  const category = useWatch({ control, name: "categoryId" });
  const description = useWatch({ control, name: "description" });
  const [cleared, setCleared] = useState(false);

  function chooseType(next: TransactionType) {
    const clash = clashesWith(categories, getValues("categoryId"), next);
    setValue("transactionType", next);
    if (clash) setValue("categoryId", "");
    setCleared(clash);
  }

  return (
    <div className={styles.fields}>
      <SegmentedField
        label="Type"
        options={types}
        value={type}
        onChange={chooseType}
        joined
      />
      <Field
        label="Amount"
        unit="GH₵"
        inputMode="decimal"
        autoComplete="off"
        placeholder="0.00"
        error={e.amount?.message}
        {...register("amount")}
      />
      <div>
        <ControlledSelect
          control={control}
          name="categoryId"
          label="Category"
          placeholder="Choose a category"
          options={categoryChoices(categories, type)}
        />
        {cleared && !category ? (
          <p className={styles.cleared}>
            <Icon name="alert" size={14} />
            Category cleared because it belongs to the other type. Choose again.
          </p>
        ) : (
          <p className={styles.hint}>
            Showing {type === "Income" ? "income" : "expense"} categories only.{" "}
            <Link to={paths.financeCategories}>Manage categories</Link>
          </p>
        )}
      </div>
      <ControlledDate control={control} name="transactionDate" label="Date" />
      <Disclosure
        title="More details"
        hint="description"
        forceOpen={!!e.description || !!description}
      >
        <TextAreaField
          label="Description"
          optional
          placeholder="e.g. ECG bill for September"
          maxLength={NOTE_MAX}
          error={e.description?.message}
          {...register("description")}
        />
      </Disclosure>
    </div>
  );
}

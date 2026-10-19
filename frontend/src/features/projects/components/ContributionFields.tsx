import { useWatch, type UseFormReturn } from "react-hook-form";
import { paymentOptions } from "@/features/transactions";
import {
  ControlledDate,
  ControlledSelect,
  Field,
  FormSection,
  TextAreaField,
} from "@/ui";
import { NOTE_MAX, type ContributionValues } from "../contributionForm";
import styles from "./ContributionFields.module.css";

type Props = {
  form: UseFormReturn<ContributionValues>;
  today: string;
  projectName: string;
};

// Every input of the contribution form. Record and edit use the same fields. The project is shown, not chosen: a gift
// is recorded from a project's page, and cannot be moved to another one. The payment method is a fixed list, so it is
// a dropdown (design rule S2), like on Tithes.
export function ContributionFields({ form, today, projectName }: Props) {
  const {
    register,
    control,
    formState: { errors: e },
  } = form;
  const note = useWatch({ control, name: "description" });
  return (
    <div className={styles.fields}>
      <FormSection>
        <div className={styles.full}>
          <Field label="Project" value={projectName} readOnly disabled />
        </div>
        <Field
          label="Amount"
          unit="GH₵"
          inputMode="decimal"
          autoComplete="off"
          placeholder="0.00"
          error={e.amount?.message}
          {...register("amount")}
        />
        <ControlledDate
          control={control}
          name="dateContributed"
          label="Date"
          max={today}
        />
      </FormSection>
      <FormSection>
        <div className={styles.full}>
          <ControlledSelect
            control={control}
            name="paymentMethod"
            label="Payment method"
            options={paymentOptions}
          />
        </div>
        <div className={styles.full}>
          <TextAreaField
            label="Note"
            optional
            placeholder="e.g. Sunday appeal. No names are recorded."
            maxLength={NOTE_MAX}
            count={note.length}
            error={e.description?.message}
            {...register("description")}
          />
        </div>
      </FormSection>
    </div>
  );
}

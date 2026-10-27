import type { ReactNode } from "react";
import { useWatch, type UseFormReturn } from "react-hook-form";
import { MemberPicker, type MemberMatch } from "@/features/members";
import { paymentOptions } from "@/features/transactions";
import {
  ControlledDate,
  ControlledSelect,
  Field,
  FormSection,
  TextAreaField,
} from "@/ui";
import {
  monthOptions,
  NOTE_MAX,
  periodOf,
  yearOptions,
  type TitheValues,
} from "../../forms/schema";
import { MemberCard } from "../profile/MemberCard";
import styles from "./TitheFields.module.css";

type Props = {
  form: UseFormReturn<TitheValues>;
  today: string;
  // Recording: the member picked so far, and how to pick or change them.
  member: (Pick<MemberMatch, "id" | "name"> & { phoneNumber?: string }) | null;
  // Left out on an existing tithe, whose member is fixed: they are shown with a link to their page (rule S6).
  onMember?: (member: MemberMatch | null) => void;
  // Shown under the member, e.g. a heads-up that they already gave for the month below.
  underMember?: ReactNode;
};

// Every input of the tithe form. Record and edit use the same fields.
export function TitheFields({
  form,
  today,
  member,
  onMember,
  underMember,
}: Props) {
  const {
    register,
    setValue,
    control,
    formState: { errors: e, dirtyFields },
  } = form;
  const note = useWatch({ control, name: "description" });
  const year = useWatch({ control, name: "forYear" });
  // The month follows the day it was received until someone picks a month themselves.
  const follow = (day: string) => {
    if (dirtyFields.forMonth || dirtyFields.forYear || !day) return;
    const period = periodOf(day);
    setValue("forMonth", period.forMonth);
    setValue("forYear", period.forYear);
  };
  return (
    <div className={styles.fields}>
      <FormSection title="Member">
        <div className={styles.full}>
          {onMember ? (
            <MemberPicker
              value={member && { phoneNumber: "", ...member }}
              onChange={onMember}
              error={e.memberId?.message}
            />
          ) : (
            member && <MemberCard id={member.id} name={member.name} />
          )}
        </div>
        {underMember && <div className={styles.full}>{underMember}</div>}
      </FormSection>
      <FormSection>
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
          name="collectionDate"
          label="Collected on"
          max={today}
          onPick={follow}
        />
      </FormSection>
      <FormSection title="Tithe for">
        <ControlledSelect
          control={control}
          name="forMonth"
          label="Month"
          options={monthOptions}
        />
        <ControlledSelect
          control={control}
          name="forYear"
          label="Year"
          options={yearOptions(today, year)}
        />
        <p className={`${styles.full} ${styles.hint}`}>
          The month it counts toward, which can differ from the day you received
          it.
        </p>
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
            placeholder="e.g. Cheque number, or who gave on their behalf"
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

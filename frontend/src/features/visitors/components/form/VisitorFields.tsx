import type { UseFormReturn } from "react-hook-form";
import { genderChoices } from "@/features/members";
import { ControlledSegmented, Field, FormSection, TextAreaField } from "@/ui";
import { NOTES_MAX, type VisitorValues } from "../../forms/schema";
import styles from "./VisitorFields.module.css";

type Props = { form: UseFormReturn<VisitorValues> };

// Every input of the visitor form, in sections. Add and edit use the same fields.
export function VisitorFields({ form }: Props) {
  const {
    register,
    watch,
    formState: { errors: e },
  } = form;
  return (
    <div className={styles.fields}>
      <FormSection title="Name">
        <Field
          label="First name"
          maxLength={100}
          error={e.firstName?.message}
          {...register("firstName")}
        />
        <Field
          label="Last name"
          maxLength={100}
          error={e.lastName?.message}
          {...register("lastName")}
        />
        <div className={styles.full}>
          <ControlledSegmented
            control={form.control}
            name="gender"
            label="Gender"
            options={genderChoices}
          />
        </div>
      </FormSection>
      <FormSection title="Contact">
        <Field
          label="Phone"
          optional
          type="tel"
          inputMode="tel"
          maxLength={20}
          placeholder="024 000 0000"
          error={e.phoneNumber?.message}
          {...register("phoneNumber")}
        />
        <Field
          label="Email"
          optional
          type="email"
          maxLength={255}
          placeholder="name@example.com"
          error={e.emailAddress?.message}
          {...register("emailAddress")}
        />
      </FormSection>
      <FormSection title="Follow-up">
        <div className={styles.full}>
          <TextAreaField
            label="Notes"
            optional
            maxLength={NOTES_MAX}
            count={watch("notes").length}
            error={e.notes?.message}
            {...register("notes")}
          />
        </div>
      </FormSection>
    </div>
  );
}

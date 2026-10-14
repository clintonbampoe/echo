import type { UseFormReturn } from "react-hook-form";
import { Field, FormSection, TextAreaField } from "@/ui";
import { NOTES_MAX, type VisitorValues } from "./schema";
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
          autoComplete="given-name"
          maxLength={100}
          error={e.firstName?.message}
          {...register("firstName")}
        />
        <Field
          label="Last name"
          autoComplete="family-name"
          maxLength={100}
          error={e.lastName?.message}
          {...register("lastName")}
        />
      </FormSection>
      <FormSection title="Contact">
        <Field
          label="Phone"
          optional
          type="tel"
          inputMode="tel"
          autoComplete="tel"
          maxLength={20}
          placeholder="024 000 0000"
          error={e.phoneNumber?.message}
          {...register("phoneNumber")}
        />
        <Field
          label="Email"
          optional
          type="email"
          autoComplete="email"
          maxLength={255}
          placeholder="name@example.com"
          error={e.emailAddress?.message}
          {...register("emailAddress")}
        />
        <div className={styles.full}>
          <Field
            label="Date of birth"
            optional
            type="date"
            autoComplete="bday"
            error={e.dateOfBirth?.message}
            {...register("dateOfBirth")}
          />
        </div>
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

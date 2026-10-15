import type { UseFormReturn } from "react-hook-form";
import { Field, TextAreaField } from "@/ui";
import { DESCRIPTION_MAX, NAME_MAX, type OrganizationValues } from "./schema";
import styles from "./OrganizationFields.module.css";

type Props = { form: UseFormReturn<OrganizationValues> };

// Every input of the organization form. Add and edit use the same fields.
export function OrganizationFields({ form }: Props) {
  const {
    register,
    watch,
    formState: { errors: e },
  } = form;
  return (
    <div className={styles.fields}>
      <Field
        label="Name"
        maxLength={NAME_MAX}
        placeholder="e.g. Church Choir"
        error={e.name?.message}
        {...register("name")}
      />
      <TextAreaField
        label="Description"
        optional
        maxLength={DESCRIPTION_MAX}
        count={watch("description").length}
        error={e.description?.message}
        {...register("description")}
      />
    </div>
  );
}

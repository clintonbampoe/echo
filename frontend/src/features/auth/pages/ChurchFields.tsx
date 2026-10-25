import type { UseFormReturn } from "react-hook-form";
import { congregationTypes } from "@/features/account";
import { regions } from "@/lib/regions";
import { ControlledSelect, Field } from "@/ui";
import type { ChurchValues } from "../forms/schema";
import styles from "./form.module.css";

// The church form's inputs.
export function ChurchFields({ form }: { form: UseFormReturn<ChurchValues> }) {
  const {
    register,
    control,
    formState: { errors },
  } = form;
  return (
    <>
      <Field
        label="Church name"
        autoComplete="organization"
        maxLength={100}
        error={errors.name?.message}
        {...register("name")}
      />
      <ControlledSelect
        control={control}
        name="orgType"
        label="Type"
        options={congregationTypes}
      />
      <div className={styles.row}>
        <Field
          label="Phone"
          type="tel"
          autoComplete="tel"
          maxLength={20}
          placeholder="024 000 0000"
          error={errors.phoneNumber?.message}
          {...register("phoneNumber")}
        />
        <Field
          label="Church email"
          type="email"
          maxLength={255}
          placeholder="office@church.org"
          error={errors.churchEmail?.message}
          {...register("churchEmail")}
        />
      </div>
      <ControlledSelect
        control={control}
        name="region"
        label="Region"
        options={regions}
      />
      <div className={styles.row}>
        <Field
          label="City"
          maxLength={100}
          error={errors.city?.message}
          {...register("city")}
        />
        <Field
          label="Town"
          maxLength={100}
          error={errors.town?.message}
          {...register("town")}
        />
      </div>
      <Field
        label="Digital address"
        maxLength={255}
        placeholder="GA-123-4567"
        hint="Your Ghana Post GPS address."
        error={errors.gpsAddress?.message}
        {...register("gpsAddress")}
      />
    </>
  );
}

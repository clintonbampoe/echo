import type { UseFormReturn } from "react-hook-form";
import { congregationTypes } from "../model/options";
import { regions } from "@/lib/regions";
import { ControlledSelect, Field } from "@/ui";
import type { ChurchValues } from "../forms/schema";
import styles from "./form.module.css";

export function ChurchFields({
  form,
  part,
}: {
  form: UseFormReturn<ChurchValues>;
  // Which group to show (0 to 2). Leave it out to show every group.
  part?: number;
}) {
  const {
    register,
    control,
    formState: { errors },
  } = form;
  const shows = (i: number) => part === undefined || part === i;
  return (
    <>
      {shows(0) && (
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
        </>
      )}
      {shows(1) && (
        <div className={`${styles.row} ${styles.stackOnPhone}`}>
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
      )}
      {shows(2) && (
        <>
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
      )}
    </>
  );
}

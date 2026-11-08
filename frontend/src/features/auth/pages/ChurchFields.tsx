import type { UseFormReturn } from "react-hook-form";
import { congregationTypes } from "../model/options";
import { regions } from "@/lib/regions";
import { ControlledSelect, Field } from "@/ui";
import type { ChurchValues } from "../forms/schema";
import styles from "./form.module.css";

type Form = UseFormReturn<ChurchValues>;

function Type({ form }: { form: Form }) {
  return (
    <ControlledSelect
      control={form.control}
      name="orgType"
      label="Type"
      options={congregationTypes}
    />
  );
}

function Region({ form }: { form: Form }) {
  return (
    <ControlledSelect
      control={form.control}
      name="region"
      label="Region"
      options={regions}
    />
  );
}

function Name({ form }: { form: Form }) {
  const { register, formState } = form;
  return (
    <Field
      label="Church name"
      autoComplete="organization"
      maxLength={100}
      error={formState.errors.name?.message}
      {...register("name")}
    />
  );
}

function Contact({ form }: { form: Form }) {
  const { register, formState } = form;
  const { errors } = formState;
  return (
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
  );
}

function Place({ form }: { form: Form }) {
  const { register, formState } = form;
  const { errors } = formState;
  return (
    <>
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

// The church details. On a phone they come a group at a time (`part` 0 to 2). Left out, every group shows at
// once, and the name and address take a line each while the rest pair up: type and region, phone and email,
// city and town.
export function ChurchFields({ form, part }: { form: Form; part?: number }) {
  if (part === undefined) {
    return (
      <>
        <Name form={form} />
        <div className={styles.row}>
          <Type form={form} />
          <Region form={form} />
        </div>
        <Contact form={form} />
        <Place form={form} />
      </>
    );
  }
  return (
    <>
      {part === 0 && (
        <>
          <Name form={form} />
          <Type form={form} />
        </>
      )}
      {part === 1 && <Contact form={form} />}
      {part === 2 && (
        <>
          <Region form={form} />
          <Place form={form} />
        </>
      )}
    </>
  );
}

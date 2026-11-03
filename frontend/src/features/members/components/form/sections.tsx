import type { UseFormReturn } from "react-hook-form";
import { regions } from "@/lib/regions";
import { withAny } from "@/lib/options";
import {
  ControlledDate,
  ControlledSegmented,
  ControlledSelect,
  Field,
  FormSection,
} from "@/ui";
import {
  genderChoices,
  optionalMaritalChoices,
  statusChoices,
} from "../../model/options";
import type { MemberValues } from "../../forms/schema";
import styles from "./MemberFields.module.css";

const regionOptions = withAny("Not set", regions);

export type Form = { form: UseFormReturn<MemberValues> };

export function Name({ form }: Form) {
  const {
    register,
    formState: { errors: e },
  } = form;
  return (
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
      <div className={styles.full}>
        <Field
          label="Other names"
          optional
          autoComplete="additional-name"
          maxLength={100}
          error={e.otherNames?.message}
          {...register("otherNames")}
        />
      </div>
    </FormSection>
  );
}

export function PhoneField({ form }: Form) {
  return (
    <Field
      label="Phone"
      type="tel"
      inputMode="tel"
      autoComplete="tel"
      maxLength={20}
      placeholder="024 000 0000"
      error={form.formState.errors.phoneNumber?.message}
      {...form.register("phoneNumber")}
    />
  );
}

export function EmailField({ form }: Form) {
  return (
    <Field
      label="Email"
      optional
      type="email"
      autoComplete="email"
      maxLength={255}
      placeholder="name@example.com"
      error={form.formState.errors.emailAddress?.message}
      {...form.register("emailAddress")}
    />
  );
}

export function Contact({ form }: Form) {
  return (
    <FormSection title="Contact">
      <PhoneField form={form} />
      <EmailField form={form} />
    </FormSection>
  );
}

export function Personal({ form }: Form) {
  return (
    <FormSection title="Personal">
      <ControlledDate
        control={form.control}
        name="dateOfBirth"
        label="Date of birth"
        kind="birth"
      />
      <ControlledDate
        control={form.control}
        name="joinedDate"
        label="Joined church"
        optional
      />
      <ControlledSegmented
        control={form.control}
        name="gender"
        label="Gender"
        options={genderChoices}
      />
      <ControlledSelect
        control={form.control}
        name="maritalStatus"
        label="Marital status"
        optional
        options={optionalMaritalChoices}
      />
      <ControlledSelect
        control={form.control}
        name="status"
        label="Status"
        options={statusChoices}
      />
    </FormSection>
  );
}

export function Address({ form }: Form) {
  const {
    register,
    formState: { errors: e },
  } = form;
  return (
    <FormSection title="Address">
      <div className={styles.full}>
        <Field
          label="Residential address"
          optional
          autoComplete="street-address"
          maxLength={255}
          placeholder="House number, street, area"
          error={e.residentialAddress?.message}
          {...register("residentialAddress")}
        />
      </div>
      <Field
        label="City"
        optional
        autoComplete="address-level2"
        maxLength={100}
        error={e.city?.message}
        {...register("city")}
      />
      <Field
        label="Hometown"
        optional
        maxLength={100}
        error={e.hometown?.message}
        {...register("hometown")}
      />
      <ControlledSelect
        control={form.control}
        name="region"
        label="Region"
        optional
        options={regionOptions}
      />
      <Field
        label="Digital address"
        optional
        maxLength={255}
        placeholder="GA-123-4567"
        error={e.gpsAddress?.message}
        {...register("gpsAddress")}
      />
    </FormSection>
  );
}

export function Emergency({ form }: Form) {
  const {
    register,
    formState: { errors: e },
  } = form;
  return (
    <FormSection title="Emergency contact">
      <div className={styles.full}>
        <Field
          label="Next of kin"
          optional
          maxLength={100}
          error={e.nextOfKin?.message}
          {...register("nextOfKin")}
        />
      </div>
      <Field
        label="Contact name"
        optional
        maxLength={100}
        error={e.emergencyContactName?.message}
        {...register("emergencyContactName")}
      />
      <Field
        label="Contact phone"
        optional
        type="tel"
        inputMode="tel"
        maxLength={20}
        placeholder="024 000 0000"
        error={e.emergencyContactPhoneNumber?.message}
        {...register("emergencyContactPhoneNumber")}
      />
    </FormSection>
  );
}

import type { UseFormReturn } from "react-hook-form";
import { regions } from "@/lib/regions";
import { ControlledDate, ControlledSelect, Field, FormSection } from "@/ui";
import { genderChoices, maritalChoices, statusChoices } from "./options";
import type { MemberValues } from "./schema";
import styles from "./MemberFields.module.css";

type Form = { form: UseFormReturn<MemberValues> };

function Name({ form }: Form) {
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

function Contact({ form }: Form) {
  const {
    register,
    formState: { errors: e },
  } = form;
  return (
    <FormSection title="Contact">
      <Field
        label="Phone"
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
    </FormSection>
  );
}

function Personal({ form }: Form) {
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
      <ControlledSelect
        control={form.control}
        name="gender"
        label="Gender"
        options={genderChoices}
      />
      <ControlledSelect
        control={form.control}
        name="maritalStatus"
        label="Marital status"
        options={maritalChoices}
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

function Address({ form }: Form) {
  const {
    register,
    formState: { errors: e },
  } = form;
  return (
    <FormSection title="Address">
      <div className={styles.full}>
        <Field
          label="Residential address"
          autoComplete="street-address"
          maxLength={255}
          placeholder="House number, street, area"
          error={e.residentialAddress?.message}
          {...register("residentialAddress")}
        />
      </div>
      <Field
        label="City"
        autoComplete="address-level2"
        maxLength={100}
        error={e.city?.message}
        {...register("city")}
      />
      <Field
        label="Hometown"
        maxLength={100}
        error={e.hometown?.message}
        {...register("hometown")}
      />
      <ControlledSelect
        control={form.control}
        name="region"
        label="Region"
        options={regions}
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

function Emergency({ form }: Form) {
  const {
    register,
    formState: { errors: e },
  } = form;
  return (
    <FormSection title="Emergency contact">
      <div className={styles.full}>
        <Field
          label="Next of kin"
          maxLength={100}
          error={e.nextOfKin?.message}
          {...register("nextOfKin")}
        />
      </div>
      <Field
        label="Contact name"
        maxLength={100}
        error={e.emergencyContactName?.message}
        {...register("emergencyContactName")}
      />
      <Field
        label="Contact phone"
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

// Every input of the member form, in sections. Create and edit use the same fields.
export function MemberFields({ form }: Form) {
  return (
    <div className={styles.fields}>
      <Name form={form} />
      <Contact form={form} />
      <Personal form={form} />
      <Address form={form} />
      <Emergency form={form} />
    </div>
  );
}

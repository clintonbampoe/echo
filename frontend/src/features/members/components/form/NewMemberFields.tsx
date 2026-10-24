import { useState } from "react";
import { Controller } from "react-hook-form";
import { ControlledDate, ControlledSelect, Disclosure, Field, FormSection, SegmentedField } from "@/ui";
import { joinName, splitName } from "../../forms/mapping";
import { maritalChoices } from "../../model/options";
import styles from "./MemberFields.module.css";
import {
  Address,
  Emergency,
  EmailField,
  PhoneField,
  type Form,
} from "./sections";

const genders = [
  { value: "Female", label: "Female" },
  { value: "Male", label: "Male" },
  { value: "Other", label: "Other" },
] as const;

// The fields the server insists on that the short form does not lead with. A problem in any of them opens the fold.
const folded = [
  "emailAddress",
  "maritalStatus",
  "residentialAddress",
  "city",
  "hometown",
  "region",
  "nextOfKin",
  "emergencyContactName",
  "emergencyContactPhoneNumber",
] as const;

// One box for the whole name, kept in step with the form's first, other and last names.
function FullName({ form }: Form) {
  const { errors } = form.formState;
  const [text, setText] = useState("");
  const [first, other, last] = form.watch([
    "firstName",
    "otherNames",
    "lastName",
  ]);
  // The form was reset (after "Save and add another"): the box empties with it.
  if (joinName(first, other, last) === "" && text.trim() !== "") setText("");

  function change(value: string) {
    setText(value);
    const parts = splitName(value);
    const options = { shouldDirty: true, shouldValidate: form.formState.isSubmitted };
    form.setValue("firstName", parts.firstName, options);
    form.setValue("otherNames", parts.otherNames, options);
    form.setValue("lastName", parts.lastName, options);
  }

  // `ref` is registered on the first name, so the form can focus this box and scroll to it when it fails.
  const { ref } = form.register("firstName");
  return (
    <Field
      ref={ref}
      label="Full name"
      autoComplete="name"
      maxLength={200}
      placeholder="Abena Mensah"
      value={text}
      onChange={(e) => change(e.target.value)}
      error={(errors.firstName ?? errors.lastName)?.message}
    />
  );
}

// The short "Add member" form: the few things a front-desk person has to hand, with everything else folded
// away. The server still needs the folded fields, so a problem in one opens the fold.
export function NewMemberFields({ form }: Form) {
  const { errors } = form.formState;
  const needsAttention = folded.some((name) => errors[name]);
  return (
    <div className={styles.fields}>
      <FormSection>
        <div className={styles.full}>
          <FullName form={form} />
        </div>
        <div className={styles.full}>
          <PhoneField form={form} />
        </div>
        <div className={styles.full}>
          <Controller
            control={form.control}
            name="gender"
            render={({ field }) => (
              <SegmentedField
                label="Gender"
                options={genders}
                value={field.value}
                onChange={field.onChange}
                joined
                stretch
              />
            )}
          />
        </div>
        <div className={styles.full}>
          <ControlledDate
            control={form.control}
            name="dateOfBirth"
            label="Date of birth"
            kind="birth"
          />
        </div>
      </FormSection>
      <Disclosure
        title="More details"
        hint="email, address, marital status"
        forceOpen={needsAttention}
      >
        <FormSection>
          <EmailField form={form} />
          <ControlledSelect
            control={form.control}
            name="maritalStatus"
            label="Marital status"
            options={maritalChoices}
          />
        </FormSection>
        <Address form={form} />
        <Emergency form={form} />
      </Disclosure>
    </div>
  );
}

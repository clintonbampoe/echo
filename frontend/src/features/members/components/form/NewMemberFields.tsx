import {
  ControlledDate,
  ControlledSegmented,
  ControlledSelect,
  Disclosure,
  FormSection,
} from "@/ui";
import { genderChoices, optionalMaritalChoices } from "../../model/options";
import styles from "./MemberFields.module.css";
import {
  Address,
  Emergency,
  EmailField,
  Name,
  PhoneField,
  type Form,
} from "./sections";

// The fields the short form does not lead with. A problem in any of them opens the fold.
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

// The short "Add member" form: the few things a front-desk person has to hand, with everything else folded
// away. A problem in a folded field (a badly typed email, say) opens the fold.
export function NewMemberFields({ form }: Form) {
  const { errors } = form.formState;
  const needsAttention = folded.some((name) => errors[name]);
  return (
    <div className={styles.fields}>
      <Name form={form} />
      <FormSection>
        <div className={styles.full}>
          <PhoneField form={form} />
        </div>
        <div className={styles.full}>
          <ControlledSegmented
            control={form.control}
            name="gender"
            label="Gender"
            options={genderChoices}
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
            optional
            options={optionalMaritalChoices}
          />
        </FormSection>
        <Address form={form} />
        <Emergency form={form} />
      </Disclosure>
    </div>
  );
}

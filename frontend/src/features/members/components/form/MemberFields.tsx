import { Address, Contact, Emergency, Name, Personal, type Form } from "./sections";
import styles from "./MemberFields.module.css";

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

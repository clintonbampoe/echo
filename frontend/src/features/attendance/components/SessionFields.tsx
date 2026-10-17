import { useWatch, type UseFormReturn } from "react-hook-form";
import {
  ControlledDate,
  ControlledSelect,
  ControlledTime,
  Field,
  FormSection,
  TextAreaField,
  type Option,
} from "@/ui";
import {
  NAME_MAX,
  NOTES_MAX,
  TOPIC_MAX,
  type SessionValues,
} from "../sessionForm";
import styles from "./SessionFields.module.css";

type Props = {
  form: UseFormReturn<SessionValues>;
  types: readonly Option[];
  // On an existing session the type and the day are fixed: shown as text, with why.
  fixed?: string;
};

// Every input of the session form. New and Edit use the same fields.
export function SessionFields({ form, types, fixed }: Props) {
  const {
    register,
    control,
    formState: { errors: e },
  } = form;
  const notes = useWatch({ control, name: "notes" });
  return (
    <div className={styles.fields}>
      <FormSection title="Session">
        {fixed ? (
          <>
            <p className={`${styles.full} ${styles.fixed}`}>{fixed}</p>
            <p className={`${styles.full} ${styles.hint}`}>
              The type and date cannot be changed. If either is wrong, delete
              this session and create a new one.
            </p>
          </>
        ) : (
          <>
            <div className={styles.full}>
              <ControlledSelect
                control={control}
                name="sessionLabelId"
                label="Session type"
                options={types}
              />
            </div>
            {types.length <= 1 && (
              <p className={`${styles.full} ${styles.hint}`}>
                There are no session types yet. Add one under Session types
                first.
              </p>
            )}
            <div className={styles.full}>
              <ControlledDate control={control} name="date" label="Date" />
            </div>
          </>
        )}
        <div className={styles.full}>
          <Field
            label="Name"
            optional
            placeholder="e.g. Christmas Eve Service"
            maxLength={NAME_MAX}
            error={e.name?.message}
            {...register("name")}
          />
        </div>
        <div className={styles.full}>
          <Field
            label="Topic"
            optional
            placeholder="e.g. Walking in Faith"
            maxLength={TOPIC_MAX}
            error={e.topic?.message}
            {...register("topic")}
          />
        </div>
      </FormSection>
      <FormSection title="Time">
        <ControlledTime
          control={control}
          name="startsAt"
          label="Starts at"
          optional
        />
        <ControlledTime
          control={control}
          name="endsAt"
          label="Ends at"
          optional
        />
      </FormSection>
      <FormSection>
        <div className={styles.full}>
          <TextAreaField
            label="Notes"
            optional
            maxLength={NOTES_MAX}
            count={notes.length}
            error={e.notes?.message}
            {...register("notes")}
          />
        </div>
      </FormSection>
    </div>
  );
}

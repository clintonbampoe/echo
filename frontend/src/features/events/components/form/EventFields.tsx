import { useWatch, type UseFormReturn } from "react-hook-form";
import { MemberPicker, type MemberMatch } from "@/features/members";
import { useOrganizationChoices } from "@/features/organizations";
import {
  ControlledDate,
  ControlledSelect,
  ControlledTime,
  Field,
  FormSection,
  TextAreaField,
} from "@/ui";
import {
  DESCRIPTION_MAX,
  LOCATION_MAX,
  NAME_MAX,
  type EventValues,
} from "../../forms/schema";
import styles from "./EventFields.module.css";

type Form = UseFormReturn<EventValues>;
type Organizer =
  (Pick<MemberMatch, "id" | "name"> & { phoneNumber?: string }) | null;

type Props = {
  form: Form;
  // The organizer picked so far, and how to pick or change them.
  organizer: Organizer;
  onOrganizer: (member: MemberMatch | null) => void;
};

// What the event is called, which group is responsible and who is in charge.
function Basics({ form, organizer, onOrganizer }: Props) {
  const {
    register,
    control,
    formState: { errors: e },
  } = form;
  const groups = useOrganizationChoices();
  const options = [
    { value: "", label: "Choose a group" },
    ...groups.map((g) => ({ value: g.id, label: g.name })),
  ];
  return (
    <FormSection>
      <div className={styles.full}>
        <Field
          label="Name"
          maxLength={NAME_MAX}
          placeholder="e.g. Annual Convention"
          error={e.name?.message}
          {...register("name")}
        />
      </div>
      <div className={styles.full}>
        <ControlledSelect
          control={control}
          name="organizationId"
          label="Organization"
          options={options}
        />
        <p className={styles.hint}>The group responsible for the event.</p>
      </div>
      <div className={styles.full}>
        <p className={styles.label}>Organizer</p>
        <MemberPicker
          value={organizer && { phoneNumber: "", ...organizer }}
          onChange={onOrganizer}
          error={e.organizerId?.message}
        />
        <p className={styles.hint}>The member in charge of the event.</p>
      </div>
    </FormSection>
  );
}

// The days and times. The end follows the start until it would fall before it, so a one-day event needs one pick.
function When({ form }: { form: Form }) {
  const { control, getValues, setValue } = form;
  const followStart = (day: string) => {
    const end = getValues("endDate");
    if (!end || end < day) setValue("endDate", day, { shouldDirty: true });
  };
  return (
    <FormSection title="When">
      <ControlledDate
        control={control}
        name="startDate"
        label="Starts on"
        onPick={followStart}
      />
      <ControlledDate control={control} name="endDate" label="Ends on" />
      <ControlledTime
        control={control}
        name="startTime"
        label="Starts at"
        optional
      />
      <ControlledTime
        control={control}
        name="endTime"
        label="Ends at"
        optional
      />
    </FormSection>
  );
}

function Details({ form }: { form: Form }) {
  const {
    register,
    control,
    formState: { errors: e },
  } = form;
  const description = useWatch({ control, name: "description" });
  return (
    <FormSection title="Details">
      <div className={styles.full}>
        <Field
          label="Location"
          optional
          maxLength={LOCATION_MAX}
          placeholder="e.g. Main Auditorium"
          error={e.location?.message}
          {...register("location")}
        />
      </div>
      <div className={styles.full}>
        <Field
          label="Capacity"
          optional
          inputMode="numeric"
          autoComplete="off"
          placeholder="No limit"
          error={e.capacity?.message}
          {...register("capacity")}
        />
      </div>
      <div className={styles.full}>
        <TextAreaField
          label="Description"
          optional
          placeholder="What is this event about?"
          maxLength={DESCRIPTION_MAX}
          count={description.length}
          error={e.description?.message}
          {...register("description")}
        />
      </div>
    </FormSection>
  );
}

// Every input of the event form. New and Edit use the same fields.
export function EventFields({ form, organizer, onOrganizer }: Props) {
  return (
    <div className={styles.fields}>
      <Basics form={form} organizer={organizer} onOrganizer={onOrganizer} />
      <When form={form} />
      <Details form={form} />
    </div>
  );
}

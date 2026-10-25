import { Link } from "react-router-dom";
import { useWatch, type UseFormReturn } from "react-hook-form";
import { paths } from "@/lib/paths";
import { MemberPicker, type MemberMatch } from "@/features/members";
import { ControlledDate, ControlledSelect, Field, TextAreaField } from "@/ui";
import type { ProjectCategory } from "../../api/categories";
import {
  categoryChoices,
  DESCRIPTION_MAX,
  NAME_MAX,
  statusChoices,
  type ProjectValues,
} from "../../forms/project";
import styles from "./ProjectFields.module.css";

type Manager =
  (Pick<MemberMatch, "id" | "name"> & { phoneNumber?: string }) | null;

type Props = {
  form: UseFormReturn<ProjectValues>;
  categories: ProjectCategory[] | undefined;
  // The manager picked so far, and how to pick or change them.
  manager: Manager;
  onManager: (member: MemberMatch | null) => void;
};

// Every input of the project form. New and Edit use the same fields. Status is chosen by hand and never changes on
// its own, which the hint says.
export function ProjectFields({ form, categories, manager, onManager }: Props) {
  const {
    register,
    control,
    formState: { errors: e },
  } = form;
  const description = useWatch({ control, name: "description" });
  return (
    <div className={styles.fields}>
      <Field
        label="Project name"
        maxLength={NAME_MAX}
        placeholder="e.g. Church roof renovation"
        error={e.name?.message}
        {...register("name")}
      />
      <div>
        <ControlledSelect
          control={control}
          name="categoryId"
          label="Category"
          placeholder="Choose a category"
          options={categoryChoices(categories)}
        />
        <p className={styles.hint}>
          <Link to={paths.projectCategories}>Manage categories</Link>
        </p>
      </div>
      <div className={styles.pair}>
        <Field
          label="Target amount"
          unit="GH₵"
          inputMode="decimal"
          autoComplete="off"
          placeholder="0.00"
          error={e.targetAmount?.message}
          {...register("targetAmount")}
        />
        <ControlledSelect
          control={control}
          name="status"
          label="Status"
          options={statusChoices}
        />
      </div>
      <div className={styles.pair}>
        <ControlledDate control={control} name="startDate" label="Start date" />
        <ControlledDate
          control={control}
          name="endDate"
          label="End date"
          optional
        />
      </div>
      <div>
        <p className={styles.label}>Manager</p>
        <MemberPicker
          value={manager && { phoneNumber: "", ...manager }}
          onChange={onManager}
          error={e.managerId?.message}
        />
        <p className={styles.hint}>The member who looks after the project.</p>
      </div>
      <TextAreaField
        label="Description"
        optional
        placeholder="What is it for?"
        maxLength={DESCRIPTION_MAX}
        count={description.length}
        error={e.description?.message}
        {...register("description")}
      />
    </div>
  );
}

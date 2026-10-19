import { Link } from "react-router-dom";
import { useWatch, type UseFormReturn } from "react-hook-form";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { ControlledDate, ControlledSelect, Field, TextAreaField } from "@/ui";
import type { AssetCategory } from "../api";
import {
  categoryChoices,
  DESCRIPTION_MAX,
  NAME_MAX,
  SERIAL_MAX,
  statusChoices,
  type AssetValues,
} from "../assetForm";
import styles from "./AssetFields.module.css";

type Props = {
  form: UseFormReturn<AssetValues>;
  categories: AssetCategory[] | undefined;
};

// Every input of the asset form. New and Edit use the same fields. The value is typed in by hand: nothing depreciates
// it, which the sheet's subtitle says.
export function AssetFields({ form, categories }: Props) {
  const {
    register,
    control,
    formState: { errors: e },
  } = form;
  const description = useWatch({ control, name: "description" });
  return (
    <div className={styles.fields}>
      <Field
        label="Asset name"
        maxLength={NAME_MAX}
        placeholder="e.g. Mixing desk"
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
          <Link to={paths.assetCategories}>Manage categories</Link>
        </p>
      </div>
      <div className={styles.pair}>
        <ControlledSelect
          control={control}
          name="status"
          label="Status"
          options={statusChoices}
        />
        <Field
          label="Serial number"
          optional
          maxLength={SERIAL_MAX}
          autoComplete="off"
          error={e.serialNumber?.message}
          {...register("serialNumber")}
        />
      </div>
      <div className={styles.pair}>
        <ControlledDate
          control={control}
          name="purchaseDate"
          label="Purchase date"
          optional
          max={todayText()}
        />
        <Field
          label="Purchase cost"
          unit="GH₵"
          inputMode="decimal"
          autoComplete="off"
          placeholder="0.00"
          error={e.purchaseCost?.message}
          {...register("purchaseCost")}
        />
      </div>
      <div className={styles.pair}>
        <Field
          label="Current value"
          unit="GH₵"
          inputMode="decimal"
          autoComplete="off"
          placeholder="0.00"
          error={e.currentValue?.message}
          {...register("currentValue")}
        />
      </div>
      <TextAreaField
        label="Description"
        optional
        placeholder="Anything worth knowing about it"
        maxLength={DESCRIPTION_MAX}
        count={description.length}
        error={e.description?.message}
        {...register("description")}
      />
    </div>
  );
}

import { Button } from "@/ui";
import styles from "./ChurchButtons.module.css";

type Props = {
  part: number;
  // Not the last part on a phone: the button goes on to the next part instead of creating the church.
  partial: boolean;
  phone: boolean;
  loading: boolean;
  onBack: () => void;
};

// The buttons under the church form. On a phone, Back and Continue sit in equal halves.
export function ChurchButtons({
  part,
  partial,
  phone,
  loading,
  onBack,
}: Props) {
  return (
    <div className={styles.buttons}>
      {phone && part > 0 && (
        <Button
          type="button"
          variant="secondary"
          size="lg"
          pill
          block
          onClick={onBack}
        >
          Back
        </Button>
      )}
      <Button type="submit" size="lg" pill block loading={loading}>
        {partial ? "Continue" : "Create church"}
      </Button>
    </div>
  );
}

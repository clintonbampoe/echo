import { useQueryClient } from "@tanstack/react-query";
import {
  PAGE_SIZES,
  usePreferences,
  type Density,
  type PageSize,
} from "@/lib/preferences";
import { PageHeader, SegmentedField, Switch } from "@/ui";
import styles from "./SettingsPage.module.css";

const densities: { value: Density; label: string }[] = [
  { value: "compact", label: "Compact" },
  { value: "comfortable", label: "Comfortable" },
  { value: "spacious", label: "Spacious" },
];

const sizes = PAGE_SIZES.map((n) => ({ value: String(n), label: String(n) }));

// Choices that stay on this device. Nothing here is sent to the server.
export function SettingsPage() {
  const [prefs, set] = usePreferences();
  const cache = useQueryClient();
  // Lists already on screen were loaded in pages of the old size, so they are asked for again.
  function choosePageSize(value: string) {
    set({ pageSize: Number(value) as PageSize });
    void cache.invalidateQueries();
  }
  return (
    <div className={`${styles.page} anim-rise`}>
      <PageHeader title="Settings" />
      <section className={styles.card} aria-labelledby="lists">
        <header className={styles.section}>
          <h2 id="lists" className={styles.heading}>
            Lists
          </h2>
          <p className={styles.sub}>Applies on every device</p>
        </header>
        <Choice
          title="Density"
          sub="How much room rows, tiles and tables take."
        >
          <SegmentedField
            label="Density"
            hideLabel
            joined
            compact
            options={densities}
            value={prefs.density}
            onChange={(density) => set({ density })}
          />
        </Choice>
        <Choice
          title="Rows per page"
          sub="How many rows a list loads before its Load more row."
        >
          <SegmentedField
            label="Rows per page"
            hideLabel
            joined
            compact
            options={sizes}
            value={String(prefs.pageSize)}
            onChange={choosePageSize}
          />
        </Choice>
      </section>
      <section className={styles.card} aria-labelledby="feel">
        <header className={styles.section}>
          <h2 id="feel" className={styles.heading}>
            Feel
          </h2>
        </header>
        <Setting
          title="Reduce motion"
          sub="Calmer animations. Follows your device setting until you change it."
          checked={prefs.reduceMotion}
          onChange={(reduceMotion) => set({ reduceMotion })}
        />
        <Setting
          title="Show currency symbol"
          sub="GH₵ before amounts"
          checked={prefs.currencySymbol}
          onChange={(currencySymbol) => set({ currencySymbol })}
        />
      </section>
    </div>
  );
}

type ChoiceProps = { title: string; sub: string; children: React.ReactNode };

// A setting with a few choices: its name and what it does on the left, the pills to choose from on the right.
function Choice({ title, sub, children }: ChoiceProps) {
  return (
    <div className={styles.setting}>
      <div className={styles.words}>
        <h3 className={styles.title}>{title}</h3>
        <p className={styles.sub}>{sub}</p>
      </div>
      {children}
    </div>
  );
}

type SettingProps = {
  title: string;
  sub: string;
  checked: boolean;
  onChange: (on: boolean) => void;
};

function Setting({ title, sub, checked, onChange }: SettingProps) {
  return (
    // A label, so pressing anywhere on the row flips the switch: nobody has to aim for the small control.
    <label className={`${styles.setting} ${styles.flip} liquid-row`}>
      <div className={styles.words}>
        <p className={styles.title}>{title}</p>
        <p className={styles.sub}>{sub}</p>
      </div>
      <Switch label={title} checked={checked} onChange={onChange} />
    </label>
  );
}

import { usePreferences, type Density } from "@/lib/preferences";
import { PageHeader, SegmentedField, Switch } from "@/ui";
import styles from "./SettingsPage.module.css";

const densities: { value: Density; label: string }[] = [
  { value: "compact", label: "Compact" },
  { value: "comfortable", label: "Comfortable" },
  { value: "spacious", label: "Spacious" },
];

// Choices that stay on this device. Nothing here is sent to the server.
export function SettingsPage() {
  const [prefs, set] = usePreferences();
  return (
    <div className={`${styles.page} anim-rise`}>
      <PageHeader title="Settings" />
      <section className={styles.card}>
        <div className={styles.density}>
          <div className={styles.head}>
            <h2 className={styles.title}>Density</h2>
            <SegmentedField
              label="Density"
              hideLabel
              joined
              compact
              options={densities}
              value={prefs.density}
              onChange={(density) => set({ density })}
            />
          </div>
          <p className={styles.sub}>How much room lists and tables take.</p>
        </div>
      </section>
      <section className={styles.card}>
        <Setting
          title="Reduce motion"
          sub="Calmer animations"
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

type SettingProps = {
  title: string;
  sub: string;
  checked: boolean;
  onChange: (on: boolean) => void;
};

function Setting({ title, sub, checked, onChange }: SettingProps) {
  return (
    // A label, so pressing anywhere on the row flips the switch: nobody has to aim for the small control.
    <label className={styles.setting}>
      <div>
        <p className={styles.title}>{title}</p>
        <p className={styles.sub}>{sub}</p>
      </div>
      <Switch label={title} checked={checked} onChange={onChange} />
    </label>
  );
}

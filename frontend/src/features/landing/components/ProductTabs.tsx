import { useState, type KeyboardEvent } from "react";
import { productTabs } from "../content";
import { ShotWindow } from "./ShotWindow";
import styles from "./ProductTabs.module.css";

// Four screens, one at a time. Arrow keys move between tabs, as a screen reader user expects.
export function ProductTabs() {
  const [active, setActive] = useState(0);

  function onKeyDown(e: KeyboardEvent, index: number) {
    const step = e.key === "ArrowRight" ? 1 : e.key === "ArrowLeft" ? -1 : 0;
    if (step === 0) return;
    e.preventDefault();
    const next = (index + step + productTabs.length) % productTabs.length;
    setActive(next);
    document.getElementById(`tab-${productTabs[next]?.id}`)?.focus();
  }

  return (
    <div>
      <div role="tablist" aria-label="Choose a feature" className={styles.list}>
        {productTabs.map((tab, i) => (
          <button
            key={tab.id}
            id={`tab-${tab.id}`}
            role="tab"
            type="button"
            aria-selected={i === active}
            aria-controls={`panel-${tab.id}`}
            tabIndex={i === active ? 0 : -1}
            className={styles.tab}
            onClick={() => setActive(i)}
            onKeyDown={(e) => onKeyDown(e, i)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {productTabs.map((tab, i) => (
        <div
          key={tab.id}
          id={`panel-${tab.id}`}
          role="tabpanel"
          aria-labelledby={`tab-${tab.id}`}
          hidden={i !== active}
          className={`${styles.panel} anim-fade`}
        >
          <div className={styles.caption}>
            <h3 className={styles.title}>{tab.title}</h3>
            <p className={styles.text}>{tab.text}</p>
          </div>
          <ShotWindow shot={tab.shot} />
        </div>
      ))}
    </div>
  );
}

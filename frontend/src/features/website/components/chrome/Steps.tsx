import {
  useLayoutEffect,
  useRef,
  useState,
  type KeyboardEvent,
  type ReactNode,
} from "react";
import { useMedia } from "@/ui";
import styles from "./Steps.module.css";

type Step = { title: string; text: string };

function reducedMotion() {
  return (
    document.documentElement.dataset.reduceMotion === "true" ||
    window.matchMedia("(prefers-reduced-motion: reduce)").matches
  );
}

const SHORT = ["Find", "Fork", "Fix", "Send", "Merge", "Shipped"];

const drawing = (children: ReactNode) => (
  <svg
    className={styles.icon}
    viewBox="0 0 48 48"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
    aria-hidden="true"
  >
    {children}
  </svg>
);

// One small drawing per step: look, copy, change, send, merge, everyone.
const ICONS: ReactNode[] = [
  drawing(
    <>
      <circle cx="21" cy="21" r="11" />
      <path d="M29.5 29.5 40 40" />
    </>,
  ),
  drawing(
    <>
      <circle cx="14" cy="10" r="4" />
      <circle cx="34" cy="10" r="4" />
      <circle cx="24" cy="38" r="4" />
      <path d="M14 14v4a6 6 0 0 0 6 6h8a6 6 0 0 0 6-6v-4M24 24v10" />
    </>,
  ),
  drawing(<path d="M18 14 8 24l10 10M30 14l10 10-10 10" />),
  drawing(<path d="M42 6 21 27M42 6 28 42l-7-15-15-7z" />),
  drawing(
    <>
      <circle cx="12" cy="10" r="4" />
      <circle cx="12" cy="38" r="4" />
      <circle cx="36" cy="26" r="4" />
      <path d="M12 14v20M36 22c0-10-24-6-24-8" />
    </>,
  ),
  drawing(
    <>
      <circle cx="24" cy="24" r="19" opacity=".35" />
      <circle cx="24" cy="24" r="11" opacity=".7" />
      <circle className={styles.rip} cx="24" cy="24" r="4" />
      <circle cx="24" cy="24" r="4" fill="currentColor" />
    </>,
  ),
];

// From an idea to every church, as steps that can only be taken one at a time. A wide screen scrolls
// through a window (a mouse wheel or a trackpad), a narrow one swipes sideways. Each step is a snap stop
// that cannot be skipped, so a hard flick still lands on the next step and no further, and the arrow
// keys do the same. Only the window takes the scroll; everything around it listens. How far through the
// steps you are (`p`, 0 to the last step, moving smoothly) is published as --p on the steps and as
// --steps-p on the section, so the line above fills with it, each step fades and drifts as it comes and
// goes, and the page's own headline can follow along (CommunityPage).
export function Steps({ items }: { items: readonly Step[] }) {
  const wide = useMedia("(min-width: 1000px)");
  const scroller = useRef<HTMLDivElement>(null);
  const box = useRef<HTMLDivElement>(null);
  const [at, setAt] = useState(0);
  const last = items.length - 1;

  const size = (el: HTMLElement) => (wide ? el.clientHeight : el.clientWidth);
  const position = (el: HTMLElement) => (wide ? el.scrollTop : el.scrollLeft);

  // Hands the position to CSS: --p on the steps, --steps-p on the section, and how far each step is from the
  // one in view (--d, signed) and how far (--a, 0 to 1).
  function publish(p: number) {
    box.current?.style.setProperty("--p", p.toFixed(3));
    box.current
      ?.closest("section")
      ?.style.setProperty("--steps-p", p.toFixed(3));
    Array.from(scroller.current?.children ?? []).forEach((step, i) => {
      const d = p - i;
      const style = (step as HTMLElement).style;
      style.setProperty("--d", d.toFixed(3));
      style.setProperty("--a", Math.min(1, Math.abs(d)).toFixed(3));
    });
  }
  // The first look, before anything is painted, and tidying up the section when the steps go.
  useLayoutEffect(() => {
    publish(0);
    const section = box.current?.closest("section");
    return () => {
      section?.style.removeProperty("--steps-p");
    };
  }, []);

  function onScroll() {
    const el = scroller.current;
    if (!el || size(el) === 0) return;
    const p = Math.max(0, Math.min(last, position(el) / size(el)));
    publish(p);
    setAt(Math.round(p));
  }

  function go(to: number) {
    const el = scroller.current;
    if (!el) return;
    el.scrollTo({
      [wide ? "top" : "left"]: to * size(el),
      behavior: reducedMotion() ? "auto" : "smooth",
    });
  }

  function onKeyDown(e: KeyboardEvent) {
    const forward = ["ArrowDown", "ArrowRight", "PageDown"].includes(e.key);
    const back = ["ArrowUp", "ArrowLeft", "PageUp"].includes(e.key);
    // At either end the key goes on to scroll the page, so the steps never trap anyone.
    if (forward && at < last) go(at + 1);
    else if (back && at > 0) go(at - 1);
    else return;
    e.preventDefault();
  }

  return (
    <div ref={box} className={styles.steps}>
      <ol className={styles.track} aria-hidden="true">
        {SHORT.map((name, i) => (
          <li
            key={name}
            className={styles.node}
            data-reached={i <= at || undefined}
            data-current={i === at || undefined}
          >
            {name}
          </li>
        ))}
      </ol>
      <div
        ref={scroller}
        className={styles.window}
        role="group"
        aria-label="How a change reaches every church. Scroll or swipe, one step at a time."
        tabIndex={0}
        onScroll={onScroll}
        onKeyDown={onKeyDown}
      >
        {items.map((step, i) => (
          <div
            key={step.title}
            className={styles.step}
            data-current={i === at || undefined}
          >
            <div>
              <span className={styles.count}>
                Step {i + 1} of {items.length}
              </span>
              <h3 className={styles.title}>{step.title}</h3>
              <p className={styles.text}>{step.text}</p>
            </div>
            {ICONS[i]}
          </div>
        ))}
      </div>
      <p className={styles.hint} data-gone={at > 0 || undefined}>
        Scroll or swipe. One step at a time.
      </p>
    </div>
  );
}

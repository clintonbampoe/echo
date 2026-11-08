import type { CSSProperties } from "react";
import { ANSWERS, type Mark } from "../../content/home";
import { useCycle } from "./useCycle";
import styles from "./AnswerStack.module.css";

// Each card ends with a small picture of its figure.
function Picture({ mark }: { mark: Mark }) {
  switch (mark.kind) {
    case "line":
      return (
        <svg
          className={styles.line}
          viewBox="0 0 110 40"
          fill="none"
          aria-hidden="true"
        >
          <polyline
            points="0,32 18,27 36,30 54,20 72,24 90,13 106,8"
            stroke="var(--text-2)"
            strokeWidth="1.6"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <circle cx="106" cy="8" r="3.4" fill="var(--text)" />
        </svg>
      );
    case "bar":
      return (
        <span className={styles.bar} aria-hidden="true">
          <span style={{ width: `${mark.percent}%` }} />
        </span>
      );
    case "people":
      return (
        <span className={styles.people} aria-hidden="true">
          {[0.95, 0.83, 0.71, 0.59, 0.47, 0.35].map((opacity) => (
            <span key={opacity} style={{ opacity }} />
          ))}
        </span>
      );
    case "date":
      return (
        <span className={styles.date} aria-hidden="true">
          <span>{mark.month}</span>
          <strong>{mark.day}</strong>
        </span>
      );
  }
}

const FADE = [1, 0.96, 0.85, 0.7, 0.45];

// Five example answers as a stack of cards. The front card gives way to the next every few seconds; a tap
// on the front card sends it to the back, a tap on one behind brings it forward. Resting a pointer on the
// stack holds it still.
export function AnswerStack() {
  const { index, choose, hold } = useCycle(ANSWERS.length, 4200);
  return (
    <div
      role="group"
      aria-label="Example answers"
      className={styles.stack}
      onMouseEnter={() => hold(true)}
      onMouseLeave={() => hold(false)}
    >
      {ANSWERS.map((answer, i) => {
        const place = (i - index + ANSWERS.length) % ANSWERS.length;
        return (
          <button
            key={answer.q}
            type="button"
            className={styles.card}
            aria-label={`Example: ${answer.q}`}
            style={
              {
                "--place": place,
                "--dim": FADE[place],
                zIndex: ANSWERS.length - place,
              } as CSSProperties
            }
            onClick={() =>
              choose(place === 0 ? (index + 1) % ANSWERS.length : i, 9000)
            }
          >
            <span className={styles.question}>{answer.q}</span>
            <span className={styles.foot}>
              <span>
                <span className={styles.figure}>{answer.a}</span>
                <span className={styles.note}>{answer.note}</span>
              </span>
              <span className={styles.picture}>
                <Picture mark={answer.mark} />
              </span>
            </span>
          </button>
        );
      })}
    </div>
  );
}

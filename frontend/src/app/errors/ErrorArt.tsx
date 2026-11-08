import styles from "./ErrorArt.module.css";

const WALK =
  "M60 140 C110 60,170 40,225 75 S320 160,380 120 S470 60,440 160 S330 215,260 200";
const BEAT_ONE =
  "M20 85 H100 L116 50 L138 118 L154 85 H234 L250 45 L272 125 L288 85";
const BEAT_ALL =
  "M20 85 H100 L116 50 L138 118 L154 85 H234 L250 45 L272 125 L288 85 H368 L384 50 L406 118 L422 85 H500";

// A page that is not here: a dot wanders off, a call goes out, and the dot finds its way back.
export function WanderArt() {
  return (
    <svg
      viewBox="0 0 520 250"
      className={styles.art}
      role="img"
      aria-label="A dot wanders off. A call goes out and the dot finds its way back."
    >
      <defs>
        <mask id="wander-mask">
          <path d={WALK} className={styles.maskPath} pathLength="100" />
        </mask>
      </defs>
      <g className={styles.frame}>
        <path d={WALK} className={styles.trail} mask="url(#wander-mask)" />
        <circle cx="260" cy="200" r="6" className={styles.ringA} />
        <circle cx="260" cy="200" r="6" className={styles.ringB} />
        <text x="260" y="172" className={styles.hello}>
          hello?
        </text>
        <circle cx="260" cy="200" r="7" className={styles.halo} />
        <circle
          r="8"
          className={styles.dot}
          style={{ offsetPath: `path('${WALK}')` }}
        />
        <text x="260" y="246" className={styles.wait}>
          tap anywhere to call it back
        </text>
        <text x="260" y="246" className={styles.found}>
          there you are
        </text>
      </g>
    </svg>
  );
}

// Something broke on our side: a heartbeat goes flat, a dot checks for a pulse, and the beat returns.
export function PulseArt() {
  return (
    <svg
      viewBox="0 0 520 170"
      className={styles.art}
      role="img"
      aria-label="A heartbeat line goes flat, a dot checks for a pulse, and the heartbeat returns."
    >
      <g className={styles.frame}>
        <path d="M20 85 H500" className={styles.baseline} />
        <path d={BEAT_ONE} className={styles.beatA} pathLength="100" />
        <path d={BEAT_ALL} className={styles.beatB} pathLength="100" />
        <circle cx="390" cy="85" r="5" className={styles.tapA} />
        <circle cx="340" cy="85" r="5" className={styles.tapB} />
        <circle cx="300" cy="85" r="5" className={styles.tapC} />
        <circle r="6" className={styles.checker} />
        <text x="260" y="150" className={`${styles.wait} ${styles.waitPulse}`}>
          checking for a pulse…
        </text>
        <text
          x="260"
          y="150"
          className={`${styles.found} ${styles.foundPulse}`}
        >
          and we are back
        </text>
      </g>
    </svg>
  );
}

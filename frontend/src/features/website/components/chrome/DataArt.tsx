import type { CSSProperties } from "react";
import styles from "./DataArt.module.css";

const PATH = "M344.2 81.1 A70 70 0 1 1 344.2 128.9";

// Words that try to get in, and bounce off: where they appear, where they start from and where they bounce to.
const KEPT_OUT = [
  {
    word: "ads",
    x: 445.0,
    y: 44.4,
    from: [55, -95],
    to: [37, -83],
    turn: -35,
    width: 35.6,
    delay: 3.4,
  },
  {
    word: "trackers",
    x: 459.5,
    y: 154.5,
    from: [78, 78],
    to: [23, -5],
    turn: 35,
    width: 71.6,
    delay: 4.3,
  },
  {
    word: "resale",
    x: 473.4,
    y: 75.4,
    from: [100, -46],
    to: [57, -61],
    turn: -30,
    width: 57.2,
    delay: 5.2,
  },
] as const;

// The records that travel to the server, one after another.
const KEPT = [
  { word: "Attendance", width: 78, delay: 0.3 },
  { word: "Giving", width: 58, delay: 1.1 },
  { word: "Members", width: 66, delay: 1.9 },
] as const;

const CAPTIONS = [
  "Your data goes to your server.",
  "Everyone else stays outside.",
  "It only opens for you.",
] as const;

// A small story, told in nine seconds on a loop: records go from the phone to the server, and ads,
// trackers and resale try to follow and bounce off. Reduced motion shows the first line and no movement.
// A record on its way to the server.
function Kept({ item }: { item: (typeof KEPT)[number] }) {
  return (
    <g className={styles.pass} style={{ animationDelay: `${item.delay}s` }}>
      <rect
        x="76"
        y="96"
        width={item.width}
        height="19"
        rx="9.5"
        className={styles.pill}
      />
      <text x={76 + item.width / 2} y="109" className={styles.pillText}>
        {item.word}
      </text>
    </g>
  );
}

// A word that tries to get in: it flies toward the server, the shield flashes, and it bounces away.
function Out({ item }: { item: (typeof KEPT_OUT)[number] }) {
  const delay = { animationDelay: `${item.delay}s` };
  const flight = {
    ...delay,
    "--sx": `${item.from[0]}px`,
    "--sy": `${item.from[1]}px`,
    "--bx": `${item.to[0]}px`,
    "--by": `${item.to[1]}px`,
    "--rot": `${item.turn}deg`,
  } as CSSProperties;
  return (
    <g>
      <g transform={`translate(${item.x} ${item.y})`}>
        <g className={styles.bounce} style={flight}>
          <rect
            x={-item.width / 2}
            y="-10"
            width={item.width}
            height="20"
            rx="10"
            className={styles.tag}
          />
          <text x="0" y="4" className={styles.tagText}>
            {item.word}
          </text>
        </g>
      </g>
      <circle
        cx={item.x}
        cy={item.y}
        r="5"
        className={styles.ring}
        style={delay}
      />
      <path d={PATH} className={styles.flash} style={delay} />
    </g>
  );
}

export function DataArt() {
  return (
    <div className={styles.art}>
      <svg
        viewBox="0 0 520 210"
        className={styles.svg}
        role="img"
        aria-label="Your data moves from the phone to your server. Ads, trackers and resale try to get in and bounce off."
      >
        <g className={styles.frame}>
          <rect
            x="28"
            y="72"
            width="34"
            height="66"
            rx="9"
            className={styles.card}
          />
          <rect
            x="34"
            y="80"
            width="22"
            height="46"
            rx="3"
            className={styles.screen}
          />
          <path d="M62 105 H340" className={styles.guide} />
          <g className={styles.card}>
            <rect x="380" y="70" width="60" height="20" rx="5" />
            <rect x="380" y="95" width="60" height="20" rx="5" />
            <rect x="380" y="120" width="60" height="20" rx="5" />
          </g>
          <g className={styles.led}>
            <circle cx="428" cy="80" r="2.5" />
            <circle cx="428" cy="105" r="2.5" />
            <circle cx="428" cy="130" r="2.5" />
          </g>
          <path d={PATH} className={styles.shield} />
          {KEPT.map((k) => (
            <Kept key={k.word} item={k} />
          ))}
          {KEPT_OUT.map((o) => (
            <Out key={o.word} item={o} />
          ))}
        </g>
      </svg>
      <div className={styles.captions} aria-hidden="true">
        {CAPTIONS.map((line, i) => (
          <span key={line} className={styles[`line${i + 1}` as "line1"]}>
            {line}
          </span>
        ))}
      </div>
    </div>
  );
}

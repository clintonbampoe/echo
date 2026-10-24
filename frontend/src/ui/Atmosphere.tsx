import { useState, type CSSProperties } from "react";
import styles from "./Atmosphere.module.css";

// [left %, top %, size px, float seconds, float delay, fade seconds, fade delay, dx, dy]
const SPHERES = [
  [-9, 9, 380, 26, 0, 22, 0, -28, -34],
  [93, 19, 300, 33, -9, 27, -4, 0, -40],
  [-12, 47, 280, 40, -18, 32, -8, 28, -36],
  [94, 66, 360, 47, -27, 37, -12, -28, -42],
  [-5, 91, 300, 54, -36, 42, -16, 0, -34],
] as const;

// [left %, top %, size px, drift seconds, drift delay, fade seconds, fade delay, dx, dy]
const MOTES = [
  [18, 6, 12, 34, 0, 14, 0, -36, -26],
  [80, 9, 16, 39, -9, 16, -3, 0, -36],
  [93, 23, 9, 44, -18, 18, -6, 36, -46],
  [8, 31, 11, 49, -27, 20, -9, -36, -56],
  [66, 38, 20, 54, -36, 22, -12, 0, -26],
  [30, 45, 10, 59, -45, 24, -15, 36, -36],
  [88, 52, 13, 64, -54, 26, -18, -36, -46],
  [14, 62, 15, 69, -63, 28, -21, 0, -56],
  [72, 70, 9, 74, -72, 30, -24, 36, -26],
  [40, 77, 18, 79, -81, 32, -27, -36, -36],
  [92, 84, 11, 84, -90, 34, -30, 0, -46],
  [22, 92, 13, 89, -99, 36, -33, 36, -56],
] as const;

type Row = readonly (readonly number[])[];

function place(
  [left, top, size, a, aDelay, b, bDelay, dx, dy]: readonly number[],
  move: string,
  fade: string,
): CSSProperties {
  return {
    left: `${left}%`,
    top: `${top}%`,
    "--size": `${size}px`,
    "--dx": `${dx}px`,
    "--dy": `${dy}px`,
    animation: `${move} ${a}s ease-in-out ${aDelay}s infinite, ${fade} ${b}s ease-in-out ${bDelay}s infinite`,
  } as CSSProperties;
}

// The still, slow light behind the website and the auth screens: a very large faint orb
// that drifts round the edges, a few glass spheres and some small glowing motes. Colours come
// from the surface (light or dark); with reduced motion everything just sits.
export function Atmosphere() {
  // Starts somewhere different on each load; a lap takes seven minutes.
  const [start] = useState(() => -Math.floor(Math.random() * 420));
  return (
    <div className={styles.sky} aria-hidden="true">
      <span className={styles.wash} />
      <span className={styles.mother} style={{ animationDelay: `${start}s` }} />
      {(SPHERES as Row).map((s, i) => (
        <span
          key={`s${i}`}
          className={styles.sphere}
          style={place(s, "echoFloat", "echoFade2")}
        />
      ))}
      {(MOTES as Row).map((m, i) => (
        <span
          key={`m${i}`}
          className={styles.mote}
          style={place(m, "echoDrift", "echoFade")}
        />
      ))}
    </div>
  );
}

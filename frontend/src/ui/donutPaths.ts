// The outlines of a donut's slices. Every gap between neighbours is the same width all the way across (the two edges
// run parallel to each other, not towards the centre), and the corners are rounded by the stroke drawn round the outline.
// Pure, so the tests can check it. No React in this file.
export type SliceShape = { key: string; path: string };

const SIZE = 140;
const MID = SIZE / 2;
const OUTER = 64;
const INNER = 32;
const HALF_GAP = 3.5;

const round = (n: number) => Math.round(n * 100) / 100;

// A point on the line that runs parallel to the edge at `angle`, pushed `HALF_GAP` towards the slice (`side` is 1 on a
// slice's first edge and -1 on its last), at distance `radius` from the middle.
function edgePoint(angle: number, radius: number, side: 1 | -1) {
  const along = Math.sqrt(radius * radius - HALF_GAP * HALF_GAP);
  const ux = Math.sin(angle);
  const uy = -Math.cos(angle);
  const nx = Math.cos(angle) * side;
  const ny = Math.sin(angle) * side;
  return {
    x: round(MID + HALF_GAP * nx + along * ux),
    y: round(MID + HALF_GAP * ny + along * uy),
  };
}

export function donutPaths(
  values: { key: string; value: number }[],
): SliceShape[] {
  const total = values.reduce((sum, v) => sum + Math.max(0, v.value), 0);
  if (total <= 0) return [];
  let from = 0;
  return values
    .filter((v) => v.value > 0)
    .map((v) => {
      const sweep = (v.value / total) * Math.PI * 2;
      const to = from + sweep;
      const a = edgePoint(from, OUTER, 1);
      const b = edgePoint(to, OUTER, -1);
      const c = edgePoint(to, INNER, -1);
      const d = edgePoint(from, INNER, 1);
      const large = sweep > Math.PI ? 1 : 0;
      const path = `M${a.x} ${a.y}A${OUTER} ${OUTER} 0 ${large} 1 ${b.x} ${b.y}L${c.x} ${c.y}A${INNER} ${INNER} 0 ${large} 0 ${d.x} ${d.y}Z`;
      from = to;
      return { key: v.key, path };
    });
}

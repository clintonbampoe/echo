// A row that opens or closes is felt by the rows around it: the nearest two lean away a little and dim,
// then settle back. Nothing stays changed, and with reduced motion nothing moves.
const LEAN = [5, 2];
const DIM = [0.7, 0.86];

function feel(row: Element | null, away: 1 | -1, step: number) {
  if (!(row instanceof HTMLElement) || !row.animate) return;
  const lean = (LEAN[step] ?? 0) * away;
  row.animate(
    [
      { transform: "translateY(0)", opacity: 1 },
      {
        transform: `translateY(${lean}px)`,
        opacity: DIM[step] ?? 1,
        offset: 0.3,
      },
      { transform: "translateY(0)", opacity: 1 },
    ],
    { duration: 820, easing: "cubic-bezier(0.3, 0, 0.2, 1)" },
  );
}

export function ripple(row: HTMLElement) {
  const still =
    typeof matchMedia !== "function" ||
    matchMedia("(prefers-reduced-motion: reduce)").matches ||
    document.documentElement.dataset.reduceMotion === "true";
  if (still) return;
  feel(row.previousElementSibling, -1, 0);
  feel(row.previousElementSibling?.previousElementSibling ?? null, -1, 1);
  feel(row.nextElementSibling, 1, 0);
  feel(row.nextElementSibling?.nextElementSibling ?? null, 1, 1);
}

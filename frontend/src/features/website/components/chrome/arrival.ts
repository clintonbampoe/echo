// How a section's content arrives the first time the section comes into focus (useSectionFocus): the
// pieces of it rise and clear one after another, in reading order.

// The things that arrive as one piece. Where one sits inside another, the outer one arrives and carries
// the rest with it, so a paragraph in a card does not move on its own.
const PIECES =
  "h1, h2, h3, h4, p, ul, ol, figure, form, details, table, svg, img, a, button, [role='group']";
// Pieces within this many pixels of each other, top to bottom, are in one row.
const ROW = 56;
const ROW_DELAY = 90;
const COLUMN_DELAY = 60;
const LONGEST_DELAY = 520;
const DURATION = 850;
const EASE = "cubic-bezier(0.05, 0.7, 0.1, 1)";

// A box that stands out from the page (a card: it has a shadow or a fill of its own) is a piece too, whatever
// it holds. A box that is only lines (a list's rules) is not: its rows arrive one by one inside it.
function isCard(el: HTMLElement) {
  const { boxShadow, backgroundColor } = getComputedStyle(el);
  return (
    boxShadow !== "none" ||
    !/^(rgba\(0, 0, 0, 0\)|transparent)$/.test(backgroundColor)
  );
}

export function piecesOf(section: HTMLElement) {
  const found = new Set(section.querySelectorAll<HTMLElement>(PIECES));
  for (const box of section.querySelectorAll<HTMLElement>("div")) {
    if (isCard(box)) found.add(box);
  }
  const inside = (el: HTMLElement) => {
    for (
      let up = el.parentElement;
      up && up !== section;
      up = up.parentElement
    ) {
      if (found.has(up)) return true;
    }
    return false;
  };
  return [...found].filter((el) => !inside(el));
}

// When each piece starts, given where it sits: down the page row by row, and along a row left to right.
// A tall piece (a card beside the headline) is placed by its middle, so it does not jump ahead of the text.
export function delaysOf(
  boxes: { top: number; left: number; height?: number }[],
) {
  const order = boxes
    .map((box, i) => ({
      i,
      row: Math.round((box.top + Math.min(box.height ?? 0, 240) / 2) / ROW),
      left: box.left,
    }))
    .sort((a, b) => a.row - b.row || a.left - b.left);
  const delays: number[] = new Array(boxes.length).fill(0);
  let row = -1;
  let column = 0;
  let last = Number.NaN;
  for (const piece of order) {
    if (piece.row !== last) {
      row += 1;
      column = 0;
      last = piece.row;
    }
    delays[piece.i] = Math.min(
      LONGEST_DELAY,
      row * ROW_DELAY + column * COLUMN_DELAY,
    );
    column += 1;
  }
  return delays;
}

// Rises 22px and clears. The piece's own styles are untouched (the web animation sits on top of them),
// and before its turn it stays hidden, so nothing flashes.
export function arrive(pieces: HTMLElement[], delays: number[]) {
  pieces.forEach((el, i) => {
    el.animate(
      [
        { opacity: 0, translate: "0 22px" },
        { opacity: 1, translate: "0 0" },
      ],
      { duration: DURATION, delay: delays[i], easing: EASE, fill: "backwards" },
    );
  });
}

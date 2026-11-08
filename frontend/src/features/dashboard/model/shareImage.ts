import { formatNumber } from "@/lib/format";
import { signed, type Row, type WeekFigures } from "./weekly";

const W = 1080;
const PAD = 84;
const INK = "#0A0A0A";
const SOFT = "#686870";
const LINE = "#E7E7E4";
const FONT = "Geist, system-ui, -apple-system, sans-serif";

type Ctx = CanvasRenderingContext2D;

function text(
  c: Ctx,
  s: string,
  x: number,
  y: number,
  size: number,
  weight: number,
  color: string,
  align: CanvasTextAlign = "left",
) {
  c.font = `${weight} ${size}px ${FONT}`;
  c.fillStyle = color;
  c.textAlign = align;
  c.fillText(s, x, y);
}

// A group: its title and total, then a row for each line.
function group(c: Ctx, title: string, total: number, rows: Row[], y: number) {
  text(c, title, PAD, y, 40, 600, INK);
  text(c, formatNumber(total), W - PAD, y, 40, 600, INK, "right");
  let at = y + 22;
  for (const r of rows) {
    c.fillStyle = LINE;
    c.fillRect(PAD, at, W - PAD * 2, 2);
    const label = r.note ? `${r.name}  ·  ${r.note}` : r.name;
    text(c, label, PAD, at + 62, 34, 400, "#52525B");
    text(c, formatNumber(r.amount), W - PAD, at + 62, 34, 500, INK, "right");
    at += 88;
  }
  return at;
}

// A clean picture of the week's money, drawn on a canvas so it can be sent through the phone's share sheet.
export async function weekImage(
  label: string,
  range: string,
  f: WeekFigures,
): Promise<Blob | null> {
  const rows = f.cameRows.length + f.spentRows.length;
  const height = 934 + rows * 88;
  const canvas = document.createElement("canvas");
  canvas.width = W;
  canvas.height = height;
  const c = canvas.getContext("2d");
  if (!c) return null;
  await document.fonts?.ready;

  c.fillStyle = "#FFFFFF";
  c.fillRect(0, 0, W, height);
  text(c, label, PAD, 150, 44, 600, INK);
  text(c, range, PAD, 205, 32, 400, SOFT);

  c.fillStyle = "#F5F5F3";
  c.beginPath();
  c.roundRect(PAD - 24, 270, W - PAD * 2 + 48, 270, 48);
  c.fill();
  text(c, "Net", PAD + 16, 350, 34, 400, SOFT);
  text(c, "GH₵", PAD + 16, 470, 44, 500, SOFT);
  c.font = `500 44px ${FONT}`;
  const unit = c.measureText("GH₵").width;
  text(c, signed(f.net), PAD + 16 + unit + 20, 470, 130, 600, INK);

  let y = group(c, "Came in", f.came, f.cameRows, 650);
  y = group(c, "Went out", f.spent, f.spentRows, y + 90);
  text(c, "Echo", W - PAD, y + 90, 30, 500, SOFT, "right");

  return new Promise((done) => canvas.toBlob(done, "image/png"));
}

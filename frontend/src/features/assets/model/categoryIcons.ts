// A mark for each kind of thing a church keeps, drawn on a 20 x 20 grid. Categories are the congregation's own words, so
// the mark is found by what the name mentions; a name that mentions nothing known gets the box. No React in this file.
export type CategoryMark = { key: string; path: string };

const marks: (CategoryMark & { words: RegExp })[] = [
  {
    key: "furniture",
    words: /furniture|chair|table|desk|pew/i,
    path: "M5 9V4h10v5M4 9h12v4H4zM6 13v3M14 13v3",
  },
  {
    key: "vehicle",
    words: /vehicle|car|bus|van|truck/i,
    path: "M3 12l1.5-5h11L17 12M3 12h14v3H3zM6 15v1.5M14 15v1.5",
  },
  {
    key: "property",
    words: /property|building|land|house|plot/i,
    path: "M3 8l7-5 7 5v9H3zM8 17v-5h4v5",
  },
  {
    key: "audio",
    words: /audio|video|sound|speaker|media/i,
    path: "M4 8v4h3l4 3V5L7 8zM14 7a4 4 0 010 6",
  },
  {
    key: "instrument",
    words: /instrument|music|piano|drum|guitar/i,
    path: "M8 15V5l7-1.5v10M8 15a2 2 0 11-4 0 2 2 0 014 0zM15 13.5a2 2 0 11-4 0 2 2 0 014 0z",
  },
  {
    key: "electronics",
    words: /electronic|computer|laptop|screen|phone/i,
    path: "M3 5h14v9H3zM7 17h6",
  },
  {
    key: "kitchen",
    words: /kitchen|cook|catering|crockery/i,
    path: "M6 3v6a2 2 0 004 0V3M8 9v8M14 3c-1.5 1-2 3-2 5h2v9",
  },
  {
    key: "books",
    words: /book|library|bible|hymn/i,
    path: "M4 4h5a2 2 0 012 2v10a2 2 0 00-2-2H4zM16 4h-5",
  },
  {
    key: "garden",
    words: /garden|plant|farm|tree/i,
    path: "M10 17v-7M10 10c-3 0-5-2-5-5 3 0 5 2 5 5zM10 12c2 0 4-1.5 4-4-2.5 0-4 1.5-4 4z",
  },
  {
    key: "safety",
    words: /safety|security|fire|first aid/i,
    path: "M10 3l6 2v5c0 4-3 6-6 7-3-1-6-3-6-7V5z",
  },
  {
    key: "sports",
    words: /sport|game|football|fitness/i,
    path: "M10 3a7 7 0 100 14 7 7 0 000-14zM3.5 10h13M10 3c2 2 3 4.5 3 7s-1 5-3 7M10 3c-2 2-3 4.5-3 7s1 5 3 7",
  },
  {
    key: "stationery",
    words: /stationery|office|supplies|paper/i,
    path: "M4 16l1-4 8-8 3 3-8 8zM11 6l3 3",
  },
];

// Equipment, and anything the names above do not mention.
const box: CategoryMark = {
  key: "equipment",
  path: "M4 4h12v12H4zM8 8h4v4H8z",
};

export function categoryMark(name: string): CategoryMark {
  return marks.find((m) => m.words.test(name)) ?? box;
}

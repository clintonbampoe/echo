// The verse shown beside the auth forms and at the foot of the landing page.
//
// SKELETON: today there is one verse. When rotation is built, replace the body of getVerse
// with "pick by day of year from a curated list". Callers and the Quote component need no change.

export type Verse = {
  text: string;
  reference: string;
};

const VERSES: readonly Verse[] = [
  {
    text: "For where two or more are gathered in my name, there am I among them.",
    reference: "Matthew 18:20",
  },
];

export function getVerse(_date: Date = new Date()): Verse {
  // First entry until rotation exists. The date parameter is already here so callers will not change.
  return VERSES[0] as Verse;
}

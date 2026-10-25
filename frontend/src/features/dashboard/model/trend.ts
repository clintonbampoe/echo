// One bar of a trend chart: the days it counts, its short name under the bar, its name for a screen reader and
// its tooltip heading.
export type Bucket = {
  from: string;
  to: string;
  label: string;
  name: string;
  tip: string;
};

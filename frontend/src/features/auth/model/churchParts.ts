import type { ChurchValues } from "../forms/schema";

// The church form in three small groups. A phone shows one group at a time; a larger screen shows all of them.
export const CHURCH_PARTS: (keyof ChurchValues)[][] = [
  ["name", "orgType"],
  ["phoneNumber", "churchEmail"],
  ["region", "city", "town", "gpsAddress"],
];

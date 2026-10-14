// Fixed lists for dropdowns (design rule S2). Values are what the API expects.
import type { Option } from "@/ui";

export const congregationTypes: readonly Option[] = [
  { value: "Church", label: "Church" },
  { value: "Mosque", label: "Mosque" },
  { value: "NonProfit", label: "Non-profit" },
  { value: "Other", label: "Other" },
];

export const regions: readonly Option[] = [
  { value: "GreaterAccra", label: "Greater Accra" },
  { value: "Ashanti", label: "Ashanti" },
  { value: "Central", label: "Central" },
  { value: "Eastern", label: "Eastern" },
  { value: "Western", label: "Western" },
  { value: "WesternNorth", label: "Western North" },
  { value: "Volta", label: "Volta" },
  { value: "Oti", label: "Oti" },
  { value: "Northern", label: "Northern" },
  { value: "Savannah", label: "Savannah" },
  { value: "NorthEast", label: "North East" },
  { value: "UpperEast", label: "Upper East" },
  { value: "UpperWest", label: "Upper West" },
  { value: "Bono", label: "Bono" },
  { value: "BonoEast", label: "Bono East" },
  { value: "Ahafo", label: "Ahafo" },
];

// "GreaterAccra" -> "Greater Accra", for showing a region the API sent back.
export function regionLabel(value: string): string {
  return regions.find((r) => r.value === value)?.label ?? value;
}

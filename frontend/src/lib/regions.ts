// The sixteen regions of Ghana. Values are what the API expects. Used by the church form,
// the invitation card and the members list.
export type Region = { value: string; label: string };

export const regions: readonly Region[] = [
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

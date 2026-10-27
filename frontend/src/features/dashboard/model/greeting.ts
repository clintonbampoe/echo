// "Good morning" until noon, "Good afternoon" until 6pm, "Good evening" after.
export function greetingFor(hour: number): string {
  if (hour < 12) return "Good morning";
  if (hour < 18) return "Good afternoon";
  return "Good evening";
}

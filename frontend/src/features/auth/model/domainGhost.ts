// The usual mail providers, offered while someone types an address. Finishing the address for
// them prevents the typo instead of scolding for it afterwards.
const DOMAINS = [
  "gmail.com",
  "yahoo.com",
  "outlook.com",
  "icloud.com",
  "hotmail.com",
];

// What to add after `email` to complete a known domain, or "" when nothing fits.
export function domainRest(email: string): string {
  const at = email.indexOf("@");
  if (at < 1 || email.indexOf("@", at + 1) !== -1) return "";
  const typed = email.slice(at + 1).toLowerCase();
  if (!typed) return "";
  const match = DOMAINS.find((d) => d.startsWith(typed) && d !== typed);
  return match ? match.slice(typed.length) : "";
}

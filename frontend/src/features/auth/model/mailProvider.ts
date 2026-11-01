export type MailProvider = { name: string; url: string };

const gmail = { name: "Gmail", url: "https://mail.google.com" };
const outlook = { name: "Outlook", url: "https://outlook.live.com/mail" };
const yahoo = { name: "Yahoo Mail", url: "https://mail.yahoo.com" };
const icloud = { name: "iCloud Mail", url: "https://www.icloud.com/mail" };
const proton = { name: "Proton Mail", url: "https://mail.proton.me" };

// Where to send someone to read their mail, by the part of the address after the @. An address on a
// church's own domain is not here: we cannot tell where that mail is read, so we offer no shortcut.
const byDomain: Record<string, MailProvider> = {
  "gmail.com": gmail,
  "googlemail.com": gmail,
  "outlook.com": outlook,
  "hotmail.com": outlook,
  "live.com": outlook,
  "msn.com": outlook,
  "yahoo.com": yahoo,
  "icloud.com": icloud,
  "me.com": icloud,
  "proton.me": proton,
  "protonmail.com": proton,
  "pm.me": proton,
};

export function mailProviderFor(email: string): MailProvider | null {
  const domain = email.split("@").pop()?.trim().toLowerCase() ?? "";
  return byDomain[domain] ?? null;
}

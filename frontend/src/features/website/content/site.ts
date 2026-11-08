// Words and links shared by every page of the website. Placeholders are marked.
export const GITHUB_URL = "https://github.com/clintonbampoe/echo";
// Placeholder address until the real one is set up.
export const CONTACT_EMAIL = "hello@echo.app";

// Every page the header offers, in order. Get started follows them.
export const NAV = [
  { to: "/about", label: "About" },
  { to: "/community", label: "Community" },
  { to: "/pricing", label: "Pricing" },
  { to: "/security", label: "Security" },
  { to: "/terms", label: "Terms" },
  { to: "/contact?topic=support", label: "Support Echo" },
] as const;

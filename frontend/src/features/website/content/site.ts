// Words and links shared by every page of the website. Placeholders are marked.
export const GITHUB_URL = "https://github.com/clintonbampoe/echo";
// Placeholder address until the real one is set up.
export const CONTACT_EMAIL = "hello@echo.app";

// The header's three pages. Sign in and Get started follow them.
export const NAV = [
  { to: "/about", label: "About" },
  { to: "/community", label: "Community" },
  { to: "/pricing", label: "Pricing" },
] as const;

type FooterLink = { label: string; to: string };

// Three links in the footer; a narrower screen keeps the first two.
export const FOOTER: FooterLink[] = [
  { label: "Security", to: "/security" },
  { label: "Terms", to: "/terms" },
  { label: "Support Echo", to: "/contact?topic=support" },
];

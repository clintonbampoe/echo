// Words and links shared by every page of the website. Placeholders are marked.
export const GITHUB_URL = "https://github.com/clintonbampoe/echo";
// Placeholder address until the real one is set up.
export const CONTACT_EMAIL = "hello@echo.app";

export const NAV = [
  { to: "/features", label: "Features" },
  { to: "/docs", label: "Docs" },
  { to: "/roadmap", label: "Roadmap" },
  { to: "/community", label: "Community" },
] as const;

type FooterLink = { label: string; to: string; external?: boolean };

// The footer shows three links; everything else lives behind "More", so no page is orphaned.
export const FOOTER_MAIN: FooterLink[] = [
  { label: "Security", to: "/security" },
  { label: "Terms", to: "/terms" },
  { label: "Support Echo", to: "/community#support" },
];
export const FOOTER_MORE: FooterLink[] = [
  { label: "About", to: "/about" },
  { label: "Contact", to: "/contact" },
  { label: "Changelog", to: "/changelog" },
  { label: "Privacy", to: "/privacy" },
  { label: "GitHub", to: GITHUB_URL, external: true },
];

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

export const FOOTER: { title: string; links: FooterLink[] }[] = [
  {
    title: "Product",
    links: [
      { label: "Features", to: "/features" },
      { label: "Docs", to: "/docs" },
      { label: "Roadmap", to: "/roadmap" },
      { label: "Changelog", to: "/changelog" },
    ],
  },
  {
    title: "Community",
    links: [
      { label: "GitHub", to: GITHUB_URL, external: true },
      { label: "Contribute", to: `${GITHUB_URL}/issues`, external: true },
      { label: "Support Echo", to: "/community#support" },
      { label: "Private deployment", to: "/community#private" },
    ],
  },
  {
    title: "Company",
    links: [
      { label: "About", to: "/about" },
      { label: "Contact", to: "/contact" },
      { label: "Security", to: "/security" },
    ],
  },
  {
    title: "Legal",
    links: [
      { label: "Privacy", to: "/privacy" },
      { label: "Terms", to: "/terms" },
    ],
  },
];

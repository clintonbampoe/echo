import type { RouteObject } from "react-router-dom";
import { SiteLayout } from "./components/chrome/SiteLayout";

// A page that loads when someone first goes to it.
const page = (
  load: () => Promise<{ Component: RouteObject["Component"] }>,
) => ({
  lazy: load,
});

// Every public page of the website. They share one frame (header, footer, background).
export const websiteRoutes: RouteObject[] = [
  {
    Component: SiteLayout,
    children: [
      {
        index: true,
        ...page(async () => ({
          Component: (await import("./pages/HomePage")).HomePage,
        })),
      },
      {
        path: "features",
        ...page(async () => ({
          Component: (await import("./pages/FeaturesPage")).FeaturesPage,
        })),
      },
      {
        path: "docs",
        ...page(async () => ({
          Component: (await import("./pages/DocsPage")).DocsPage,
        })),
      },
      {
        path: "roadmap",
        ...page(async () => ({
          Component: (await import("./pages/RoadmapPage")).RoadmapPage,
        })),
      },
      {
        path: "changelog",
        ...page(async () => ({
          Component: (await import("./pages/ChangelogPage")).ChangelogPage,
        })),
      },
      {
        path: "community",
        ...page(async () => ({
          Component: (await import("./pages/CommunityPage")).CommunityPage,
        })),
      },
      {
        path: "about",
        ...page(async () => ({
          Component: (await import("./pages/AboutPage")).AboutPage,
        })),
      },
      {
        path: "contact",
        ...page(async () => ({
          Component: (await import("./pages/ContactPage")).ContactPage,
        })),
      },
      {
        path: "security",
        ...page(async () => ({
          Component: (await import("./pages/SecurityPage")).SecurityPage,
        })),
      },
      {
        path: "privacy",
        ...page(async () => ({
          Component: (await import("./pages/PrivacyPage")).PrivacyPage,
        })),
      },
      {
        path: "terms",
        ...page(async () => ({
          Component: (await import("./pages/TermsPage")).TermsPage,
        })),
      },
    ],
  },
];

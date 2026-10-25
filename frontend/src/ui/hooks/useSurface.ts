import { useEffect } from "react";
import { useTheme } from "./theme";

// Public pages (website, auth) are light or dark by the person's choice; the signed-in app
// is always light. A public page calls this once and the tokens in styles/tokens.css do the
// rest. The surface is removed when the page unmounts.
export function usePublicSurface() {
  const { theme } = useTheme();
  useEffect(() => {
    const root = document.documentElement;
    root.dataset.surface = theme;
    return () => {
      delete root.dataset.surface;
    };
  }, [theme]);
}

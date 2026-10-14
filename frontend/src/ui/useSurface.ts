import { useEffect } from "react";

// Public pages (landing, auth) are dark; the signed-in app is light. A page asks for its
// surface and the tokens in styles/tokens.css do the rest. Restored when the page unmounts.
export function useSurface(surface: "dark") {
  useEffect(() => {
    const root = document.documentElement;
    root.dataset.surface = surface;
    return () => {
      delete root.dataset.surface;
    };
  }, [surface]);
}

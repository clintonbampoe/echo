import { useEffect } from "react";
import { flushSync } from "react-dom";
import { useNavigate } from "react-router-dom";

const modified = (e: MouseEvent) =>
  e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey;

// Whether following this link moves to another page of the site (not a new tab, a download or a hash).
function toAnotherPage(link: HTMLAnchorElement) {
  if ((link.target && link.target !== "_self") || link.hasAttribute("download"))
    return false;
  const url = new URL(link.href);
  const same =
    url.pathname === location.pathname && url.search === location.search;
  return url.origin === location.origin && !same;
}

// The link this click follows, if it is one the router would handle.
function linkOf(e: MouseEvent) {
  if (e.defaultPrevented || modified(e) || !(e.target instanceof Element))
    return null;
  const link = e.target.closest<HTMLAnchorElement>("a[href]");
  return link && toAnotherPage(link) ? link : null;
}

// A link between pages is a soft change, not a cut: the new page fades in over the old one while its sections
// arrive (styles/motion.css). Without View Transitions, or with reduced motion, links work as they always did.
export function useSiteNavigation() {
  const navigate = useNavigate();
  useEffect(() => {
    const still =
      typeof matchMedia !== "function" ||
      matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (still || !document.startViewTransition) return;
    const root = document.documentElement;
    const onClick = (e: MouseEvent) => {
      const link = linkOf(e);
      if (!link) return;
      const url = new URL(link.href);
      // Stopping the default is enough: the link's own click handler still runs (closing a menu, say),
      // and the router sees the click as already handled.
      e.preventDefault();
      root.dataset.vtSite = "";
      const move = document.startViewTransition(() =>
        flushSync(() => navigate(url.pathname + url.search + url.hash)),
      );
      void move.finished.finally(() => delete root.dataset.vtSite);
    };
    // Captured, so this runs before the router's own handler.
    document.addEventListener("click", onClick, true);
    return () => document.removeEventListener("click", onClick, true);
  }, [navigate]);
}

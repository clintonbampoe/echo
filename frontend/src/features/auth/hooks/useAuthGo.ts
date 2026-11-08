import { flushSync } from "react-dom";
import { useNavigate, type NavigateOptions } from "react-router-dom";

// Moving between auth screens: the title and card travel to where they sit on the next screen and
// swipe sideways (left going on, right going back to sign in), while the rest fades. Without View
// Transitions (or with reduced motion) it is a plain navigation.
export function useAuthGo() {
  const navigate = useNavigate();
  return function go(to: string, options?: NavigateOptions) {
    const still =
      typeof matchMedia !== "function" ||
      matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (still || !document.startViewTransition) return navigate(to, options);
    const root = document.documentElement;
    // The shared names exist only while moving, so they do not disturb other transitions.
    root.dataset.vtNav = to === "/signin" ? "back" : "forward";
    const transition = document.startViewTransition(() =>
      flushSync(() => navigate(to, options)),
    );
    void transition.finished.finally(() => delete root.dataset.vtNav);
  };
}

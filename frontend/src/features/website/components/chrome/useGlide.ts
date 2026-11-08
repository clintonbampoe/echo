import { useEffect, useRef, useState, type CSSProperties } from "react";
import { useLocation } from "react-router-dom";

type Box = { x: number; y: number; w: number; h: number };

// Where `el` sits inside `parent`, or null when either is missing.
function boxOf(
  parent: Element | null,
  el: Element | null | undefined,
): Box | null {
  if (!parent || !el) return null;
  const a = el.getBoundingClientRect();
  const b = parent.getBoundingClientRect();
  return { x: a.left - b.left, y: a.top - b.top, w: a.width, h: a.height };
}

// A pill that slides to the link under the pointer or keyboard focus, and rests on the current
// page's link otherwise. Positions are measured, never guessed, so it follows any font or width.
export function useGlide() {
  const nav = useRef<HTMLElement>(null);
  const [box, setBox] = useState<Box | null>(null);
  const { pathname } = useLocation();

  function measure(el: Element | null | undefined) {
    setBox(boxOf(nav.current, el));
  }

  function rest() {
    measure(nav.current?.querySelector('[aria-current="page"]'));
  }

  // Settle on the current page's link after navigating, and again if the window changes size.
  useEffect(() => {
    const settle = () =>
      setBox(
        boxOf(nav.current, nav.current?.querySelector('[aria-current="page"]')),
      );
    settle();
    window.addEventListener("resize", settle);
    return () => window.removeEventListener("resize", settle);
  }, [pathname]);

  const style: CSSProperties = box
    ? {
        width: box.w,
        height: box.h,
        transform: `translate3d(${box.x}px, ${box.y}px, 0)`,
        opacity: 1,
      }
    : { opacity: 0 };

  return {
    navRef: nav,
    style,
    onEnter: (e: { currentTarget: Element }) => measure(e.currentTarget),
    onLeave: rest,
  };
}

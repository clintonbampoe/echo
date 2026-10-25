import {
  useEffect,
  useRef,
  useState,
  type CSSProperties,
  type ReactNode,
} from "react";

type Props = {
  children: ReactNode;
  // Milliseconds. Use to stagger siblings.
  delay?: number;
  className?: string;
};

// Fades and lifts its children once, when they scroll into view. Motion is defined in
// styles/motion.css (.reveal) and switched off there for prefers-reduced-motion.
export function Reveal({ children, delay = 0, className }: Props) {
  const ref = useRef<HTMLDivElement>(null);
  // Without IntersectionObserver (old browsers, tests) show the content straight away.
  const [visible, setVisible] = useState(
    typeof IntersectionObserver === "undefined",
  );

  useEffect(() => {
    const el = ref.current;
    if (!el || visible) return;
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (!entry?.isIntersecting) return;
        setVisible(true);
        observer.disconnect();
      },
      { threshold: 0.15, rootMargin: "0px 0px -8% 0px" },
    );
    observer.observe(el);
    return () => observer.disconnect();
  }, [visible]);

  return (
    <div
      ref={ref}
      data-visible={visible}
      className={["reveal", className].filter(Boolean).join(" ")}
      style={{ "--delay": `${delay}ms` } as CSSProperties}
    >
      {children}
    </div>
  );
}

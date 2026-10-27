const RINGS = [70, 130, 190, 250, 320, 400];
const OPACITY = [0.1, 0.082, 0.065, 0.05, 0.039, 0.029];

// Rings spreading out from the middle, like a sound. Decoration only.
export function Ripples({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      width="800"
      height="800"
      viewBox="-400 -400 800 800"
      fill="none"
      aria-hidden="true"
    >
      {RINGS.map((r, i) => (
        <circle
          key={r}
          r={r}
          stroke="currentColor"
          strokeOpacity={OPACITY[i]}
          strokeWidth="1.5"
          style={{
            transformOrigin: "center",
            transformBox: "fill-box",
            animation: `echoPing 7s ease-out ${i * -1.1}s infinite`,
          }}
        />
      ))}
    </svg>
  );
}

import { renderHook } from "@testing-library/react";
import { useHeld } from "./useHeld";

describe("useHeld", () => {
  it("keeps the last real value after the value goes back to null", () => {
    const { result, rerender } = renderHook(
      ({ v }: { v: string | null }) => useHeld(v),
      { initialProps: { v: null as string | null } },
    );
    expect(result.current).toBeNull();
    rerender({ v: "Ama" });
    expect(result.current).toBe("Ama");
    rerender({ v: null });
    expect(result.current).toBe("Ama");
    rerender({ v: "Kofi" });
    expect(result.current).toBe("Kofi");
  });
});

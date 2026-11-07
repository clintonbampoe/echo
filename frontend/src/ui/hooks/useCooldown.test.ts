import { act, renderHook } from "@testing-library/react";
import { useCooldown } from "./useCooldown";

describe("useCooldown", () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it("is ready until started, then counts down to ready again", () => {
    const { result } = renderHook(() => useCooldown(3));
    expect(result.current[0]).toBe(0);

    act(() => result.current[1]());
    expect(result.current[0]).toBe(3);

    act(() => vi.advanceTimersByTime(1000));
    expect(result.current[0]).toBe(2);

    // One second at a time: each tick sets up the next once React has drawn it.
    act(() => vi.advanceTimersByTime(1000));
    act(() => vi.advanceTimersByTime(1000));
    expect(result.current[0]).toBe(0);
  });

  it("counts the time the page spent paused in another app", () => {
    const { result } = renderHook(() => useCooldown(30));
    act(() => result.current[1]());

    // The phone froze the page for 20s: no timers ran, only the clock moved.
    act(() => {
      vi.setSystemTime(Date.now() + 20_000);
      document.dispatchEvent(new Event("visibilitychange"));
    });
    expect(result.current[0]).toBe(10);
  });
});

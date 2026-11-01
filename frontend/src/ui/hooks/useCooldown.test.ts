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
});

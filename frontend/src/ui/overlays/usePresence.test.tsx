import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { exitAnimations, usePresence } from "./usePresence";

beforeEach(() => {
  exitAnimations.enabled = true;
});
afterEach(() => {
  exitAnimations.enabled = false;
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

describe("usePresence", () => {
  it("keeps the thing mounted, marked closing, until the exit animation has played", () => {
    vi.useFakeTimers();
    vi.stubGlobal("matchMedia", () => ({ matches: false }));
    const { result, rerender } = renderHook(
      ({ open }) => usePresence(open, 200),
      { initialProps: { open: true } },
    );
    expect(result.current).toEqual({ mounted: true, closing: false });

    rerender({ open: false });
    expect(result.current).toEqual({ mounted: true, closing: true });

    act(() => vi.advanceTimersByTime(200));
    expect(result.current).toEqual({ mounted: false, closing: false });
  });

  it("leaves at once when there is no motion to wait for", () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(
      ({ open }) => usePresence(open, 200),
      { initialProps: { open: true } },
    );
    rerender({ open: false });
    act(() => vi.advanceTimersByTime(0));
    expect(result.current.mounted).toBe(false);
  });

  it("mounts again if it is reopened mid-exit", () => {
    vi.useFakeTimers();
    vi.stubGlobal("matchMedia", () => ({ matches: false }));
    const { result, rerender } = renderHook(
      ({ open }) => usePresence(open, 200),
      { initialProps: { open: true } },
    );
    rerender({ open: false });
    rerender({ open: true });
    act(() => vi.advanceTimersByTime(300));
    expect(result.current).toEqual({ mounted: true, closing: false });
  });
});

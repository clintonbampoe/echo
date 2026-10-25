import { render, screen } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { act } from "react";
import { Presence } from "./Presence";
import { useLeaving } from "./presenceContext";
import { exitAnimations } from "./usePresence";

function Probe() {
  return <p>{useLeaving() ? "leaving" : "here"}</p>;
}

beforeEach(() => {
  exitAnimations.enabled = true;
  vi.useFakeTimers();
  vi.stubGlobal("matchMedia", () => ({ matches: false }));
});
afterEach(() => {
  exitAnimations.enabled = false;
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

describe("presence", () => {
  it("tells what is inside it that it is leaving, then removes it", () => {
    const { rerender } = render(
      <Presence open>
        <Probe />
      </Presence>,
    );
    expect(screen.getByText("here")).toBeInTheDocument();

    rerender(
      <Presence open={false}>
        <Probe />
      </Presence>,
    );
    expect(screen.getByText("leaving")).toBeInTheDocument();

    act(() => vi.advanceTimersByTime(300));
    expect(screen.queryByText("leaving")).toBeNull();
  });
});

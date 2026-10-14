import { act, render, screen } from "@testing-library/react";
import { Reveal } from "./Reveal";

describe("Reveal", () => {
  it("shows its content straight away when IntersectionObserver is missing", () => {
    render(<Reveal>hello</Reveal>);
    expect(screen.getByText("hello")).toHaveAttribute("data-visible", "true");
  });

  it("stays hidden until it enters the viewport, then reveals once", () => {
    let trigger: (
      entries: Pick<IntersectionObserverEntry, "isIntersecting">[],
    ) => void = () => {};
    class FakeObserver {
      constructor(cb: typeof trigger) {
        trigger = cb;
      }
      observe() {}
      disconnect() {}
    }
    vi.stubGlobal("IntersectionObserver", FakeObserver);

    render(<Reveal>later</Reveal>);
    const el = screen.getByText("later");
    expect(el).toHaveAttribute("data-visible", "false");

    act(() => trigger([{ isIntersecting: true }]));
    expect(el).toHaveAttribute("data-visible", "true");
    vi.unstubAllGlobals();
  });
});

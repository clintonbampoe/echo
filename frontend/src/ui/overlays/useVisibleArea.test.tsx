import { render, screen } from "@testing-library/react";
import { useVisibleArea } from "./useVisibleArea";

class FakeViewport extends EventTarget {
  height = 800;
  offsetTop = 0;
  scale = 1;
}

let view: FakeViewport;
beforeEach(() => {
  view = new FakeViewport();
  vi.stubGlobal("innerHeight", 800);
  Object.defineProperty(window, "visualViewport", {
    value: view,
    configurable: true,
  });
});
afterEach(() => {
  Object.defineProperty(window, "visualViewport", {
    value: undefined,
    configurable: true,
  });
  vi.unstubAllGlobals();
});

function Overlay() {
  const root = useVisibleArea(true);
  return <div ref={root} data-testid="root" />;
}

function mount() {
  const view = render(<Overlay />);
  return { el: screen.getByTestId("root"), ...view };
}

describe("useVisibleArea", () => {
  it("follows the part of the screen the keyboard leaves", () => {
    const { el } = mount();
    expect(el.dataset.keyboard).toBe("false");

    view.height = 450;
    view.dispatchEvent(new Event("resize"));
    expect(el.style.getPropertyValue("--vv-height")).toBe("450px");
    expect(el.dataset.keyboard).toBe("true");
  });

  it("follows the page iOS scrolls up to show the field", () => {
    const { el } = mount();
    view.height = 450;
    view.offsetTop = 120;
    view.dispatchEvent(new Event("scroll"));
    expect(el.style.getPropertyValue("--vv-top")).toBe("120px");
  });

  it("does not take a pinch-zoom for a keyboard", () => {
    const { el } = mount();
    view.height = 400;
    view.scale = 2;
    view.dispatchEvent(new Event("resize"));
    expect(el.dataset.keyboard).toBeUndefined();
    expect(el.style.getPropertyValue("--vv-height")).toBe("");
  });

  it("puts everything back when it stops", () => {
    const { el, unmount } = mount();
    view.height = 450;
    view.dispatchEvent(new Event("resize"));
    unmount();
    expect(el.dataset.keyboard).toBeUndefined();
    expect(el.style.getPropertyValue("--vv-top")).toBe("");
  });
});

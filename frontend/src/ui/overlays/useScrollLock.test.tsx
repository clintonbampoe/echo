import { renderHook } from "@testing-library/react";
import { useScrollLock } from "./useScrollLock";

afterEach(() => {
  document.body.style.overflow = "";
});

describe("useScrollLock", () => {
  it("holds the page still while locked and lets go after", () => {
    const { rerender } = renderHook(({ on }) => useScrollLock(on), {
      initialProps: { on: true },
    });
    expect(document.body.style.overflow).toBe("hidden");
    rerender({ on: false });
    expect(document.body.style.overflow).toBe("");
  });

  it("keeps the page still until the last overlay closes, in any order", () => {
    const first = renderHook(() => useScrollLock(true));
    const second = renderHook(() => useScrollLock(true));

    first.unmount();
    expect(document.body.style.overflow).toBe("hidden");
    second.unmount();
    expect(document.body.style.overflow).toBe("");
  });

  it("puts back the overflow the page had before", () => {
    document.body.style.overflow = "scroll";
    const { unmount } = renderHook(() => useScrollLock(true));
    unmount();
    expect(document.body.style.overflow).toBe("scroll");
  });
});

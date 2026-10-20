import { act, renderHook } from "@testing-library/react";
import { usePeriod } from "./usePeriod";

describe("usePeriod", () => {
  it("starts on last week", () => {
    const { result } = renderHook(() => usePeriod());
    expect(result.current[0]).toBe("last-week");
  });

  it("remembers a pick for the next visit", () => {
    const first = renderHook(() => usePeriod());
    act(() => first.result.current[1]("this-month"));
    expect(first.result.current[0]).toBe("this-month");
    expect(renderHook(() => usePeriod()).result.current[0]).toBe("this-month");
  });

  it("ignores something stored that is not a period", () => {
    localStorage.setItem("echo.dashboard.period", "tomorrow");
    expect(renderHook(() => usePeriod()).result.current[0]).toBe("last-week");
  });
});

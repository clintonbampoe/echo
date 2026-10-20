import { act, renderHook } from "@testing-library/react";
import { MemoryRouter, useLocation } from "react-router-dom";
import { useUrlFlag } from "./useUrlFlag";

const at = (address: string) =>
  renderHook(() => ({ flag: useUrlFlag("new"), location: useLocation() }), {
    wrapper: ({ children }) => (
      <MemoryRouter initialEntries={[address]}>{children}</MemoryRouter>
    ),
  });

describe("useUrlFlag", () => {
  it("is on only when the address says so", () => {
    expect(at("/members").result.current.flag[0]).toBe(false);
    expect(at("/members?new=1").result.current.flag[0]).toBe(true);
    expect(at("/members?new=0").result.current.flag[0]).toBe(false);
  });

  it("turns on and off in the address and keeps the other filters", () => {
    const { result } = at("/members?name=ama");
    act(() => result.current.flag[1](true));
    expect(result.current.flag[0]).toBe(true);
    expect(result.current.location.search).toBe("?name=ama&new=1");
    act(() => result.current.flag[1](false));
    expect(result.current.flag[0]).toBe(false);
    expect(result.current.location.search).toBe("?name=ama");
  });
});

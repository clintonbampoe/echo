import { act, renderHook } from "@testing-library/react";
import type { ReactNode } from "react";
import { MemoryRouter } from "react-router-dom";
import { useUrlFilters } from "./useUrlFilters";
import { urlFilters } from "./urlFilters";

const def = urlFilters({ name: "", status: "" });
const wrap =
  (path: string) =>
  ({ children }: { children: ReactNode }) => (
    <MemoryRouter initialEntries={[path]}>{children}</MemoryRouter>
  );

describe("url filters hook", () => {
  it("reads the filters from the address and writes changes back", () => {
    const { result } = renderHook(() => useUrlFilters(def), {
      wrapper: wrap("/?name=ama"),
    });
    expect(result.current.filters).toEqual({ name: "ama", status: "" });
    act(() => result.current.update({ status: "Active" }));
    expect(result.current.filters).toEqual({ name: "ama", status: "Active" });
  });

  it("resets every filter on clear and changes the key once, so a search box rebuilds empty", () => {
    const { result } = renderHook(() => useUrlFilters(def), {
      wrapper: wrap("/?name=ama&status=Active"),
    });
    expect(result.current.resetKey).toBe(0);
    act(() => result.current.clear());
    expect(result.current.filters).toEqual({ name: "", status: "" });
    expect(result.current.resetKey).toBe(1);
  });
});

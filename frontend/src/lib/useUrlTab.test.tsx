import { act, renderHook } from "@testing-library/react";
import { MemoryRouter, useLocation, useNavigationType } from "react-router-dom";
import { useUrlTab } from "./useUrlTab";

const tabs = [{ id: "details" }, { id: "members" }, { id: "events" }];

const at = (address: string) =>
  renderHook(
    () => ({
      tab: useUrlTab(tabs),
      location: useLocation(),
      how: useNavigationType(),
    }),
    {
      wrapper: ({ children }) => (
        <MemoryRouter initialEntries={[address]}>{children}</MemoryRouter>
      ),
    },
  );

describe("useUrlTab", () => {
  it("shows the tab the address names", () => {
    expect(at("/o/1?tab=members").result.current.tab[0]).toBe("members");
  });

  it("falls back to the first tab when the address names none or an unknown one", () => {
    expect(at("/o/1").result.current.tab[0]).toBe("details");
    expect(at("/o/1?tab=nope").result.current.tab[0]).toBe("details");
  });

  it("writes the pick to the address, keeping the rest of the query", () => {
    const { result } = at("/o/1?tithe=7");
    act(() => result.current.tab[1]("events"));
    expect(result.current.tab[0]).toBe("events");
    expect(result.current.location.search).toBe("?tithe=7&tab=events");
  });

  it("does not add a step to the back button", () => {
    const { result } = at("/o/1");
    act(() => result.current.tab[1]("members"));
    expect(result.current.how).toBe("REPLACE");
  });
});

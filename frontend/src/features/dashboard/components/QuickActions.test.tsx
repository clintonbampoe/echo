import { render, screen, within } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { QuickActions } from "./QuickActions";
import { quickActions } from "../quickActions";

function shortcuts() {
  render(
    <MemoryRouter>
      <QuickActions />
    </MemoryRouter>,
  );
  return within(
    screen.getByRole("region", { name: "Quick actions" }),
  ).getAllByRole("link");
}

describe("QuickActions", () => {
  it("shows the six shortcuts in order, each a link to the page that owns the record", () => {
    expect(shortcuts().map((l) => l.getAttribute("href"))).toEqual([
      "/tithes?new=1",
      "/attendance",
      "/members?new=1",
      "/visitors?new=1",
      "/finance?new=1",
      "/events?new=1",
    ]);
  });

  it("gives every tile a name and a hint", () => {
    const links = shortcuts();
    quickActions.forEach((a, i) => {
      expect(links[i]).toHaveTextContent(a.label);
      expect(links[i]).toHaveTextContent(a.hint);
    });
  });
});

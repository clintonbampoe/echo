import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QuickActions } from "./QuickActions";

function show() {
  render(
    <MemoryRouter>
      <Routes>
        <Route path="/" element={<QuickActions />} />
        <Route path="/events" element={<p>Events page</p>} />
      </Routes>
    </MemoryRouter>,
  );
  return within(screen.getByRole("navigation", { name: "Quick actions" }));
}

describe("QuickActions", () => {
  it("leads with the three shortcuts used most, each a link to the page that owns the record", () => {
    const nav = show();
    expect(
      nav
        .getAllByRole("link")
        .map((l) => [l.textContent, l.getAttribute("href")]),
    ).toEqual([
      ["Take attendance", "/attendance"],
      ["Record tithe", "/tithes?new=1"],
      ["New member", "/members?new=1"],
    ]);
  });

  it("keeps the rest under More, and goes to the one picked", async () => {
    const nav = show();
    await userEvent.click(nav.getByRole("button", { name: /More/ }));
    expect(screen.getAllByRole("option").map((o) => o.textContent)).toEqual([
      "More",
      "Add visitor",
      "Record transaction",
      "New event",
    ]);
    await userEvent.click(screen.getByRole("option", { name: "New event" }));
    expect(await screen.findByText("Events page")).toBeInTheDocument();
  });
});

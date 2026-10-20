import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn } from "../dashboardTestKit";
import { VisitorsJoinedCard } from "./VisitorsJoinedCard";

let answer: () => Response;
beforeEach(() => {
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => answer()),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("visitors who joined card", () => {
  it("shows how many became members, and the visitors added", async () => {
    answer = () =>
      json({
        totalVisitors: 3,
        newVisitors: 4,
        recurringVisitors: 1,
        convertedVisitors: 1,
      });
    renderCard(<VisitorsJoinedCard period="last-week" today="2026-10-19" />);
    expect(
      await screen.findByText("Visitor became a member, last week"),
    ).toBeInTheDocument();
    const rows = within(
      screen.getByRole("list", { name: "Visitors in the period" }),
    );
    expect(rows.getByText("4")).toBeInTheDocument();
    expect(rows.getByText("3")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    answer = () => json({ title: "Server error", status: 500 }, 500);
    renderCard(<VisitorsJoinedCard period="last-week" today="2026-10-19" />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

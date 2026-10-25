import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderCard, signIn, asked } from "../../testing/dashboardPage";
import { MemberGrowthCard } from "./MemberGrowthCard";

// Members who joined, by the first day asked for. 2 of them have no joined date, so they are in every count:
// the months are really 3, 5, 2, 6, 4 and 9 new members, and last week 9.
const counts: Record<string, number> = {
  "2000-01-01": 2,
  "2026-05-01": 5,
  "2026-06-01": 7,
  "2026-07-01": 4,
  "2026-08-01": 8,
  "2026-09-01": 6,
  "2026-10-01": 11,
  "2026-10-11": 11,
};

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path, query } = asked(url);
      if (broken || path !== "/v1/members/summary")
        return json({ title: "Server error", status: 500 }, 500);
      // Everyone, for the total under the figure; the rest are counted by the day they joined.
      if (query.get("to") === "2100-12-31")
        return json({ totalMembers: 1284, ageGroups: [] });
      return json({
        totalMembers: counts[query.get("from") ?? ""] ?? 0,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("member growth card", () => {
  it("shows this month's new members, without the ones who have no joined date, and the total", async () => {
    renderCard(<MemberGrowthCard today="2026-10-19" />);
    expect(await screen.findByText("+9")).toBeInTheDocument();
    expect(screen.getByText("October 2026")).toBeInTheDocument();
    expect(await screen.findByText("1.28k")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "October 2026: 9" }),
    ).toBeInTheDocument();
  });

  it("speaks about the month picked", async () => {
    renderCard(<MemberGrowthCard today="2026-10-19" />);
    await screen.findByText("+9");
    await userEvent.click(
      screen.getByRole("button", { name: "September 2026: 4" }),
    );
    expect(screen.getByText("September 2026")).toBeInTheDocument();
    expect(screen.getByText("+4")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<MemberGrowthCard today="2026-10-19" />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

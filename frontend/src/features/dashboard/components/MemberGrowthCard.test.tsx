import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn } from "../dashboardTestKit";
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
      const u = new URL(url, "http://x");
      if (broken || u.pathname !== "/v1/members/summary")
        return json({ title: "Server error", status: 500 }, 500);
      return json({
        totalMembers: counts[u.searchParams.get("from") ?? ""] ?? 0,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const summary = () => within(screen.getByRole("group", { name: "Summary" }));

describe("member growth card", () => {
  it("shows the new members, without the ones who have no joined date", async () => {
    renderCard(<MemberGrowthCard today="2026-10-19" />);
    expect(await summary().findByText("29")).toBeInTheDocument();
    expect(summary().getByText("9")).toBeInTheDocument();
    expect(summary().getByText("5")).toBeInTheDocument();
    expect(
      screen.getByRole("img", { name: "October 2026: 9" }),
    ).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<MemberGrowthCard today="2026-10-19" />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  json,
  renderCard,
  signIn,
  asked,
  monthlyRows,
} from "../../testing/dashboardPage";
import { MemberGrowthCard } from "./MemberGrowthCard";

// New members each month. Other months answer 0.
const counts: Record<string, number> = {
  "2026-05": 3,
  "2026-06": 5,
  "2026-07": 2,
  "2026-08": 6,
  "2026-09": 4,
  "2026-10": 9,
};

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path, query } = asked(url);
      if (broken) return json({ title: "Server error", status: 500 }, 500);
      if (path === "/v1/members/summary/monthly")
        return json(
          monthlyRows(query, (month) => ({ newMembers: counts[month] ?? 0 })),
        );
      // Everyone, for the total under the figure.
      if (path === "/v1/members/summary")
        return json({ totalMembers: 1284, ageGroups: [] });
      return json({ title: "Not found", status: 404 }, 404);
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("member growth card", () => {
  it("shows this month's new members and the total", async () => {
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

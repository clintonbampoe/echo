import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn, asked } from "../../testing/dashboardPage";
import { MembersCard } from "./MembersCard";

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { query } = asked(url);
      if (broken) return json({ title: "Server error", status: 500 }, 500);
      // Everyone, then the members who joined this year, then the ones with no joined date.
      if (query.get("to") === "2100-12-31")
        return json({
          totalMembers: 248,
          activeMembers: 201,
          maleCount: 114,
          femaleCount: 134,
          averageAge: 36,
          ageGroups: [
            { label: "Under 18", count: 22 },
            { label: "30-39", count: 84 },
          ],
        });
      return json({
        totalMembers: query.get("from") === "2000-01-01" ? 2 : 18,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("members card", () => {
  it("shows women and men, the age groups, and the status of the members", async () => {
    renderCard(<MembersCard today="2026-10-19" />);
    expect(await screen.findByText("248 total")).toBeInTheDocument();
    const gender = within(screen.getByRole("list", { name: "Women and men" }));
    expect(gender.getByText("134")).toBeInTheDocument();
    expect(gender.getByText("54%")).toBeInTheDocument();
    expect(screen.getByTitle("30-39: 84")).toBeInTheDocument();
    const status = within(
      screen.getByRole("list", { name: "Members by status" }),
    );
    expect(status.getByText("201")).toBeInTheDocument();
    expect(status.getByText("47")).toBeInTheDocument();
    expect(await status.findByText("16")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<MembersCard today="2026-10-19" />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

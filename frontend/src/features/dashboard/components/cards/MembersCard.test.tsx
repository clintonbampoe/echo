import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
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
      // Everyone, with how many joined this year in the same answer.
      if (query.get("to") === "2100-12-31")
        return json({
          joinedThisYear: 16,
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
      return json({ joinedInWindow: 0, joinedInPreviousWindow: 0 });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("members card", () => {
  it("shows women and men, the age groups, and the status of the members", async () => {
    renderCard(<MembersCard />);
    expect(await screen.findByText("248 total")).toBeInTheDocument();
    const gender = within(screen.getByRole("list", { name: "Women and men" }));
    expect(gender.getByText("134")).toBeInTheDocument();
    expect(gender.getByText("54%")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /^30-39 years: 84/ }),
    ).toBeInTheDocument();
    const status = within(
      screen.getByRole("list", { name: "Members by status" }),
    );
    expect(status.getByText("201")).toBeInTheDocument();
    expect(status.getByText("47")).toBeInTheDocument();
    expect(await status.findByText("16")).toBeInTheDocument();
  });

  it("darkens the biggest age group, lets another be picked, and goes back on a second click", async () => {
    renderCard(<MembersCard />);
    const biggest = await screen.findByRole("button", { name: /^30-39 years/ });
    const young = screen.getByRole("button", { name: /^Under 18 years/ });
    expect(biggest).toHaveAttribute("aria-pressed", "true");
    await userEvent.click(young);
    expect(young).toHaveAttribute("aria-pressed", "true");
    expect(biggest).toHaveAttribute("aria-pressed", "false");
    await userEvent.click(young);
    expect(biggest).toHaveAttribute("aria-pressed", "true");
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<MembersCard />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

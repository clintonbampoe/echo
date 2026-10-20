import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn } from "../dashboardTestKit";
import { GenderCard } from "./GenderCard";

let answer: () => Response;
beforeEach(() => {
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => answer()),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("gender card", () => {
  it("shows each share, with everyone who is neither under Other", async () => {
    answer = () =>
      json({
        totalMembers: 1284,
        activeMembers: 1200,
        maleCount: 692,
        femaleCount: 578,
        averageAge: 38,
      });
    renderCard(<GenderCard />);
    const rows = within(await screen.findByRole("list"));
    expect(rows.getByText("Male")).toBeInTheDocument();
    expect(rows.getByText("54%")).toBeInTheDocument();
    expect(rows.getByText("Other")).toBeInTheDocument();
    expect(rows.getByText("14")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    answer = () => json({ title: "Server error", status: 500 }, 500);
    renderCard(<GenderCard />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

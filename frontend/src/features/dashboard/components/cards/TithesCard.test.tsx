import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderCard, signIn, asked } from "../../testing/dashboardPage";
import { TithesCard } from "./TithesCard";

// Tithes of each month, by the day the month began. Other months answer 4k.
const months: Record<string, number> = {
  "2026-09-01": 10640,
  "2026-10-01": 11300,
};

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path, query } = asked(url);
      if (broken || path !== "/v1/tithes/summary")
        return json({ title: "Server error", status: 500 }, 500);
      return json({
        totalCollected: months[query.get("from") ?? ""] ?? 4000,
        uniqueTithers: 214,
        averagePerMember: 52.8,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const sides = () => within(screen.getByRole("group", { name: "Made up of" }));

describe("tithes card", () => {
  it("shows this month's tithes, how many people gave and the average gift", async () => {
    renderCard(<TithesCard today="2026-10-19" />);
    expect(await screen.findByText("11.3k")).toBeInTheDocument();
    expect(sides().getByText("214")).toBeInTheDocument();
    expect(sides().getByText("53")).toBeInTheDocument();
  });

  it("speaks about the month picked", async () => {
    renderCard(<TithesCard today="2026-10-19" />);
    await screen.findByText("11.3k");
    await userEvent.click(
      screen.getByRole("button", { name: "September 2026: GH₵ 10.64k" }),
    );
    expect(screen.getByText("10.64k")).toBeInTheDocument();
    expect(screen.getByText("September 2026")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<TithesCard today="2026-10-19" />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

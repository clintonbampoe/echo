import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderCard, signIn } from "../dashboardTestKit";
import { TithesCard } from "./TithesCard";

// Tithes for each of the last eight weeks, by the day the week began. Other days answer 500.
const weeks: Record<string, number> = {
  "2026-08-23": 8900,
  "2026-08-30": 9500,
  "2026-09-06": 9100,
  "2026-09-13": 9800,
  "2026-09-20": 9600,
  "2026-09-27": 10100,
  "2026-10-04": 10640,
  "2026-10-11": 11300,
};

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const u = new URL(url, "http://x");
      if (broken || u.pathname !== "/v1/tithes/summary")
        return json({ title: "Server error", status: 500 }, 500);
      return json({
        totalCollected: weeks[u.searchParams.get("from") ?? ""] ?? 4000,
        uniqueTithers: 214,
        averagePerMember: 52.8,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const summary = () => within(screen.getByRole("group", { name: "Summary" }));

describe("tithes card", () => {
  it("shows last week's figures and how it compares with the week before", async () => {
    renderCard(<TithesCard today="2026-10-19" />);
    expect(await summary().findByText("GH₵ 11,300")).toBeInTheDocument();
    expect(summary().getByText("214")).toBeInTheDocument();
    expect(summary().getByText("GH₵ 53")).toBeInTheDocument();
    expect(
      await screen.findByText("6.2% up on the week before"),
    ).toBeInTheDocument();
  });

  it("counts months when another range is picked", async () => {
    renderCard(<TithesCard today="2026-10-19" />);
    await summary().findByText("GH₵ 11,300");
    await userEvent.click(
      screen.getByRole("button", { name: "Range: Last 8 weeks" }),
    );
    await userEvent.click(
      screen.getByRole("option", { name: "Last 6 months" }),
    );
    expect(await summary().findByText("Last month")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<TithesCard today="2026-10-19" />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

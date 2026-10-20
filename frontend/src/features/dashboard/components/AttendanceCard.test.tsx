import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderCard, signIn, json, asked } from "../dashboardTestKit";
import { AttendanceCard } from "./AttendanceCard";

// People present in each of the last eight weeks, by the day the week began. Any other day answers 500.
const weeks: Record<string, number> = {
  "2026-08-23": 800,
  "2026-08-30": 820,
  "2026-09-06": 810,
  "2026-09-13": 840,
  "2026-09-20": 830,
  "2026-09-27": 930,
  "2026-10-04": 860,
  "2026-10-11": 912,
};

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path, query } = asked(url);
      if (broken || path !== "/v1/attendance/summary")
        return json({ title: "Server error", status: 500 }, 500);
      return json({
        totalPresent: weeks[query.get("from") ?? ""] ?? 500,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () => renderCard(<AttendanceCard today="2026-10-19" />);

const summary = () => within(screen.getByRole("group", { name: "Summary" }));

describe("attendance card", () => {
  it("shows the last week, the average and the best week", async () => {
    show();
    expect(await summary().findByText("912")).toBeInTheDocument();
    expect(summary().getByText("850")).toBeInTheDocument();
    expect(summary().getByText("930")).toBeInTheDocument();
    expect(summary().getByText("Average per week")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Week of 11 Oct: 912" }),
    ).toBeInTheDocument();
  });

  it("speaks about the week picked, and goes back to the latest", async () => {
    show();
    await summary().findByText("912");
    const bar = screen.getByRole("button", { name: "Week of 4 Oct: 860" });
    await userEvent.click(bar);
    expect(summary().getByText("Week of 4 Oct")).toBeInTheDocument();
    expect(summary().getByText("860")).toBeInTheDocument();
    await userEvent.click(bar);
    expect(summary().getByText("Last week")).toBeInTheDocument();
  });

  it("counts months when asked for six months", async () => {
    show();
    await userEvent.click(screen.getByRole("radio", { name: "6 months" }));
    expect(await summary().findByText("Average per month")).toBeInTheDocument();
    expect(
      await screen.findByRole("button", { name: "April 2026: 500" }),
    ).toBeInTheDocument();
  });

  it("says so when it could not load, and offers another try", async () => {
    broken = true;
    show();
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Try again" }),
    ).toBeInTheDocument();
  });
});

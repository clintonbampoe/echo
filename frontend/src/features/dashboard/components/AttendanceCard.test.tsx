import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderCard, signIn, json, asked } from "../dashboardTestKit";
import { AttendanceCard } from "./AttendanceCard";

// Who was present in each month, by the day the month began. Any other month answers with 100.
const months: Record<string, [number, number, number]> = {
  "2026-09-01": [880, 700, 180],
  "2026-10-01": [912, 764, 148],
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
      const [total, members, visitors] = months[query.get("from") ?? ""] ?? [
        100, 80, 20,
      ];
      return json({
        totalPresent: total,
        membersPresent: members,
        visitorsPresent: visitors,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () =>
  renderCard(<AttendanceCard today="2026-10-19" title="Attendance" />);
const sides = () => within(screen.getByRole("group", { name: "Made up of" }));

describe("attendance card", () => {
  it("speaks about this month: who was present, and how many were members and visitors", async () => {
    show();
    expect(
      await screen.findByText("912", { selector: "p" }),
    ).toBeInTheDocument();
    expect(screen.getByText("October 2026")).toBeInTheDocument();
    expect(sides().getByText("764")).toBeInTheDocument();
    expect(sides().getByText("148")).toBeInTheDocument();
  });

  it("speaks about the month picked, and goes back to the latest when it is picked again", async () => {
    show();
    await screen.findByText("912", { selector: "p" });
    const bar = screen.getByRole("button", { name: "September 2026: 880" });
    await userEvent.click(bar);
    expect(screen.getByText("September 2026")).toBeInTheDocument();
    expect(sides().getByText("700")).toBeInTheDocument();
    await userEvent.click(bar);
    expect(screen.getByText("October 2026")).toBeInTheDocument();
  });

  it("draws six months when asked", async () => {
    show();
    await screen.findByText("912", { selector: "p" });
    expect(screen.getAllByRole("button", { name: /20\d\d: / })).toHaveLength(
      12,
    );
    await userEvent.click(screen.getByRole("button", { name: /Range/ }));
    await userEvent.click(screen.getByRole("option", { name: "6 months" }));
    expect(screen.getAllByRole("button", { name: /20\d\d: / })).toHaveLength(6);
  });

  it("says so when it could not load", async () => {
    broken = true;
    show();
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

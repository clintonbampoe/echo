import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderCard, signIn } from "../dashboardTestKit";
import { EventCalendarCard } from "./EventCalendarCard";

const event = (id: string, name: string, day: string) => ({
  id,
  name,
  startDate: `${day}T00:00:00`,
  endDate: `${day}T00:00:00`,
  registrationCount: 0,
});

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async () =>
      broken
        ? json({ title: "Server error", status: 500 }, 500)
        : json({
            data: [
              event("e1", "Youth Rally", "2026-10-24"),
              event("e2", "Harvest Thanksgiving", "2026-11-01"),
            ],
            hasMore: false,
            next: null,
          }),
    ),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("event calendar card", () => {
  it("marks the days with an event and says what is on the next one", async () => {
    renderCard(<EventCalendarCard today="2026-10-19" />);
    expect(await screen.findByText("October 2026")).toBeInTheDocument();
    const day = await screen.findByRole("button", { name: /Youth Rally/ });
    expect(day).toHaveTextContent("24");
    expect(screen.getByText("Sat 24 Oct, Youth Rally")).toBeInTheDocument();
    // Only days of this month are drawn: the 1 November event is not a button yet.
    expect(
      screen.queryByRole("button", { name: /Harvest/ }),
    ).not.toBeInTheDocument();
  });

  it("moves to the next month", async () => {
    renderCard(<EventCalendarCard today="2026-10-19" />);
    await screen.findByRole("button", { name: /Youth Rally/ });
    await userEvent.click(screen.getByRole("button", { name: "Next month" }));
    expect(await screen.findByText("November 2026")).toBeInTheDocument();
    expect(
      await screen.findByRole("button", { name: /Harvest Thanksgiving/ }),
    ).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<EventCalendarCard today="2026-10-19" />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

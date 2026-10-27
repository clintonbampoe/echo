import { screen } from "@testing-library/react";
import { json, renderCard, signIn } from "../../testing/dashboardPage";
import { UpcomingEventsCard } from "./UpcomingEventsCard";

const event = (
  id: string,
  name: string,
  startDate: string,
  startTime: string,
) => ({
  id,
  name,
  startDate: `${startDate}T00:00:00`,
  endDate: `${startDate}T00:00:00`,
  startTime,
  registrationCount: 86,
});

let answer: () => Response;
beforeEach(() => {
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => answer()),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("upcoming events card", () => {
  it("lists the next events, each a link to the event", async () => {
    answer = () =>
      json({
        data: [event("e1", "Youth Rally", "2026-10-24", "16:00:00")],
        hasMore: false,
        next: null,
      });
    renderCard(<UpcomingEventsCard today="2026-10-19" />);
    const link = await screen.findByRole("link", { name: /Youth Rally/ });
    expect(link).toHaveAttribute("href", "/events/e1");
    expect(screen.getByText("Sat 24 Oct, 4:00 pm")).toBeInTheDocument();
  });

  it("says so when there are none", async () => {
    answer = () => json({ data: [], hasMore: false, next: null });
    renderCard(<UpcomingEventsCard today="2026-10-19" />);
    expect(await screen.findByText("No upcoming events")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    answer = () => json({ title: "Server error", status: 500 }, 500);
    renderCard(<UpcomingEventsCard today="2026-10-19" />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { EventItem, EventSummary } from "../api";
import { EventsPage } from "./EventsPage";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const rally: EventItem = {
  id: "e1",
  name: "Youth Rally",
  organizationId: "o1",
  organizationName: "Youth Ministry",
  organizerId: "m1",
  organizerName: "Ama Mensah",
  startDate: "2099-10-17",
  endDate: "2099-10-17",
  startTime: "16:00:00",
  location: "Main Auditorium",
  capacity: 150,
  createdAt: "2099-01-01T00:00:00Z",
  registrationCount: 84,
};
const carols: EventItem = {
  ...rally,
  id: "e2",
  name: "Christmas Carols",
  organizationName: "Church Choir",
  capacity: null,
  registrationCount: 12,
  location: null,
};
const summary: EventSummary = {
  totalEvents: 24,
  upcomingEvents: 6,
  totalRegistered: 1284,
  totalAttended: 1102,
};

let rows: EventItem[];
let stats: EventSummary;
let urls: URL[];
beforeEach(() => {
  rows = [rally, carols];
  stats = summary;
  urls = [];
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const u = new URL(url, "http://x");
      urls.push(u);
      if (u.pathname.endsWith("/summary")) return json(stats);
      if (u.pathname.endsWith("/organizations"))
        return json({
          data: [
            {
              id: "o1",
              name: "Youth Ministry",
              createdAt: "2016-02-03T09:00:00Z",
              memberCount: 4,
            },
          ],
          hasMore: false,
          next: null,
        });
      return json({ data: rows, hasMore: false, next: null });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const open = (tab: "upcoming" | "past" = "upcoming", address = "/events") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <EventsPage tab={tab} />
      </MemoryRouter>
    </QueryClientProvider>,
  );

describe("events page", () => {
  it("shows the numbers, the two tabs and a row for each event, linking to its page", async () => {
    open();
    const row = await screen.findByRole("link", { name: /Youth Rally/ });
    expect(row).toHaveAttribute("href", "/events/e1");
    expect(row).toHaveTextContent("84 registered");
    expect(row).toHaveTextContent("Registered");
    expect(row).toHaveTextContent("56% of 150");
    expect(
      screen.getByRole("link", { name: /Christmas Carols/ }),
    ).toHaveTextContent("No limit");
    expect(await screen.findByText("1.28k")).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: "Upcoming" })).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: "Past" })).toBeInTheDocument();
  });

  it("asks the upcoming tab for events from today, soonest first", async () => {
    open();
    await screen.findByRole("link", { name: /Youth Rally/ });
    const list = urls.find(
      (u) =>
        u.pathname.endsWith("/events") &&
        u.searchParams.get("pageSize") === "50",
    );
    expect(list?.searchParams.get("order")).toBe("Asc");
    expect(list?.searchParams.get("from")).toBe(
      new Date().toISOString().slice(0, 10),
    );
  });

  it("asks the past tab for the newest first, and shows how many of those registered came when it is known", async () => {
    rows = [
      {
        ...rally,
        startDate: "2020-03-01",
        endDate: "2020-03-01",
        attendanceCount: 41,
      },
    ];
    open("past", "/events/past?window=all");
    expect(await screen.findByText("Came")).toBeInTheDocument();
    expect(screen.getByText("49%")).toBeInTheDocument();
    expect(screen.getByText("of 84")).toBeInTheDocument();
    const list = urls.find(
      (u) =>
        u.pathname.endsWith("/events") &&
        u.searchParams.get("order") === "Desc",
    );
    expect(list).toBeDefined();
    expect(screen.getByText(/Newest first/)).toBeInTheDocument();
  });

  it("invites the first event when there are none at all, and hides the numbers and filters", async () => {
    rows = [];
    stats = {
      totalEvents: 0,
      upcomingEvents: 0,
      totalRegistered: 0,
      totalAttended: 0,
    };
    open();
    expect(
      await screen.findByText("Plan your first event"),
    ).toBeInTheDocument();
    expect(screen.queryByRole("searchbox")).toBeNull();
    expect(screen.queryByText("Registered")).toBeNull();
  });

  it("says there is nothing coming up, rather than inviting a first event, when others exist", async () => {
    rows = [];
    open();
    expect(await screen.findByText("No upcoming events")).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: /Past/ })).toBeInTheDocument();
  });

  it("says nothing matches when a filter is on", async () => {
    rows = [];
    open("upcoming", "/events?window=all");
    expect(
      await screen.findByText("No upcoming events match"),
    ).toBeInTheDocument();
  });

  it("opens the new event sheet from the header button", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("link", { name: /Youth Rally/ });
    await user.click(screen.getByRole("button", { name: "New event" }));
    expect(
      await screen.findByRole("dialog", { name: "New event" }),
    ).toBeInTheDocument();
  });

  it("offers the same button on the empty page", async () => {
    rows = [];
    stats = {
      totalEvents: 0,
      upcomingEvents: 0,
      totalRegistered: 0,
      totalAttended: 0,
    };
    open();
    expect(
      await screen.findByRole("button", { name: "New event" }),
    ).toBeInTheDocument();
  });

  it("opens the edit sheet from the menu on its row", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("link", { name: /Youth Rally/ });
    await user.click(
      screen.getByRole("button", { name: "Actions for Youth Rally" }),
    );
    await user.click(await screen.findByRole("menuitem", { name: "Edit" }));
    expect(
      await screen.findByRole("dialog", { name: "Edit Youth Rally" }),
    ).toBeInTheDocument();
  });
});

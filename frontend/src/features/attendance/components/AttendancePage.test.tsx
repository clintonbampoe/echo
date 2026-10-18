import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  asked,
  json,
  page,
  renderAt,
  route,
  service,
  session,
  signIn,
  summary,
} from "../attendanceTestKit";
import { AttendancePage } from "./AttendancePage";

let sessions: unknown[];
let labels: unknown[];
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  sessions = [
    session(),
    session({
      id: "s2",
      name: "Evening Prayer",
      topic: null,
      startsAt: "18:00:00",
      endsAt: null,
      status: "Scheduled",
      attendanceCount: 0,
    }),
    session({ id: "s3", date: "2026-10-18", status: "Scheduled" }),
  ];
  labels = [service];
  signIn();
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/attendance/labels")) return json(labels);
    if (u.pathname.endsWith("/attendance/summary")) return json(summary);
    if (u.pathname.endsWith("/attendance/sessions") && init?.method === "POST")
      return json(session({ id: "new", date: "2026-10-12" }), 201);
    if (u.pathname.endsWith("/attendance/sessions")) return page(sessions);
    return json({}, 404);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

const open = (address = "/attendance?day=2026-10-11") =>
  renderAt(address, route("/attendance", <AttendancePage />));

describe("attendance page", () => {
  it("shows the day, its sessions and the numbers", async () => {
    open();
    const heading = await screen.findByRole("heading", {
      name: /Sunday, 11 October 2026/,
    });
    expect(await within(heading).findByText("2 sessions")).toBeInTheDocument();
    const list = await screen.findByRole("list", { name: "Sessions" });
    expect(
      within(list).getByRole("link", { name: "Sunday Service" }),
    ).toHaveAttribute("href", "/attendance/sessions/s1");
    expect(
      within(list).getByRole("link", { name: "Evening Prayer" }),
    ).toBeInTheDocument();
    expect(within(list).getByText("Walking in Faith")).toBeInTheDocument();
    expect(within(list).getByText("8:00 am")).toBeInTheDocument();
    expect(within(list).getByText("Scheduled")).toBeInTheDocument();
    expect(await screen.findByText("Across 2 sessions")).toBeInTheDocument();
  });

  it("asks for the sessions around the day and the numbers of the one day", async () => {
    open();
    await screen.findByRole("list", { name: "Sessions" });
    const calls = asked(fetchMock);
    const list = calls.find((c) =>
      c.url.pathname.endsWith("/attendance/sessions"),
    );
    expect(list?.url.searchParams.get("from")).toBe("2026-09-20");
    expect(list?.url.searchParams.get("to")).toBe("2026-11-01");
    const totals = calls.find((c) =>
      c.url.pathname.endsWith("/attendance/summary"),
    );
    expect([
      totals?.url.searchParams.get("from"),
      totals?.url.searchParams.get("to"),
    ]).toEqual(["2026-10-11", "2026-10-11"]);
  });

  it("offers the nearest day with a session when this one has none", async () => {
    open("/attendance?day=2026-10-14");
    expect(
      await screen.findByText("No sessions on this day"),
    ).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole("button", { name: "Go to Sun 18 Oct" }),
    );
    expect(
      await screen.findByRole("heading", { name: /Sunday, 18 October 2026/ }),
    ).toBeInTheDocument();
  });

  it("points a church with no session types at setting them up", async () => {
    sessions = [];
    labels = [];
    open();
    expect(
      await screen.findByText("Create your first session"),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Set up session types" }),
    ).toHaveAttribute("href", "/attendance/labels");
  });

  it("moves a week with the arrows and back with Today", async () => {
    // The fixtures are dated around 12 October 2026, so the clock is set there. Only Date is faked: timers stay real.
    vi.useFakeTimers({
      toFake: ["Date"],
      now: new Date("2026-10-12T10:00:00"),
    });
    open();
    await screen.findByRole("list", { name: "Sessions" });
    await userEvent.click(screen.getByRole("button", { name: "Next week" }));
    expect(
      await screen.findByRole("heading", { name: /Sunday, 18 October 2026/ }),
    ).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Today" }));
    expect(
      screen.queryByRole("heading", { name: /18 October 2026/ }),
    ).toBeNull();
  });

  it("creates a session on the day that is open", async () => {
    open("/attendance?day=2026-10-12");
    await userEvent.click(
      await screen.findByRole("button", { name: "New session" }),
    );
    const sheet = await screen.findByRole("dialog", { name: "New session" });
    await userEvent.click(
      within(sheet).getByRole("button", { name: "Create session" }),
    );
    expect(
      await within(sheet).findByText("Choose a session type"),
    ).toBeInTheDocument();
    await userEvent.click(
      within(sheet).getByRole("combobox", { name: "Session type" }),
    );
    await userEvent.click(
      await screen.findByRole("option", { name: "Sunday Service" }),
    );
    await userEvent.click(
      within(sheet).getByRole("button", { name: "Create session" }),
    );
    const post = await vi.waitFor(() => {
      const found = asked(fetchMock).find((c) => c.init?.method === "POST");
      if (!found) throw new Error("not yet");
      return found;
    });
    expect(JSON.parse(String(post.init?.body))).toMatchObject({
      sessionLabelId: 3,
      date: "2026-10-12",
    });
  });
});

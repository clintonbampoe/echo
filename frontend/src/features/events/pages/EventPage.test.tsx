import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { EventCheckIn } from "../api/checkIns";
import type { EventItem } from "../api/events";
import type { EventRegistration } from "../api/registrations";
import { EventPage } from "./EventPage";
import { anEvent } from "../testing/fixtures";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const retreat = anEvent({
  id: "e1",
  name: "Prayer Retreat",
  organizationId: "o1",
  organizationName: "Prayer Warriors",
  organizerId: "m7",
  organizerName: "Efua Quaye",
  startDate: "2020-10-03",
  endDate: "2020-10-03",
  startTime: "06:00:00",
  endTime: "18:00:00",
  location: "Aburi Mountain Lodge",
  capacity: 60,
  description: "A day of prayer.",
  createdAt: "2020-01-01T00:00:00Z",
  registrationCount: 3,
});
const reg = (
  id: string,
  memberId: string,
  memberName: string,
  registrationDate: string,
): EventRegistration => ({
  id,
  memberId,
  memberName,
  eventId: "e1",
  registrationDate,
});
const came = (
  id: string,
  memberId: string,
  memberName: string,
  checkInTime: string,
): EventCheckIn => ({ id, memberId, memberName, eventId: "e1", checkInTime });

const page = (data: unknown[]) => ({ data, hasMore: false, next: null });
const has = (name: string, text: string | null) =>
  !text || name.toLowerCase().includes(text.toLowerCase());

// What the server answers: who came, counted, without either list.
const rosterOf = (regs: EventRegistration[], checks: EventCheckIn[]) => {
  const signedUp = new Set(regs.map((r) => r.memberId));
  const walkIns = checks.filter((c) => !signedUp.has(c.memberId)).length;
  return {
    checkedIn: checks.length,
    walkIns,
    cameRegistered: checks.length - walkIns,
  };
};

// The registrations of the event, searched and narrowed by the server, each saying whether the person came.
const registrationRows = (
  regs: EventRegistration[],
  checks: EventCheckIn[],
  query: URLSearchParams,
) =>
  regs
    .map((r) => {
      const c = checks.find((x) => x.memberId === r.memberId);
      return {
        ...r,
        checkIn: c ? { id: c.id, checkInTime: c.checkInTime } : null,
      };
    })
    .filter(
      (r) =>
        has(r.memberName, query.get("name")) &&
        (query.get("status") === null ||
          (query.get("status") === "Came") === (r.checkIn !== null)),
    );

// The check-ins of the event, searched and narrowed by the server, each saying whether the person was a walk-in.
const checkInRows = (
  regs: EventRegistration[],
  checks: EventCheckIn[],
  query: URLSearchParams,
) =>
  checks
    .map((c) => {
      const r = regs.find((x) => x.memberId === c.memberId);
      return { ...c, walkIn: !r, registrationId: r?.id ?? null };
    })
    .filter(
      (c) =>
        has(c.memberName, query.get("name")) &&
        (query.get("kind") === null ||
          (query.get("kind") === "WalkIn") === c.walkIn),
    );

let event: EventItem;
let regs: EventRegistration[];
let checks: EventCheckIn[];
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  event = retreat;
  regs = [
    reg("r1", "m1", "Ama Boateng", "2020-09-12"),
    reg("r2", "m2", "Comfort Addo", "2020-09-14"),
    reg("r3", "m3", "Kofi Mensah", "2020-09-12"),
  ];
  checks = [
    came("c1", "m1", "Ama Boateng", "06:12:00"),
    came("c2", "m3", "Kofi Mensah", "06:05:00"),
    came("c3", "m9", "Grace Appiah", "06:55:00"),
  ];
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    if (init?.method === "PUT") return json(checks[0]);
    if (u.pathname.endsWith("/event-attendance/event/e1/summary"))
      return json(rosterOf(regs, checks));
    if (u.pathname.includes("/event-registrations/event/"))
      return json(page(registrationRows(regs, checks, u.searchParams)));
    if (u.pathname.includes("/event-attendance/event/"))
      return json(page(checkInRows(regs, checks, u.searchParams)));
    return json(event);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = () =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={["/events/e1"]}>
        <Routes>
          <Route path="/events/:id" element={<EventPage />} />
          <Route path="/events" element={<p>The events list</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );

// The page opens on Details; the roster is one click away.
async function openRegistrations() {
  open();
  await userEvent
    .setup()
    .click(await screen.findByRole("tab", { name: "Registrations" }));
}

const sent = (method: string) => {
  const call = fetchMock.mock.calls.find(
    ([, i]) => (i as RequestInit | undefined)?.method === method,
  ) as [string, RequestInit] | undefined;
  return {
    url: call?.[0] ?? "",
    body: call?.[1].body
      ? (JSON.parse(call[1].body as string) as Record<string, unknown>)
      : undefined,
  };
};

describe("event page", () => {
  it("shows the event, its numbers and who signed up with whether they came", async () => {
    await openRegistrations();
    expect(
      await screen.findByRole("heading", { name: "Prayer Retreat" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Sat 3 Oct · Aburi Mountain Lodge"),
    ).toBeInTheDocument();
    expect(
      await screen.findByText("67% of those registered"),
    ).toBeInTheDocument();
    const numbers = screen.getByRole("region", { name: "Event numbers" });
    expect(within(numbers).getByText("Show rate")).toBeInTheDocument();
    expect(within(numbers).getAllByText("1")).toHaveLength(1);
    const list = await screen.findByRole("list", { name: "Registrations" });
    const rows = within(list).getAllByRole("listitem");
    expect(rows).toHaveLength(3);
    expect(rows[0]).toHaveTextContent("Ama Boateng");
    expect(rows[0]).toHaveTextContent("Checked in 6:12 am");
    expect(rows[1]).toHaveTextContent("Comfort Addo");
    expect(rows[1]).toHaveTextContent("No-show");
  });

  it("shows who came on the check-ins tab, with walk-ins marked", async () => {
    const user = userEvent.setup();
    open();
    await user.click(await screen.findByRole("tab", { name: /Check-ins/ }));
    const list = await screen.findByRole("list", { name: "Check-ins" });
    const rows = within(list).getAllByRole("listitem");
    expect(rows).toHaveLength(3);
    expect(rows[2]).toHaveTextContent("Grace Appiah");
    expect(rows[2]).toHaveTextContent("Walk-in");
    expect(rows[0]).toHaveTextContent("Registered");
  });

  it("reads no list until its tab is opened, and then only that one", async () => {
    const user = userEvent.setup();
    open();
    expect(
      await screen.findByText("67% of those registered"),
    ).toBeInTheDocument();
    const asked = () =>
      fetchMock.mock.calls.map(
        ([url]) => new URL(url as string, "http://x").pathname,
      );
    // The numbers come from one count, not from reading who signed up and who came.
    expect(
      asked().some((p) => p.endsWith("/event-attendance/event/e1/summary")),
    ).toBe(true);
    expect(asked().some((p) => p.includes("/event-registrations/event/"))).toBe(
      false,
    );
    expect(
      asked().filter((p) => p.endsWith("/event-attendance/event/e1")),
    ).toHaveLength(0);

    await user.click(screen.getByRole("tab", { name: "Registrations" }));
    await screen.findByRole("list", { name: "Registrations" });
    expect(asked().some((p) => p.includes("/event-registrations/event/"))).toBe(
      true,
    );
    expect(
      asked().filter((p) => p.endsWith("/event-attendance/event/e1")),
    ).toHaveLength(0);
  });

  it("asks the server to narrow by status, instead of reading everyone", async () => {
    const user = userEvent.setup();
    await openRegistrations();
    await screen.findByRole("list", { name: "Registrations" });
    await user.click(screen.getByRole("button", { name: /Status/ }));
    await user.click(await screen.findByRole("option", { name: "No-show" }));
    await waitFor(() =>
      expect(
        within(
          screen.getByRole("list", { name: "Registrations" }),
        ).getAllByRole("listitem"),
      ).toHaveLength(1),
    );
    expect(screen.getByText("Comfort Addo")).toBeInTheDocument();
    const urls = fetchMock.mock.calls.map(([u]) => String(u));
    expect(urls.some((u) => u.includes("status=Missing"))).toBe(true);
  });

  it("narrows the registrations by name and by status", async () => {
    const user = userEvent.setup();
    await openRegistrations();
    await user.type(
      await screen.findByRole("searchbox", { name: "Search registrations" }),
      "comfort",
    );
    await waitFor(() =>
      expect(
        within(
          screen.getByRole("list", { name: "Registrations" }),
        ).getAllByRole("listitem"),
      ).toHaveLength(1),
    );
    expect(screen.getByText("Comfort Addo")).toBeInTheDocument();
  });

  it("marks the guests of an event still to come as not checked in, not as no-shows", async () => {
    event = { ...retreat, startDate: "2099-10-03", endDate: "2099-10-03" };
    checks = [];
    await openRegistrations();
    const list = await screen.findByRole("list", { name: "Registrations" });
    expect(within(list).getAllByText("Not checked in")).toHaveLength(3);
    expect(screen.queryByText("No-show")).toBeNull();
    expect(screen.getByText("Event has not started")).toBeInTheDocument();
  });

  it("opens a panel from a row, with the way out to the member page, and removes the registration", async () => {
    const user = userEvent.setup();
    await openRegistrations();
    await user.click(
      await screen.findByRole("button", { name: "Comfort Addo" }),
    );
    const panel = await screen.findByRole("dialog", { name: "Registration" });
    expect(
      within(panel).getByRole("link", { name: /View profile/ }),
    ).toHaveAttribute("href", "/members/m2?tab=events");
    expect(within(panel).getByText("14 Sep")).toBeInTheDocument();
    await user.click(
      within(panel).getByRole("button", { name: "Remove registration" }),
    );
    expect(
      await screen.findByText("Remove Comfort Addo's registration?"),
    ).toBeInTheDocument();
    await user.click(
      screen.getByRole("button", { name: "Remove registration" }),
    );
    await waitFor(() =>
      expect(sent("DELETE").url).toContain("/v1/event-registrations/r2"),
    );
  });

  it("calls a walk-in a check-in, and offers nothing to remove a registration with", async () => {
    const user = userEvent.setup();
    open();
    await user.click(await screen.findByRole("tab", { name: /Check-ins/ }));
    await user.click(
      await screen.findByRole("button", { name: "Grace Appiah" }),
    );
    const panel = await screen.findByRole("dialog", { name: "Check-in" });
    expect(
      within(panel).getByText("Came without registering"),
    ).toBeInTheDocument();
    expect(
      within(panel).queryByRole("button", { name: "Remove registration" }),
    ).toBeNull();
    expect(
      within(panel).getByRole("button", { name: "Delete check-in" }),
    ).toBeInTheDocument();
  });

  it("changes a check-in time", async () => {
    const user = userEvent.setup();
    open();
    await user.click(await screen.findByRole("tab", { name: /Check-ins/ }));
    await user.click(
      await screen.findByRole("button", { name: "Ama Boateng" }),
    );
    await user.click(
      within(
        await screen.findByRole("dialog", { name: "Registration" }),
      ).getByRole("button", { name: "Edit check-in time" }),
    );
    expect(
      await screen.findByRole("dialog", { name: "Edit check-in time" }),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() =>
      expect(sent("PUT").url).toContain("/v1/event-attendance/c1"),
    );
    expect(sent("PUT").body).toEqual({ checkInTime: "06:12:00" });
  });

  it("deletes the event and goes back to the list", async () => {
    const user = userEvent.setup();
    open();
    await user.click(
      await screen.findByRole("button", {
        name: "More actions for Prayer Retreat",
      }),
    );
    await user.click(screen.getByRole("menuitem", { name: "Delete" }));
    expect(
      await screen.findByText(/Its 3 registrations and 3 check-ins are kept/),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Delete event" }));
    expect(await screen.findByText("The events list")).toBeInTheDocument();
    expect(sent("DELETE").url).toContain("/v1/events/e1");
  });

  it("says when the event cannot be found", async () => {
    fetchMock.mockImplementation(async () =>
      json({ title: "Not found", status: 404 }, 404),
    );
    open();
    expect(
      await screen.findByText("We could not find this event"),
    ).toBeInTheDocument();
  });

  it("says when every spot is taken, and shows who hosts the event and what it is about under Details", async () => {
    const user = userEvent.setup();
    event = { ...retreat, capacity: 3 };
    open();
    expect(await screen.findByText("All 3 spots taken")).toBeInTheDocument();
    await user.click(screen.getByRole("tab", { name: "Details" }));
    const details = within(
      screen.getByRole("region", { name: "Event details" }),
    );
    expect(details.getByText("Efua Quaye")).toBeInTheDocument();
    expect(details.getByText("A day of prayer.")).toBeInTheDocument();
    expect(details.getByText("Aburi Mountain Lodge")).toBeInTheDocument();
  });

  it("opens the check-in panel from the Check in button, and registering from the menu", async () => {
    const user = userEvent.setup();
    open();
    await user.click(await screen.findByRole("button", { name: "Check in" }));
    const sheet = await screen.findByRole("dialog", { name: "Check in" });
    expect(
      within(sheet).getByRole("radio", { name: "Check in" }),
    ).toBeChecked();
    await user.click(within(sheet).getByRole("button", { name: "Done" }));
    await user.click(
      screen.getByRole("button", { name: "More actions for Prayer Retreat" }),
    );
    await user.click(
      screen.getByRole("menuitem", { name: "Register someone" }),
    );
    expect(
      within(await screen.findByRole("dialog", { name: "Register" })).getByRole(
        "radio",
        { name: "Register" },
      ),
    ).toBeChecked();
  });
});

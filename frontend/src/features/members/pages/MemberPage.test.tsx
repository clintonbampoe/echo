import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Route, Routes } from "react-router-dom";
import { json, pathOf, renderApp, signIn } from "@/test/kit";
import { aMember } from "../testing/fixtures";
import { MemberPage } from "./MemberPage";

const member = aMember();

const empty = { data: [], hasMore: false, next: null };

// Answers every request the profile makes: the member itself, then whatever else is given by address.
function server(extra: Record<string, unknown> = {}) {
  return async (url: string, init?: RequestInit) => {
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    const path = pathOf(url);
    if (path in extra) return json(extra[path]);
    if (path === "/v1/members/m1") return json(member);
    if (
      path === "/v1/tithes/summary/monthly" ||
      path === "/v1/attendance/labels"
    )
      return json([]);
    if (path.endsWith("/summary"))
      return json({ totalPresent: 0, completedSessions: 0, totalCollected: 0 });
    return json(empty);
  };
}

beforeEach(signIn);
afterEach(() => vi.unstubAllGlobals());

function show(
  handler: (url: string, init?: RequestInit) => Promise<Response>,
  start = "/members/m1",
) {
  vi.stubGlobal("fetch", vi.fn(handler));
  renderApp(
    <Routes>
      <Route path="/members/:id" element={<MemberPage />} />
      <Route path="/members" element={<p>The list</p>} />
    </Routes>,
    start,
  );
}

describe("MemberPage", () => {
  it("shows the member and their details", async () => {
    show(server());
    expect(
      await screen.findByRole("heading", { name: "Kwame Asante" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Greater Accra")).toBeInTheDocument();
    expect(screen.getByText("Married")).toBeInTheDocument();
    expect(screen.getByText(/14 Feb 1988 \(\d+\)/)).toBeInTheDocument();
  });

  it("puts what we know under the name, and no status chip for an active member", async () => {
    show(server());
    await screen.findByRole("heading", { name: "Kwame Asante" });
    expect(
      screen.getByText("024 412 7788 · Accra · Member since Mar 2019"),
    ).toBeInTheDocument();
    expect(screen.queryByText("Inactive")).toBeNull();
  });

  it("shows a dash for details we do not have", async () => {
    show(server());
    await screen.findByRole("heading", { name: "Kwame Asante" });
    expect(screen.getAllByText("–").length).toBeGreaterThan(0);
  });

  it("says so when the member does not exist", async () => {
    show(async () =>
      json({ errorCode: "NOT_FOUND", message: "Member not found" }, 404),
    );
    expect(
      await screen.findByText("We could not find this member"),
    ).toBeInTheDocument();
  });

  it("goes back to the list after a delete", async () => {
    const user = userEvent.setup();
    show(server());
    await user.click(
      await screen.findByRole("button", {
        name: "More actions for Kwame Asante",
      }),
    );
    await user.click(screen.getByRole("menuitem", { name: "Delete" }));
    const dialog = await screen.findByRole("alertdialog");
    expect(dialog).toHaveTextContent("He leaves your lists and totals.");
    await user.click(within(dialog).getByRole("button", { name: "Delete" }));
    expect(await screen.findByText("The list")).toBeInTheDocument();
  });
});

describe("the numbers and the tabs", () => {
  it("fills the four cards", async () => {
    show(
      server({
        "/v1/attendance/summary": { totalPresent: 38 },
        "/v1/attendance/sessions/summary": { completedSessions: 44 },
        "/v1/attendance": {
          data: [
            {
              id: "a1",
              sessionId: "s1",
              sessionDate: "2026-10-11",
              sessionName: null,
              sessionLabelName: "Sunday Service",
              checkedInAt: "2026-10-11T09:04:00",
            },
          ],
          hasMore: false,
          next: null,
        },
        "/v1/organization-members/member/m1": {
          data: [
            {
              id: "g1",
              organizationId: "o1",
              organizationName: "Choir",
              role: "Leader",
              joinedAt: "2020-01-05",
            },
          ],
          hasMore: false,
          next: null,
        },
      }),
    );
    expect(await screen.findByText("86")).toBeInTheDocument();
    expect(screen.getByText("38 of 44 sessions this year")).toBeInTheDocument();
    expect(screen.getAllByText("11 Oct").length).toBeGreaterThan(0);
  });

  it("opens on Details and switches tabs", async () => {
    const user = userEvent.setup();
    show(
      server({
        "/v1/tithes/member/m1": {
          data: [
            {
              id: "t1",
              amount: 800,
              forYear: 2026,
              forMonth: "October",
              paymentMethod: "MobileMoney",
              collectionDate: "2026-10-04",
            },
          ],
          hasMore: false,
          next: null,
        },
      }),
    );
    expect(await screen.findByRole("tab", { name: /Details/ })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    await user.click(screen.getByRole("tab", { name: /Tithes/ }));
    expect(await screen.findByText("October 2026")).toBeInTheDocument();
    expect(screen.getByText("GH₵ 800")).toBeInTheDocument();
    expect(screen.getByText("Mobile money")).toBeInTheDocument();
  });

  it("starts on the tab named in the address", async () => {
    show(server(), "/members/m1?tab=organizations");
    expect(
      await screen.findByRole("tab", { name: /Organizations/ }),
    ).toHaveAttribute("aria-selected", "true");
    expect(
      await screen.findByText("Not in any organization"),
    ).toBeInTheDocument();
  });

  it("says whether the member came to each event they signed up for", async () => {
    show(
      server({
        "/v1/event-registrations/member/m1": {
          data: [
            {
              id: "r1",
              eventId: "e1",
              eventName: "Easter Convention",
              eventStartDate: "2026-03-10",
              registrationDate: "2026-03-01",
            },
            {
              id: "r2",
              eventId: "e2",
              eventName: "Retreat",
              eventStartDate: "2026-01-20",
              registrationDate: "2026-01-05",
            },
            {
              id: "r3",
              eventId: "e3",
              eventName: "Crossover",
              eventStartDate: "2099-12-31",
              registrationDate: "2026-10-01",
            },
          ],
          hasMore: false,
          next: null,
        },
        "/v1/event-attendance/member/m1": {
          data: [
            {
              id: "c1",
              eventId: "e1",
              eventName: "Easter Convention",
              eventStartDate: "2026-03-10",
              checkInTime: "08:42:00",
            },
          ],
          hasMore: false,
          next: null,
        },
      }),
      "/members/m1?tab=events",
    );
    const easter = (
      await screen.findAllByText("Easter Convention")
    )[0]!.closest("li")!;
    expect(within(easter).getByText("Attended")).toBeInTheDocument();
    expect(within(easter).getByText("10 Mar 2026")).toBeInTheDocument();
    expect(
      within(screen.getByText("Retreat").closest("li")!).getByText(
        "Did not attend",
      ),
    ).toBeInTheDocument();
    expect(
      within(screen.getByText("Crossover").closest("li")!).getByText(
        "Upcoming",
      ),
    ).toBeInTheDocument();
    expect(screen.getByText("8:42 am")).toBeInTheDocument();
  });

  it("links each event to its own page", async () => {
    show(
      server({
        "/v1/event-registrations/member/m1": {
          data: [
            {
              id: "r1",
              eventId: "e1",
              eventName: "Easter Convention",
              eventStartDate: "2026-03-10",
              registrationDate: "2026-03-01",
            },
          ],
          hasMore: false,
          next: null,
        },
      }),
      "/members/m1?tab=events",
    );
    expect(
      await screen.findByRole("link", { name: "Easter Convention" }),
    ).toHaveAttribute("href", "/events/e1");
  });

  it("shows the organizations with a role, each linking to its page", async () => {
    show(
      server({
        "/v1/organization-members/member/m1": {
          data: [
            {
              id: "g1",
              organizationId: "o1",
              organizationName: "Choir",
              role: "Leader",
              joinedAt: "2020-01-05",
            },
          ],
          hasMore: false,
          next: null,
        },
      }),
      "/members/m1?tab=organizations",
    );
    expect(await screen.findByRole("link", { name: "Choir" })).toHaveAttribute(
      "href",
      "/organizations/o1",
    );
    expect(screen.getByText("Leader")).toBeInTheDocument();
  });

  it("draws the recent sessions as a strip and links the rows to their sessions", async () => {
    show(
      server({
        "/v1/attendance": {
          data: [
            {
              id: "a1",
              sessionId: "s1",
              sessionDate: "2026-10-11",
              sessionName: null,
              sessionLabelName: "Sunday Service",
              checkedInAt: "2026-10-11T09:04:00",
            },
          ],
          hasMore: false,
          next: null,
        },
        "/v1/attendance/sessions": {
          data: [
            {
              id: "s1",
              date: "2026-10-11",
              name: null,
              sessionLabelName: "Sunday Service",
            },
            {
              id: "s0",
              date: "2026-10-04",
              name: null,
              sessionLabelName: "Sunday Service",
            },
          ],
          hasMore: false,
          next: null,
        },
        "/v1/attendance/labels": [{ id: 1, name: "Sunday Service" }],
      }),
      "/members/m1?tab=attendance",
    );
    expect(await screen.findByText("1 of 2 attended")).toBeInTheDocument();
    expect(
      screen.getByRole("link", {
        name: "Sunday Service, 11 Oct 2026: attended",
      }),
    ).toHaveAttribute("href", "/attendance/sessions/s1");
    expect(
      screen.getByRole("link", { name: "Sunday Service, 04 Oct 2026: missed" }),
    ).toBeInTheDocument();
  });

  it("asks for the whole history of events and projects, not just this year", async () => {
    show(server(), "/members/m1?tab=responsibilities");
    await screen.findByText("Not managing a project");
    const calls = (
      fetch as unknown as { mock: { calls: [string][] } }
    ).mock.calls.map(([u]) => u);
    const events = calls.find(
      (u) => u.includes("/v1/events") && u.includes("organizerId=m1"),
    );
    const projects = calls.find(
      (u) => u.includes("/v1/projects") && u.includes("managerId=m1"),
    );
    expect(events).toContain("from=2000-01-01");
    expect(projects).toContain("from=2000-01-01");
  });
});

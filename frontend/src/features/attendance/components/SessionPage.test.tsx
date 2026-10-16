import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  asked,
  checkIn,
  json,
  page,
  renderAt,
  route,
  session,
  signIn,
  summary,
} from "../attendanceTestKit";
import { SessionPage } from "./SessionPage";

const kofi = { id: "m2", name: "Kofi Mensah", phoneNumber: "024 412 0098" };

type Route = [
  matches: (path: string, method: string) => boolean,
  reply: (init?: RequestInit) => Response,
];

// What the server answers, first match wins. The list is read when asked, so a test can change the data.
const routes: Route[] = [
  [(p) => p.endsWith("/attendance/sessions/s1"), () => json(current)],
  [(p) => p.endsWith("/attendance/sessions"), () => page([])],
  [(p) => p.endsWith("/attendance/summary"), () => json(summary)],
  [(p) => p.endsWith("/members/search"), () => json([kofi])],
  [
    (p, m) => p.endsWith("/v1/visitors") && m === "POST",
    () => json({ id: "v9", name: "Yaw Frimpong" }, 201),
  ],
  [(p) => p.endsWith("/v1/visitors"), () => page([])],
  [
    (p, m) => p.endsWith("/v1/attendance") && m === "POST",
    (init) =>
      json(
        checkIn({
          id: "c9",
          personId: JSON.parse(String(init?.body)).personId,
        }),
        201,
      ),
  ],
  [(_, m) => m === "DELETE", () => new Response(null, { status: 204 })],
  [(_, m) => m === "PUT", () => json(current)],
  [(p) => p.endsWith("/v1/attendance"), () => page(checkIns)],
];

function answer(u: URL, init?: RequestInit) {
  const found = routes.find(([matches]) =>
    matches(u.pathname, init?.method ?? "GET"),
  );
  return found ? found[1](init) : json({}, 404);
}

let current: unknown;
let checkIns: unknown[];
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  current = session({ status: "Scheduled" });
  checkIns = [
    checkIn(),
    checkIn({
      id: "c2",
      personId: "v1",
      personName: "Esi Owusu",
      personKind: "Visitor",
      checkedInAt: "2026-10-11T09:20:00",
    }),
  ];
  signIn();
  fetchMock = vi.fn(async (url: string, init?: RequestInit) =>
    answer(new URL(url, "http://x"), init),
  );
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = () =>
  renderAt(
    "/attendance/sessions/s1",
    route("/attendance/sessions/:id", <SessionPage />),
  );
const sent = (method: string, ending: string) =>
  asked(fetchMock).filter(
    (c) => c.init?.method === method && c.url.pathname.endsWith(ending),
  );
const body = (call?: { init?: RequestInit }) =>
  JSON.parse(String(call?.init?.body));

describe("session page", () => {
  it("shows the session, the numbers and who is marked present", async () => {
    open();
    expect(
      await screen.findByRole("heading", { name: /Sunday Service/ }),
    ).toBeInTheDocument();
    expect(screen.getByText(/Topic: Walking in Faith\./)).toBeInTheDocument();
    expect(
      screen.getByText(/Created by Ama Serwaa Boateng\./),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /Sunday, 11 October/ }),
    ).toHaveAttribute("href", "/attendance?day=2026-10-11");
    const list = await screen.findByRole("list", { name: "Check-ins" });
    expect(
      within(list).getByRole("link", { name: "Kwame Asante" }),
    ).toHaveAttribute("href", "/members/m1?tab=attendance");
    expect(
      within(list).getByRole("link", { name: "Esi Owusu" }),
    ).toHaveAttribute("href", "/visitors/v1");
    expect(within(list).getByText("9:04 am")).toBeInTheDocument();
    expect(within(list).getByText("9:20 am")).toBeInTheDocument();
    expect(
      await screen.findByText("Marked for this session"),
    ).toBeInTheDocument();
  });

  it("marks a member present with a tap, and says so", async () => {
    open();
    await userEvent.type(
      await screen.findByRole("searchbox", { name: "Search members" }),
      "ko",
    );
    await userEvent.click(
      await screen.findByRole("button", { name: "Mark Kofi Mensah present" }),
    );
    await waitFor(() => expect(sent("POST", "/v1/attendance")).toHaveLength(1));
    expect(body(sent("POST", "/v1/attendance")[0])).toEqual({
      sessionId: "s1",
      personId: "m2",
    });
    expect(
      await screen.findByText("1 marked present just now"),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("searchbox", { name: "Search members" }),
    ).toHaveValue("");
  });

  it("marks the first match when Enter is pressed", async () => {
    open();
    await userEvent.type(
      await screen.findByRole("searchbox", { name: "Search members" }),
      "ko",
    );
    await screen.findByRole("button", { name: "Mark Kofi Mensah present" });
    await userEvent.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "{Enter}",
    );
    await waitFor(() => expect(sent("POST", "/v1/attendance")).toHaveLength(1));
    expect(body(sent("POST", "/v1/attendance")[0]).personId).toBe("m2");
  });

  it("undoes the last person marked", async () => {
    open();
    await userEvent.type(
      await screen.findByRole("searchbox", { name: "Search members" }),
      "ko",
    );
    await userEvent.click(
      await screen.findByRole("button", { name: "Mark Kofi Mensah present" }),
    );
    await userEvent.click(
      await screen.findByRole("button", { name: "Undo Kofi Mensah" }),
    );
    await waitFor(() =>
      expect(sent("DELETE", "/v1/attendance/c9")).toHaveLength(1),
    );
    expect(
      await screen.findByText("No one marked just now"),
    ).toBeInTheDocument();
  });

  it("says a person is already present instead of marking them twice", async () => {
    checkIns = [checkIn({ personId: "m2", personName: "Kofi Mensah" })];
    open();
    await screen.findByRole("list", { name: "Check-ins" });
    await userEvent.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "ko",
    );
    expect(await screen.findByText("Already present")).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Mark Kofi Mensah present" }),
    ).toBeNull();
  });

  it("adds a visitor nobody has met and marks them present", async () => {
    open();
    await userEvent.click(
      await screen.findByRole("radio", { name: "Visitor" }),
    );
    await userEvent.type(
      screen.getByRole("searchbox", { name: "Search visitors" }),
      "Yaw Frimpong",
    );
    expect(await screen.findByText(/No visitor named/)).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole("button", { name: "Add as new visitor" }),
    );
    await waitFor(() => expect(sent("POST", "/v1/attendance")).toHaveLength(1));
    expect(body(sent("POST", "/v1/visitors")[0])).toEqual({
      firstName: "Yaw",
      lastName: "Frimpong",
    });
    expect(body(sent("POST", "/v1/attendance")[0]).personId).toBe("v9");
  });

  it("needs both names before adding a visitor", async () => {
    open();
    await userEvent.click(
      await screen.findByRole("radio", { name: "Visitor" }),
    );
    await userEvent.type(
      screen.getByRole("searchbox", { name: "Search visitors" }),
      "Yaw",
    );
    expect(
      await screen.findByText(
        "Type the first and last name to add a new visitor.",
      ),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Add as new visitor" }),
    ).toBeNull();
  });

  it("searches the check-ins by name", async () => {
    open();
    await userEvent.type(
      await screen.findByRole("searchbox", { name: "Search check-ins" }),
      "esi",
    );
    await waitFor(() =>
      expect(
        asked(fetchMock).some(
          (c) =>
            c.url.searchParams.get("name") === "esi" &&
            c.url.searchParams.get("sessionId") === "s1",
        ),
      ).toBe(true),
    );
  });

  it("removes a check-in after asking", async () => {
    open();
    await userEvent.click(
      await screen.findByRole("button", { name: "Actions for Esi Owusu" }),
    );
    await userEvent.click(
      await screen.findByRole("menuitem", { name: "Remove from session" }),
    );
    expect(
      await screen.findByText("Remove Esi Owusu from this session?"),
    ).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole("button", { name: "Remove record" }),
    );
    await waitFor(() =>
      expect(sent("DELETE", "/v1/attendance/c2")).toHaveLength(1),
    );
  });

  it("changes a check-in time to that time on the session's day", async () => {
    open();
    await userEvent.click(
      await screen.findByRole("button", { name: "Actions for Kwame Asante" }),
    );
    await userEvent.click(
      await screen.findByRole("menuitem", { name: "Edit check-in time" }),
    );
    const sheet = await screen.findByRole("dialog", {
      name: "Edit check-in time",
    });
    const time = within(sheet).getByLabelText("Checked in at");
    expect(time).toHaveValue("09:04");
    await userEvent.clear(time);
    await userEvent.type(time, "09:30");
    await userEvent.click(
      within(sheet).getByRole("button", { name: "Save changes" }),
    );
    await waitFor(() =>
      expect(sent("PUT", "/v1/attendance/c1")).toHaveLength(1),
    );
    const at = new Date(body(sent("PUT", "/v1/attendance/c1")[0]).checkedInAt);
    expect([at.getDate(), at.getHours(), at.getMinutes()]).toEqual([11, 9, 30]);
  });

  it("marks a scheduled session completed after asking", async () => {
    open();
    await userEvent.click(
      await screen.findByRole("button", { name: "Mark completed" }),
    );
    expect(
      await screen.findByText(/2 people marked present will count/),
    ).toBeInTheDocument();
    await userEvent.click(
      within(screen.getByRole("alertdialog")).getByRole("button", {
        name: "Mark completed",
      }),
    );
    await waitFor(() =>
      expect(sent("PUT", "/v1/attendance/sessions/s1")).toHaveLength(1),
    );
    expect(body(sent("PUT", "/v1/attendance/sessions/s1")[0])).toEqual({
      status: "Completed",
    });
  });

  it("does not offer to complete or cancel a session that is already completed", async () => {
    current = session({ status: "Completed" });
    open();
    await screen.findByRole("heading", { name: /Sunday Service/ });
    expect(screen.queryByRole("button", { name: "Mark completed" })).toBeNull();
    expect(screen.queryByRole("button", { name: "Cancel session" })).toBeNull();
  });

  it("says when the session is gone", async () => {
    fetchMock.mockImplementation(async () =>
      json({ errorCode: "NOT_FOUND", detail: "Gone" }, 404),
    );
    open();
    expect(
      await screen.findByText("We could not find this session"),
    ).toBeInTheDocument();
  });
});

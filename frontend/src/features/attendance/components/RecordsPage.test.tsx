import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  asked,
  checkIn,
  json,
  page,
  renderAt,
  route,
  service,
  session,
  signIn,
  summary,
} from "../attendanceTestKit";
import { RecordsPage } from "./RecordsPage";

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  signIn();
  fetchMock = vi.fn(async (url: string) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/attendance/labels")) return json([service]);
    if (u.pathname.endsWith("/attendance/summary")) return json(summary);
    if (u.pathname.endsWith("/attendance/sessions"))
      return page([
        session(),
        session({
          id: "s2",
          name: "Evening Prayer",
          status: "Cancelled",
          attendanceCount: 0,
        }),
      ]);
    if (u.pathname.endsWith("/attendance"))
      return page([
        checkIn(),
        checkIn({
          id: "c2",
          personId: "v1",
          personName: "Efua Mensah",
          personKind: "Visitor",
        }),
      ]);
    return json({}, 404);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = (address = "/attendance/records") =>
  renderAt(
    address,
    <>
      {route("/attendance/records", <RecordsPage />)}
      {route("/attendance/records/people", <RecordsPage />)}
    </>,
  );

describe("attendance records", () => {
  it("lists the sessions held this month, with how many came, and numbers for the period", async () => {
    open();
    const table = await screen.findByRole("list", { name: "Sessions" });
    expect(
      within(table).getByRole("link", { name: "Sunday Service" }),
    ).toHaveAttribute("href", "/attendance/sessions/s1");
    expect(within(table).getByText("Cancelled")).toBeInTheDocument();
    expect(await screen.findByText("In this period")).toBeInTheDocument();
    const call = asked(fetchMock).find((c) =>
      c.url.pathname.endsWith("/attendance/sessions"),
    );
    expect(call?.url.searchParams.get("from")).toMatch(/^\d{4}-\d{2}-01$/);
    expect(call?.url.searchParams.get("to")).toBeTruthy();
  });

  it("draws attendance month by month above the list", async () => {
    open();
    expect(await screen.findByText("Attendance by month")).toBeInTheDocument();
    expect(
      screen.getByRole("list", { name: /Attendance for each of the last/ }),
    ).toBeInTheDocument();
  });

  it("has Day and Records tabs, with Records open", async () => {
    open();
    expect(
      await screen.findByRole("tab", { name: "Records", selected: true }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("tab", { name: "Day", selected: false }),
    ).toBeInTheDocument();
  });

  it("shows who was marked present on the People view, and a row opens their check-in panel", async () => {
    open("/attendance/records/people");
    const list = await screen.findByRole("list", { name: "Check-ins" });
    await userEvent.click(
      within(list).getByRole("button", { name: "Kwame Asante" }),
    );
    const panel = await screen.findByRole("dialog", {
      name: "Attendance record",
    });
    expect(
      within(panel).getByRole("link", { name: /View profile/ }),
    ).toHaveAttribute("href", "/members/m1?tab=attendance");
    await userEvent.click(within(panel).getByRole("button", { name: "Close" }));
    await userEvent.click(
      within(list).getByRole("button", { name: "Efua Mensah" }),
    );
    expect(
      within(
        await screen.findByRole("dialog", { name: "Attendance record" }),
      ).getByRole("link", { name: /View profile/ }),
    ).toHaveAttribute("href", "/visitors/v1");
  });

  it("switches between Sessions and People and keeps the filters", async () => {
    open("/attendance/records?type=3");
    await screen.findByRole("list", { name: "Sessions" });
    await userEvent.click(screen.getByRole("radio", { name: "People" }));
    expect(
      await screen.findByRole("list", { name: "Check-ins" }),
    ).toBeInTheDocument();
    const call = asked(fetchMock)
      .filter(
        (c) =>
          c.url.pathname.endsWith("/attendance") &&
          c.url.searchParams.has("from"),
      )
      .at(-1);
    expect(call?.url.searchParams.get("sessionLabelId")).toBe("3");
  });

  it("asks for the custom days that are in the address", async () => {
    open("/attendance/records?window=custom&from=2026-03-01&to=2026-04-01");
    await screen.findByRole("list", { name: "Sessions" });
    const call = asked(fetchMock).find((c) =>
      c.url.pathname.endsWith("/attendance/sessions"),
    );
    expect(call?.url.searchParams.get("from")).toBe("2026-03-01");
    expect(call?.url.searchParams.get("to")).toBe("2026-04-01");
    expect(
      screen.getByRole("button", { name: "From: 01 Mar 2026" }),
    ).toBeInTheDocument();
  });
});

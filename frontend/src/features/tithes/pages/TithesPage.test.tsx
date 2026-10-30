import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Tithe } from "../api/tithes";
import { TithesPage } from "./TithesPage";
import { aTithe } from "../testing/fixtures";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const kofi = aTithe({
  id: "t1",
  memberId: "m1",
  memberName: "Kofi Mensah",
  amount: 500,
  forYear: 2026,
  forMonth: "October",
  paymentMethod: "Cash",
  collectionDate: "2026-10-11",
  createdAt: "",
});
const ama = aTithe({
  id: "t2",
  memberId: "m2",
  memberName: "Ama Boateng",
  amount: 150,
  forYear: 2026,
  forMonth: "September",
  paymentMethod: "MobileMoney",
  collectionDate: "2026-10-10",
  description: "Whole family",
  createdAt: "",
});
const summary = {
  totalCollected: 650,
  uniqueTithers: 2,
  mostUsedPaymentMethod: "Cash",
  averagePerMember: 325,
  byMethod: [
    { method: "Cash", count: 1, total: 500 },
    { method: "MobileMoney", count: 1, total: 150 },
  ],
};
const months = Array.from({ length: 12 }, (_, i) => ({
  year: 2025 + Math.floor((i + 10) / 12),
  month: ((i + 10) % 12) + 1,
  totalCollected: i === 7 ? 900 : 100,
  uniqueTithers: 3,
}));

// What the server answers to "record a tithe": done, or "this member already has one for the month".
const recorded = (refuse: boolean) =>
  refuse
    ? json(
        {
          errorCode: "TITHE_DUPLICATE",
          detail:
            "Kofi Mensah already has a tithe for October 2026. Update that tithe instead.",
        },
        409,
      )
    : json({ ...kofi, id: "new" }, 201);

// A list asked for one member (the "already has a tithe" check) answers only for them.
const forMember = (all: Tithe[], u: URL) =>
  all.filter(
    (r) =>
      !u.searchParams.has("memberId") ||
      r.memberId === u.searchParams.get("memberId"),
  );

// One tithe by its id, and a delete.
const byId = (u: URL, init?: RequestInit) => {
  if (init?.method === "DELETE") return new Response(null, { status: 204 });
  return /\/tithes\/t1$/.test(u.pathname) ? json(kofi) : null;
};

let rows: Tithe[];
let refuseRecording: boolean;
const needsMonths = (u: URL) =>
  u.searchParams.has("months")
    ? null
    : new Response(JSON.stringify({ errorCode: "BAD_REQUEST" }), {
        status: 400,
      });

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  rows = [kofi, ama];
  refuseRecording = false;
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/tithes/summary/monthly"))
      return needsMonths(u) ?? json(months);
    if (u.pathname.endsWith("/tithes/summary")) return json(summary);
    if (u.pathname.endsWith("/members/summary"))
      return json({ activeMembers: 118 });
    if (u.pathname.endsWith("/members/search"))
      return json([
        { id: "m1", name: "Kofi Mensah", phoneNumber: "024 412 0098" },
      ]);
    if (u.pathname.endsWith("/tithes") && init?.method === "POST")
      return recorded(refuseRecording);
    const other = byId(u, init);
    if (other) return other;
    return json({ data: forMember(rows, u), hasMore: false, next: null });
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = (address = "/tithes") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <TithesPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );

const asked = () =>
  fetchMock.mock.calls.map((c) => new URL((c as [string])[0], "http://x"));

describe("tithes page", () => {
  it("shows the numbers, the chart and every tithe", async () => {
    open();
    expect(
      await screen.findByRole("button", { name: "Kofi Mensah" }),
    ).toBeInTheDocument();
    expect(
      await screen.findByText("Out of 118 active members"),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("list", { name: "Collected per month" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Collected this week", { selector: "p" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        (_, el) =>
          el?.tagName === "P" &&
          /Highest: GH₵ 900 in/.test(el.textContent ?? ""),
      ),
    ).toBeInTheDocument();
  });

  it("builds how it was paid from the one summary, not one request for each way of paying", async () => {
    open();
    const card = await screen.findByRole("list", {
      name: "Tithes by way of paying",
    });
    expect(within(card).getByText("Cash")).toBeInTheDocument();
    expect(within(card).getByText("Mobile money")).toBeInTheDocument();
    const summaries = fetchMock.mock.calls
      .map(([u]) => new URL(String(u), "http://x"))
      .filter((u) => u.pathname.endsWith("/tithes/summary"));
    expect(summaries).toHaveLength(1);
    expect(summaries[0]?.searchParams.has("paymentMethod")).toBe(false);
  });

  it("asks for this week unless told otherwise", async () => {
    open();
    await screen.findByRole("button", { name: "Kofi Mensah" });
    const list = asked().find(
      (u) => u.pathname.endsWith("/tithes") && u.searchParams.has("from"),
    );
    expect(list?.searchParams.get("from")).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    expect(list?.searchParams.get("to")).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });

  it("narrows the list to a month when a chart bar is clicked, and widens the dates", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("button", { name: "Kofi Mensah" });
    const bar = within(
      screen.getByRole("list", { name: "Collected per month" }),
    ).getAllByRole("button")[0];
    if (!bar) throw new Error("no bars");
    await user.click(bar);
    expect(
      await screen.findByRole("button", { name: /^Tithe for: / }),
    ).toBeInTheDocument();
    const last = asked()
      .filter((u) => u.pathname.endsWith("/tithes"))
      .pop();
    expect(last?.searchParams.get("year")).toBeTruthy();
    expect(last?.searchParams.get("month")).toBeTruthy();
    expect(last?.searchParams.get("from")).toBe("2000-01-01");
  });

  it("opens the tithe a link points to", async () => {
    open("/tithes?tithe=t1");
    const sheet = await screen.findByRole("dialog");
    expect(within(sheet).getByText("Edit tithe")).toBeInTheDocument();
    expect(within(sheet).getByText("GH₵ 500")).toBeInTheDocument();
  });

  it("says so, with an icon card, when nothing was collected", async () => {
    rows = [];
    open();
    expect(
      await screen.findByText("No tithes collected this week"),
    ).toBeInTheDocument();
  });

  it("records a tithe and starts the next one on the same day and method, with no member", async () => {
    const user = userEvent.setup();
    rows = [ama];
    open();
    await screen.findByRole("button", { name: "Ama Boateng" });
    await user.click(screen.getByRole("button", { name: /Record tithe/ }));
    const sheet = await screen.findByRole("dialog");
    await user.type(
      within(sheet).getByRole("searchbox", { name: "Search members" }),
      "Kofi",
    );
    await user.click(
      await within(sheet).findByRole("button", { name: /Kofi Mensah/ }),
    );
    await user.type(within(sheet).getByLabelText("Amount"), "500");
    await user.click(
      within(sheet).getByRole("button", { name: "Save and add another" }),
    );
    expect(
      await within(sheet).findByText("Recorded in this sitting"),
    ).toBeInTheDocument();
    expect(
      within(sheet).getByText(/Recorded GH₵ 500 for Kofi Mensah/),
    ).toBeInTheDocument();
    expect(within(sheet).queryByText("Change")).not.toBeInTheDocument();
    expect(
      within(sheet).getByRole("searchbox", { name: "Search members" }),
    ).toHaveValue("");
    expect(within(sheet).getByLabelText("Amount")).toHaveValue("");
    const post = fetchMock.mock.calls.find(
      (c) => (c[1] as RequestInit | undefined)?.method === "POST",
    );
    expect(JSON.parse((post?.[1] as RequestInit).body as string)).toMatchObject(
      {
        memberId: "m1",
        amount: 500,
        forMonth: expect.any(String),
        paymentMethod: "Cash",
      },
    );
  });

  it("stops a second tithe for the month, and opens the first one to top up", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("button", { name: "Kofi Mensah" });
    await user.click(screen.getByRole("button", { name: /Record tithe/ }));
    const sheet = await screen.findByRole("dialog");
    expect(within(sheet).queryByText(/already has/)).not.toBeInTheDocument();
    await user.type(
      within(sheet).getByRole("searchbox", { name: "Search members" }),
      "Kofi",
    );
    await user.click(
      await within(sheet).findByRole("button", { name: /Kofi Mensah/ }),
    );
    expect(
      await within(sheet).findByText(/Kofi Mensah already has a tithe for/),
    ).toBeInTheDocument();
    expect(
      within(sheet).getByRole("button", { name: "Save tithe" }),
    ).toBeDisabled();
    expect(
      within(sheet).getByRole("button", { name: "Save and add another" }),
    ).toBeDisabled();
    const asked = fetchMock.mock.calls
      .map((c) => new URL((c as [string])[0], "http://x"))
      .filter((u) => u.searchParams.get("memberId") === "m1");
    expect(
      asked.some(
        (u) => u.searchParams.get("year") && u.searchParams.get("month"),
      ),
    ).toBe(true);
    await user.click(
      within(sheet).getByRole("button", { name: "Update that tithe" }),
    );
    const edit = await screen.findByRole("dialog");
    expect(
      within(edit).getByText(
        "Enter the new total for the month. The day is today.",
      ),
    ).toBeInTheDocument();
    expect(
      within(edit).getByRole("button", { name: "Save changes" }),
    ).toBeInTheDocument();
  });

  it("says so in plain words when the server refuses a duplicate that got past the check", async () => {
    const user = userEvent.setup();
    rows = [];
    refuseRecording = true;
    open();
    await user.click(
      await screen.findByRole("button", { name: /Record tithe/ }),
    );
    const sheet = await screen.findByRole("dialog");
    await user.type(
      within(sheet).getByRole("searchbox", { name: "Search members" }),
      "Kofi",
    );
    await user.click(
      await within(sheet).findByRole("button", { name: /Kofi Mensah/ }),
    );
    await user.type(within(sheet).getByLabelText("Amount"), "500");
    await user.click(within(sheet).getByRole("button", { name: "Save tithe" }));
    expect(
      await within(sheet).findByText("We could not record this tithe"),
    ).toBeInTheDocument();
    expect(
      within(sheet).getByText(
        "Kofi Mensah already has a tithe for October 2026. Update that tithe instead.",
      ),
    ).toBeInTheDocument();
  });

  it("opens a tithe to read, with its member, and without a way to change them", async () => {
    const user = userEvent.setup();
    open();
    await user.click(
      await screen.findByRole("button", { name: "Kofi Mensah" }),
    );
    const sheet = await screen.findByRole("dialog");
    expect(within(sheet).getByText("GH₵ 500")).toBeInTheDocument();
    expect(
      within(sheet).getByRole("link", { name: "View profile" }),
    ).toHaveAttribute("href", "/members/m1?tab=tithes");
    expect(
      within(sheet).getByRole("button", { name: "Edit tithe" }),
    ).toBeInTheDocument();
    expect(
      within(sheet).getByRole("button", { name: "Delete tithe" }),
    ).toBeInTheDocument();
    expect(within(sheet).queryByRole("searchbox")).not.toBeInTheDocument();
    expect(within(sheet).queryByLabelText("Amount")).not.toBeInTheDocument();
  });

  it("saves changes to a tithe without sending a member", async () => {
    const user = userEvent.setup();
    open();
    await user.click(
      await screen.findByRole("button", { name: "Kofi Mensah" }),
    );
    await user.click(
      within(await screen.findByRole("dialog")).getByRole("button", {
        name: "Edit tithe",
      }),
    );
    const sheet = await screen.findByRole("dialog");
    const amount = within(sheet).getByLabelText("Amount");
    await user.clear(amount);
    await user.type(amount, "650");
    await user.click(
      within(sheet).getByRole("button", { name: "Save changes" }),
    );
    const put = fetchMock.mock.calls.find(
      (c) => (c[1] as RequestInit | undefined)?.method === "PUT",
    );
    const body = JSON.parse((put?.[1] as RequestInit).body as string) as Record<
      string,
      unknown
    >;
    expect(body).toMatchObject({ amount: 650 });
    expect(body).not.toHaveProperty("memberId");
  });

  it("asks before deleting and then deletes", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("button", { name: "Kofi Mensah" });
    await user.click(screen.getByRole("button", { name: "Kofi Mensah" }));
    await user.click(
      await screen.findByRole("button", { name: "Delete tithe" }),
    );
    expect(screen.getByText("Delete this tithe?")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Delete tithe" }));
    expect(
      fetchMock.mock.calls.some(
        (c) => (c[1] as RequestInit | undefined)?.method === "DELETE",
      ),
    ).toBe(true);
  });

  it("deletes from the menu on its row, asking first", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("button", { name: "Kofi Mensah" });
    await user.click(
      screen.getByRole("button", { name: "Actions for Kofi Mensah" }),
    );
    await user.click(await screen.findByRole("menuitem", { name: "Delete" }));
    expect(await screen.findByText("Delete this tithe?")).toBeInTheDocument();
  });
});

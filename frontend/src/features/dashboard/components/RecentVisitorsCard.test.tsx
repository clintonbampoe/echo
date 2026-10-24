import { screen, within } from "@testing-library/react";
import { asked, json, renderCard, signIn } from "../dashboardTestKit";
import { RecentVisitorsCard } from "./RecentVisitorsCard";

// A page the server sends, in name order: the newest are not first, so the card has to sort what it got.
const page = [
  { id: "u1", name: "Kwame Boateng", createdAt: "2026-10-11T09:20:00Z" },
  { id: "u2", name: "Esi Mensah", createdAt: "2026-10-19T08:10:00Z" },
  { id: "u3", name: "Ade Boateng", createdAt: "2026-03-02T10:00:00Z" },
  { id: "u4", name: "Abena Osei", createdAt: "2026-10-17T17:45:00Z" },
];

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path, query } = asked(url);
      if (broken || path !== "/v1/visitors")
        return json({ title: "Server error", status: 500 }, 500);
      expect(query.get("pageSize")).toBe("50");
      return json({ data: page });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

it("names the three newest, and when each was added", async () => {
  renderCard(<RecentVisitorsCard today="2026-10-20" />);
  // Wait for a row instead of the list: the list is there while it is still loading, holding ghosts.
  await screen.findByText("Esi Mensah");
  const rows = screen.getByRole("list");
  expect(rows.textContent).toContain("Esi Mensah");
  expect(rows.textContent).toContain("Added yesterday");
  const inOrder = within(rows).getAllByText(
    /Esi Mensah|Abena Osei|Kwame Boateng/,
  );
  expect(inOrder.map((e) => e.textContent)).toEqual([
    "Esi Mensah",
    "Abena Osei",
    "Kwame Boateng",
  ]);
});

it("leads to every visitor's records", async () => {
  renderCard(<RecentVisitorsCard today="2026-10-20" />);
  await screen.findByText("Esi Mensah");
  expect(screen.getByRole("link", { name: /All visitors/ })).toHaveAttribute(
    "href",
    "/visitors",
  );
  expect(
    screen.getAllByRole("link", { name: /Esi Mensah/ })[0],
  ).toHaveAttribute("href", "/visitors/u2");
});

it("says when it has nothing to show", async () => {
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path } = asked(url);
      if (broken || path !== "/v1/visitors")
        return json({ title: "Server error", status: 500 }, 500);
      return json({ data: [] });
    }),
  );
  renderCard(<RecentVisitorsCard today="2026-10-20" />);
  expect(
    await screen.findByText("No visitors in the last 6 months"),
  ).toBeInTheDocument();
});

it("says so when the visitors could not load, with another try", async () => {
  broken = true;
  renderCard(<RecentVisitorsCard today="2026-10-20" />);
  expect(await screen.findByText("Could not load")).toBeInTheDocument();
  expect(screen.getByRole("button", { name: "Try again" })).toBeInTheDocument();
});

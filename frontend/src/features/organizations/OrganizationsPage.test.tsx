import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Organization, OrganizationMember } from "./api";
import { OrganizationPage } from "./OrganizationPage";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const choir: Organization = {
  id: "o1",
  name: "Church Choir",
  description: "Leads worship on Sundays.",
  createdAt: "2016-02-03T09:00:00Z",
  memberCount: 61,
};
const person = (
  id: string,
  name: string,
  role: OrganizationMember["role"],
  joinedAt: string,
): OrganizationMember => ({
  id: `om-${id}`,
  memberId: id,
  memberName: name,
  organizationId: "o1",
  organizationName: "Church Choir",
  role,
  joinedAt,
  createdAt: joinedAt,
});
const people = [
  person("m1", "Kofi Mensah", "Leader", "2016-01-05"),
  person("m2", "Comfort Addo", "Secretary", "2018-03-11"),
  person("m3", "Kwame Asante", "Member", "2020-01-12"),
];
const event = {
  id: "e1",
  name: "Christmas Carols",
  startDate: "2099-12-20",
  startTime: "17:00:00",
  location: "Main Auditorium",
  organizerId: "m2",
  organizerName: "Comfort Addo",
};

const page = (data: unknown[]) => json({ data, hasMore: false, next: null });

// Adding, changing and searching: what the Members tab's menus and the Add member sheet talk to.
function changes(path: string, method?: string) {
  if (method === "PUT") return json(people[1]);
  if (method === "POST") return json(people[0]);
  if (path === "/v1/members/search")
    return json([
      { id: "m9", name: "Kojo Antwi", phoneNumber: "020 551 7733" },
    ]);
  return null;
}

function server(status = 200) {
  return async (url: string, init?: RequestInit) => {
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    const u = new URL(url, "http://x");
    const path = u.pathname.replace("/api", "");
    const change = changes(path, init?.method);
    if (change) return change;
    if (path === "/v1/organizations/o1")
      return status === 200
        ? json(choir)
        : json({ title: "Not found" }, status);
    if (path === "/v1/organization-members/organization/o1") {
      const role = u.searchParams.get("role");
      const name = u.searchParams.get("name")?.toLowerCase();
      return page(
        people.filter(
          (p) =>
            (!role || p.role === role) &&
            (!name || p.memberName.toLowerCase().includes(name)),
        ),
      );
    }
    if (path === "/v1/events") return page([event]);
    return page([]);
  };
}

beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
});
afterEach(() => vi.unstubAllGlobals());

function show(handler: (url: string, init?: RequestInit) => Promise<Response>) {
  const fetchMock = vi.fn(handler);
  vi.stubGlobal("fetch", fetchMock);
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={["/organizations/o1"]}>
        <Routes>
          <Route path="/organizations/:id" element={<OrganizationPage />} />
          <Route path="/organizations" element={<p>The list</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
  return fetchMock;
}

describe("organization page", () => {
  it("shows the group, who leads it and its members", async () => {
    show(server());
    expect(
      await screen.findByRole("heading", { name: "Church Choir" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Leads worship on Sundays.")).toBeInTheDocument();
    expect(
      await screen.findByRole("button", { name: /^Comfort Addo$/ }),
    ).toBeInTheDocument();
    expect(screen.getByText("Jan 2016")).toBeInTheDocument();
    expect(screen.getByText("Secretary")).toBeInTheDocument();
    const strip = screen.getByRole("region", { name: "Organization numbers" });
    expect(within(strip).getByText("61")).toBeInTheDocument();
    expect(await within(strip).findByText("Kofi Mensah")).toBeInTheDocument();
    expect(within(strip).getByText("03 Feb 2016")).toBeInTheDocument();
  });

  it("asks only for the people with the role chosen in the filter", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await screen.findByRole("button", { name: /^Kwame Asante$/ });
    await user.click(screen.getByRole("button", { name: /Role/ }));
    await user.click(await screen.findByRole("option", { name: "Secretary" }));
    await waitFor(() =>
      expect(
        screen.queryByRole("button", { name: /^Kwame Asante$/ }),
      ).toBeNull(),
    );
    expect(
      screen.getByRole("button", { name: /^Comfort Addo$/ }),
    ).toBeInTheDocument();
    expect(
      fetchMock.mock.calls.some(
        ([u]) => new URL(u as string).searchParams.get("role") === "Secretary",
      ),
    ).toBe(true);
  });

  it("searches the group’s members by name", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await screen.findByRole("button", { name: /^Kwame Asante$/ });
    await user.type(
      screen.getByRole("searchbox", { name: "Search this group" }),
      "comf",
    );
    expect(
      await screen.findByRole("button", { name: /^Comfort Addo$/ }),
    ).toBeInTheDocument();
    await waitFor(() =>
      expect(
        screen.queryByRole("button", { name: /^Kwame Asante$/ }),
      ).toBeNull(),
    );
    await waitFor(() =>
      expect(
        fetchMock.mock.calls.some(
          ([u]) =>
            new URL(u as string, "http://x").searchParams.get("name") ===
            "comf",
        ),
      ).toBe(true),
    );
  });

  it("empties the search box as well as the role when the filters are cleared", async () => {
    const user = userEvent.setup();
    show(server());
    await screen.findByRole("button", { name: /^Kwame Asante$/ });
    await user.type(
      screen.getByRole("searchbox", { name: "Search this group" }),
      "zzz",
    );
    await user.click(
      await screen.findByRole("button", { name: /Clear filters/ }),
    );
    expect(
      screen.getByRole("searchbox", { name: "Search this group" }),
    ).toHaveValue("");
    expect(
      await screen.findByRole("button", { name: /^Kwame Asante$/ }),
    ).toBeInTheDocument();
  });

  it("lists the events the group is hosting on the Events tab, each linking to its page", async () => {
    const user = userEvent.setup();
    show(server());
    await user.click(await screen.findByRole("tab", { name: /Events/ }));
    expect(
      await screen.findByRole("link", { name: "Christmas Carols" }),
    ).toHaveAttribute("href", "/events/e1");
    expect(screen.getByText("Main Auditorium")).toBeInTheDocument();
    expect(screen.getByText("1 upcoming event")).toBeInTheDocument();
  });

  it("deletes the group and goes back to the list", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await user.click(await screen.findByRole("button", { name: "Delete" }));
    await user.click(
      await screen.findByRole("button", { name: "Delete organization" }),
    );
    expect(await screen.findByText("The list")).toBeInTheDocument();
    expect(
      fetchMock.mock.calls.some(
        ([u, i]) =>
          (i as RequestInit | undefined)?.method === "DELETE" &&
          String(u).includes("/v1/organizations/o1"),
      ),
    ).toBe(true);
  });

  it("opens the edit sheet filled with what the group has", async () => {
    const user = userEvent.setup();
    show(server());
    await user.click(await screen.findByRole("button", { name: "Edit" }));
    expect(await screen.findByLabelText("Name")).toHaveValue("Church Choir");
  });

  it("says when the group is gone, and links back to the list", async () => {
    show(server(404));
    expect(
      await screen.findByText("We could not find this organization"),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Organizations" })).toHaveAttribute(
      "href",
      "/organizations",
    );
  });

  it("changes a member’s role from their ⋯ menu", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await user.click(
      await screen.findByRole("button", { name: "Actions for Kwame Asante" }),
    );
    expect(
      screen.getByRole("menuitemradio", { name: "Member" }),
    ).toHaveAttribute("aria-checked", "true");
    await user.click(screen.getByRole("menuitemradio", { name: "Leader" }));
    await waitFor(() =>
      expect(
        fetchMock.mock.calls.some(
          ([u, i]) =>
            (i as RequestInit | undefined)?.method === "PUT" &&
            String(u).includes("/v1/organization-members/om-m3"),
        ),
      ).toBe(true),
    );
    const put = fetchMock.mock.calls.find(
      ([, i]) => (i as RequestInit | undefined)?.method === "PUT",
    ) as [string, RequestInit];
    expect(JSON.parse(put[1].body as string)).toEqual({ role: "Leader" });
  });

  it("does not call the server when the role chosen is the one they already have", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await user.click(
      await screen.findByRole("button", { name: "Actions for Kwame Asante" }),
    );
    await user.click(screen.getByRole("menuitemradio", { name: "Member" }));
    expect(
      fetchMock.mock.calls.some(
        ([, i]) => (i as RequestInit | undefined)?.method === "PUT",
      ),
    ).toBe(false);
  });

  it("tells the person when a role change fails", async () => {
    const user = userEvent.setup();
    const handler = server();
    show(async (url, init) =>
      init?.method === "PUT"
        ? json({ title: "Boom" }, 500)
        : handler(url, init),
    );
    await user.click(
      await screen.findByRole("button", { name: "Actions for Kwame Asante" }),
    );
    await user.click(screen.getByRole("menuitemradio", { name: "Leader" }));
    expect(
      await screen.findByText("We could not change that role"),
    ).toBeInTheDocument();
  });

  it("removes a member from the group after asking", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await user.click(
      await screen.findByRole("button", { name: "Actions for Kwame Asante" }),
    );
    await user.click(
      screen.getByRole("menuitem", { name: "Remove from group" }),
    );
    expect(
      await screen.findByText("Remove Kwame Asante from Church Choir?"),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Remove from group" }));
    await waitFor(() =>
      expect(
        fetchMock.mock.calls.some(
          ([u, i]) =>
            (i as RequestInit | undefined)?.method === "DELETE" &&
            String(u).includes("/v1/organization-members/om-m3"),
        ),
      ).toBe(true),
    );
  });

  it("opens Add member and adds the person chosen from the search", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await user.click(await screen.findByRole("button", { name: "Add member" }));
    await user.type(
      await screen.findByRole("searchbox", { name: "Search members" }),
      "ko",
    );
    await user.click(await screen.findByRole("button", { name: /Kojo Antwi/ }));
    await user.click(
      within(screen.getByRole("dialog")).getByRole("button", {
        name: "Add member",
      }),
    );
    await waitFor(() =>
      expect(
        fetchMock.mock.calls.some(
          ([, i]) => (i as RequestInit | undefined)?.method === "POST",
        ),
      ).toBe(true),
    );
  });

  it("opens the membership panel from a name, with a link to the profile as the only way out", async () => {
    const user = userEvent.setup();
    show(server());
    await user.click(
      await screen.findByRole("button", { name: /^Comfort Addo$/ }),
    );
    const panel = await screen.findByRole("dialog", { name: "Membership" });
    expect(within(panel).getByText("In Church Choir")).toBeInTheDocument();
    expect(
      within(panel).getByRole("link", { name: "View profile" }),
    ).toHaveAttribute("href", "/members/m2?tab=organizations");
    expect(
      within(panel).getByRole("radio", { name: "Secretary" }),
    ).toBeChecked();
  });

  it("saves a role changed in the membership panel", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await user.click(
      await screen.findByRole("button", { name: /^Kwame Asante$/ }),
    );
    const panel = await screen.findByRole("dialog", { name: "Membership" });
    await user.click(within(panel).getByRole("radio", { name: "Leader" }));
    await user.click(
      within(panel).getByRole("button", { name: "Save changes" }),
    );
    await waitFor(() =>
      expect(
        fetchMock.mock.calls.some(
          ([u, i]) =>
            (i as RequestInit | undefined)?.method === "PUT" &&
            String(u).includes("/v1/organization-members/om-m3"),
        ),
      ).toBe(true),
    );
    const put = fetchMock.mock.calls.find(
      ([, i]) => (i as RequestInit | undefined)?.method === "PUT",
    ) as [string, RequestInit];
    expect(JSON.parse(put[1].body as string)).toEqual({
      role: "Leader",
      joinedAt: "2020-01-12",
    });
    await waitFor(() =>
      expect(screen.queryByRole("dialog", { name: "Membership" })).toBeNull(),
    );
  });

  it("closes the panel and asks before removing when Remove from group is chosen in it", async () => {
    const user = userEvent.setup();
    show(server());
    await user.click(
      await screen.findByRole("button", { name: /^Kwame Asante$/ }),
    );
    await user.click(
      within(
        await screen.findByRole("dialog", { name: "Membership" }),
      ).getByRole("button", { name: "Remove from group" }),
    );
    expect(
      await screen.findByText("Remove Kwame Asante from Church Choir?"),
    ).toBeInTheDocument();
    expect(screen.queryByRole("dialog", { name: "Membership" })).toBeNull();
  });
});

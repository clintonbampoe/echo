import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  json,
  server,
  showMembers,
  signedIn,
} from "./testing/organizationPage";

beforeEach(signedIn);
afterEach(() => vi.unstubAllGlobals());

describe("organization page members", () => {
  it("asks only for the people with the role chosen in the filter", async () => {
    const user = userEvent.setup();
    const fetchMock = await showMembers(server());
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
    const fetchMock = await showMembers(server());
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
    await showMembers(server());
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

  it("changes a member’s role from their ⋯ menu", async () => {
    const user = userEvent.setup();
    const fetchMock = await showMembers(server());
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
    const fetchMock = await showMembers(server());
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
    await showMembers(async (url, init) =>
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
    const fetchMock = await showMembers(server());
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
    const fetchMock = await showMembers(server());
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
    await showMembers(server());
    await user.click(
      await screen.findByRole("button", { name: /^Comfort Addo$/ }),
    );
    const panel = await screen.findByRole("dialog", { name: "Membership" });
    expect(within(panel).getByText("In Church Choir")).toBeInTheDocument();
    expect(
      within(panel).getByRole("link", { name: "View profile" }),
    ).toHaveAttribute("href", "/members/m2?tab=organizations");
    expect(within(panel).getAllByText("Secretary").length).toBeGreaterThan(0);
    expect(
      within(panel).getByRole("button", { name: "Edit membership" }),
    ).toBeInTheDocument();
    expect(within(panel).queryByRole("radio")).toBeNull();
  });

  it("saves a role changed in the membership panel", async () => {
    const user = userEvent.setup();
    const fetchMock = await showMembers(server());
    await user.click(
      await screen.findByRole("button", { name: /^Kwame Asante$/ }),
    );
    await user.click(
      within(
        await screen.findByRole("dialog", { name: "Membership" }),
      ).getByRole("button", { name: "Edit membership" }),
    );
    const sheet = await screen.findByRole("dialog", {
      name: "Edit membership",
    });
    await user.click(within(sheet).getByRole("radio", { name: "Leader" }));
    await user.click(
      within(sheet).getByRole("button", { name: "Save changes" }),
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
      expect(
        screen.queryByRole("dialog", { name: "Edit membership" }),
      ).toBeNull(),
    );
  });

  it("closes the panel and asks before removing when Remove from group is chosen in it", async () => {
    const user = userEvent.setup();
    await showMembers(server());
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

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Organization, OrganizationSummary } from "./api";
import { OrganizationsPage } from "./OrganizationsPage";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const choir: Organization = {
  id: "o1",
  name: "Church Choir",
  description: "Leads worship on Sundays.",
  createdAt: "2016-02-03T09:00:00Z",
  memberCount: 61,
};
const media: Organization = {
  id: "o2",
  name: "Media Team",
  createdAt: "2020-05-01T09:00:00Z",
  memberCount: 1,
};
const summary: OrganizationSummary = {
  totalOrganizations: 14,
  totalMembers: 612,
  averageMembersPerOrganization: 43.7,
  largestOrganization: "Youth Ministry",
};

let rows: Organization[];
beforeEach(() => {
  rows = [choir, media];
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
      return u.pathname.endsWith("/summary")
        ? json(summary)
        : json({ data: rows, hasMore: false, next: null });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const open = (address = "/organizations") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <OrganizationsPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );

describe("organizations page", () => {
  it("shows the numbers and a card for each organization, linking to its page", async () => {
    open();
    const card = await screen.findByRole("link", { name: /Church Choir/ });
    expect(card).toHaveAttribute("href", "/organizations/o1");
    expect(card).toHaveTextContent("61 members");
    expect(screen.getByRole("link", { name: /Media Team/ })).toHaveTextContent(
      "1 member",
    );
    expect(await screen.findByText("612")).toBeInTheDocument();
    expect(screen.getByText("44")).toBeInTheDocument();
    expect(screen.getByText("Youth Ministry")).toBeInTheDocument();
  });

  it("invites the first organization when there are none yet, and hides the numbers and filters", async () => {
    rows = [];
    open();
    expect(
      await screen.findByText("Create your first organization"),
    ).toBeInTheDocument();
    expect(screen.queryByRole("searchbox")).toBeNull();
    expect(screen.queryByText("Memberships")).toBeNull();
  });

  it("says nothing matches, rather than inviting a first one, when a filter is on", async () => {
    rows = [];
    open("/organizations?window=year");
    expect(
      await screen.findByText("No organizations match"),
    ).toBeInTheDocument();
    expect(screen.queryByText("Create your first organization")).toBeNull();
  });
});

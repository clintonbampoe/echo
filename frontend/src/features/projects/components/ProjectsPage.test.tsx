import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Project, ProjectCategory, ProjectSummary } from "../api";
import { ProjectsPage } from "./ProjectsPage";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const base = { description: null, createdAt: "2026-01-01T00:00:00Z" };
const rows: Project[] = [
  {
    ...base,
    id: "p1",
    name: "Church roof renovation",
    categoryId: 1,
    categoryName: "Building",
    managerId: "m1",
    managerName: "Ama Boateng",
    status: "AtRisk",
    targetAmount: 80000,
    raisedAmount: 48200,
    startDate: "2026-03-01",
    endDate: "2026-12-31",
  },
  {
    ...base,
    id: "p2",
    name: "Youth camp 2026",
    categoryId: 2,
    categoryName: "Outreach",
    managerId: "m2",
    managerName: "Kofi Mensah",
    status: "OnTrack",
    targetAmount: 12000,
    raisedAmount: 9100,
    startDate: "2026-08-01",
    endDate: "2026-12-31",
  },
  {
    ...base,
    id: "p3",
    name: "Radio ministry",
    categoryId: 2,
    categoryName: "Outreach",
    managerId: "m2",
    managerName: "Kofi Mensah",
    status: "Planning",
    targetAmount: 5000,
    raisedAmount: 0,
    startDate: "2026-11-01",
    endDate: null,
  },
];
const categories: ProjectCategory[] = [
  { id: 1, name: "Building", projectCount: 1 },
  { id: 2, name: "Outreach", projectCount: 2 },
];
const summary: ProjectSummary = {
  totalProjects: 3,
  totalRaised: 57300,
  totalTarget: 97000,
  atRiskCount: 1,
};

let list: Project[];
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  vi.useFakeTimers({ toFake: ["Date"], now: new Date("2026-10-19T10:00:00") });
  list = rows;
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/project-categories")) return json(categories);
    if (u.pathname.endsWith("/projects/summary")) return json(summary);
    const status = u.searchParams.get("status");
    const shown = list.filter((p) => !status || p.status === status);
    return json({ data: shown, hasMore: false, next: null });
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

const open = (address = "/projects") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <Routes>
          <Route path="/projects" element={<ProjectsPage />} />
          <Route
            path="/projects/categories"
            element={<p>The categories page</p>}
          />
          <Route path="/projects/:id" element={<p>The project page</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );

const asked = (path: string) =>
  fetchMock.mock.calls
    .map(([u]) => new URL(u as string, "http://x"))
    .filter((u) => u.pathname.endsWith(path));

describe("projects page", () => {
  it("shows the numbers and every project with its category, status, raised, percent and dates", async () => {
    open();
    const table = await screen.findByRole("list", { name: "Projects" });
    const roof = within(table)
      .getByText("Church roof renovation")
      .closest("li") as HTMLElement;
    expect(within(roof).getByText("Building")).toBeInTheDocument();
    expect(within(roof).getByText("At risk")).toBeInTheDocument();
    expect(within(roof).getByText("GH₵ 48,200")).toBeInTheDocument();
    expect(within(roof).getByText("of GH₵ 80,000")).toBeInTheDocument();
    expect(within(roof).getByText(/60%/)).toBeInTheDocument();
    const strip = screen.getByRole("region", { name: "Projects summary" });
    expect(await within(strip).findByText("57,300")).toBeInTheDocument();
    expect(within(strip).getByText("97,000")).toBeInTheDocument();
    expect(within(strip).getByText("This year")).toBeInTheDocument();
  });

  it("asks for the whole of this year by default", async () => {
    open();
    await screen.findByRole("list", { name: "Projects" });
    // The first request is the page's own; one more asks for all time to find the managers.
    const call = asked("/v1/projects").find(
      (u) => u.searchParams.get("from") !== "2000-01-01",
    );
    expect(call?.searchParams.get("from")).toBe("2026-01-01");
    expect(call?.searchParams.get("to")).toBe("2026-12-31");
  });

  it("narrows by the status in the address and asks the server for it", async () => {
    open("/projects?status=AtRisk");
    const table = await screen.findByRole("list", { name: "Projects" });
    expect(
      within(table).getByText("Church roof renovation"),
    ).toBeInTheDocument();
    expect(
      within(table).queryByText("Youth camp 2026"),
    ).not.toBeInTheDocument();
    expect(
      asked("/v1/projects").some(
        (u) => u.searchParams.get("status") === "AtRisk",
      ),
    ).toBe(true);
  });

  it("opens a project from its row", async () => {
    const user = userEvent.setup();
    open();
    await user.click(
      await screen.findByRole("link", { name: /Youth camp 2026/ }),
    );
    expect(await screen.findByText("The project page")).toBeInTheDocument();
  });

  it("invites the first project when there are none at all, and points to wider dates otherwise", async () => {
    list = [];
    open("/projects?window=all");
    expect(
      await screen.findByText("Start your first project"),
    ).toBeInTheDocument();
  });

  it("says nothing matches when filters are narrowing an empty list", async () => {
    list = [];
    open("/projects?status=Missed");
    expect(await screen.findByText("No projects match")).toBeInTheDocument();
  });

  it("says there is nothing this year without calling it the first project", async () => {
    list = [];
    open();
    expect(
      await screen.findByText("No projects in this period"),
    ).toBeInTheDocument();
  });

  it("leads to the categories page from the header, and from the form", async () => {
    const user = userEvent.setup();
    open();
    expect(
      await screen.findByRole("link", { name: "Categories" }),
    ).toHaveAttribute("href", "/projects/categories");
    await user.click(screen.getByRole("button", { name: /new project/i }));
    expect(
      await screen.findByRole("link", { name: "Manage categories" }),
    ).toHaveAttribute("href", "/projects/categories");
  });
});

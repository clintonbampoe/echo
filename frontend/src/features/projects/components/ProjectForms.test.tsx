import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import { pickDate } from "@/test/pickers";
import type { Project } from "../api";
import { DeleteProjectDialog } from "./DeleteProjectDialog";
import { EditProjectSheet } from "./EditProjectSheet";
import { ProjectSheet } from "./ProjectSheet";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const roof: Project = {
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
  description: "Fix the roof.",
  createdAt: "2026-01-01T00:00:00Z",
};
const categories = [
  { id: 1, name: "Building", projectCount: 1 },
  { id: 2, name: "Outreach", projectCount: 0 },
];

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/project-categories")) return json(categories);
    if (u.pathname.endsWith("/members/search"))
      return json([
        { id: "m1", name: "Ama Boateng", phoneNumber: "0244000111" },
      ]);
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    return json(roof);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const wrap = (ui: React.ReactElement) =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter>{ui}</MemoryRouter>
    </QueryClientProvider>,
  );

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

describe("new project sheet", () => {
  it("says what is missing and sends nothing until it is there", async () => {
    const user = userEvent.setup();
    wrap(<ProjectSheet open onClose={() => {}} />);
    await user.click(screen.getByRole("button", { name: "Create project" }));
    expect(await screen.findByText("Enter a project name")).toBeInTheDocument();
    expect(screen.getAllByText("Choose a category")).toHaveLength(2);
    expect(screen.getByText("Choose a manager")).toBeInTheDocument();
    expect(screen.getByText("Enter the start date")).toBeInTheDocument();
    expect(screen.getByText("Fix 5 fields to continue")).toBeInTheDocument();
    expect(sent("POST").url).toBe("");
  });

  it("creates the project with what was filled in, then closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<ProjectSheet open onClose={onClose} />);
    await user.type(
      screen.getByLabelText("Project name"),
      "Church roof renovation",
    );
    await user.click(screen.getByRole("combobox", { name: /Category/ }));
    await user.click(await screen.findByRole("option", { name: "Building" }));
    await user.type(screen.getByLabelText(/Target amount/), "80,000");
    await pickDate(user, "Start date", "2026-03-01");
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "am",
    );
    await user.click(
      await screen.findByRole("button", { name: /Ama Boateng/ }),
    );
    await user.click(screen.getByRole("button", { name: "Create project" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(sent("POST").body).toEqual({
      categoryId: 1,
      managerId: "m1",
      name: "Church roof renovation",
      targetAmount: 80000,
      status: "Planning",
      startDate: "2026-03-01",
    });
  });

  it("stays open and says so when the server cannot save it", async () => {
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      const u = new URL(url, "http://x");
      if (u.pathname.endsWith("/project-categories")) return json(categories);
      if (u.pathname.endsWith("/members/search"))
        return json([
          { id: "m1", name: "Ama Boateng", phoneNumber: "0244000111" },
        ]);
      return init?.method === "POST"
        ? json({ title: "Boom", status: 500 }, 500)
        : json(roof);
    });
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<ProjectSheet open onClose={onClose} />);
    await user.type(screen.getByLabelText("Project name"), "Roof");
    await user.click(screen.getByRole("combobox", { name: /Category/ }));
    await user.click(await screen.findByRole("option", { name: "Building" }));
    await user.type(screen.getByLabelText(/Target amount/), "5000");
    await pickDate(user, "Start date", "2026-03-01");
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "am",
    );
    await user.click(
      await screen.findByRole("button", { name: /Ama Boateng/ }),
    );
    await user.click(screen.getByRole("button", { name: "Create project" }));
    expect(
      await screen.findByText("We could not save this project"),
    ).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });
});

describe("edit project sheet", () => {
  it("starts with what the project has and saves a change", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<EditProjectSheet project={roof} onClose={onClose} />);
    expect(screen.getByLabelText("Project name")).toHaveValue(
      "Church roof renovation",
    );
    expect(screen.getByLabelText(/Target amount/)).toHaveValue("80,000.00");
    expect(screen.getByText("Ama Boateng")).toBeInTheDocument();
    await user.clear(screen.getByLabelText("Project name"));
    await user.type(screen.getByLabelText("Project name"), "Roof and ceiling");
    await user.clear(screen.getByLabelText(/Description/));
    expect(screen.getByText("Unsaved changes")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const put = sent("PUT");
    expect(put.url).toContain("/v1/projects/p1");
    expect(put.body).toMatchObject({
      name: "Roof and ceiling",
      description: "",
      targetAmount: 80000,
      status: "AtRisk",
      endDate: "2026-12-31",
    });
  });

  it("does not let a set end date be taken away", async () => {
    const user = userEvent.setup();
    wrap(<EditProjectSheet project={roof} onClose={() => {}} />);
    await user.click(screen.getByRole("button", { name: /^End date/ }));
    await user.click(
      within(screen.getByRole("dialog", { name: "End date" })).getByRole(
        "button",
        { name: "Clear" },
      ),
    );
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    expect(
      await screen.findByText("An end date can be changed, not removed"),
    ).toBeInTheDocument();
    expect(sent("PUT").url).toBe("");
  });
});

describe("delete project dialog", () => {
  it("says what is kept, with the count when it knows it", () => {
    wrap(
      <DeleteProjectDialog
        project={roof}
        contributions={21}
        onClose={() => {}}
      />,
    );
    expect(
      screen.getByText("Delete Church roof renovation?"),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/Its 21 contributions are kept in history/),
    ).toBeInTheDocument();
  });

  it("keeps the project when asked to", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<DeleteProjectDialog project={roof} onClose={onClose} />);
    expect(
      screen.getByText(/Its contributions are kept in history/),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Keep project" }));
    expect(onClose).toHaveBeenCalled();
    expect(sent("DELETE").url).toBe("");
  });

  it("deletes, closes and tells the page", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    const onDeleted = vi.fn();
    wrap(
      <DeleteProjectDialog
        project={roof}
        onClose={onClose}
        onDeleted={onDeleted}
      />,
    );
    await user.click(screen.getByRole("button", { name: "Delete project" }));
    await waitFor(() => expect(onDeleted).toHaveBeenCalled());
    expect(sent("DELETE").url).toContain("/v1/projects/p1");
    expect(onClose).toHaveBeenCalled();
  });
});

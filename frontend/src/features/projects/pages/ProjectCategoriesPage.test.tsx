import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { ProjectCategory } from "../api/categories";
import { ProjectCategoriesPage } from "./ProjectCategoriesPage";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

let categories: ProjectCategory[];
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  categories = [
    { id: 1, name: "Building", projectCount: 3 },
    { id: 2, name: "Outreach", projectCount: 1 },
    { id: 3, name: "Missions", projectCount: 0 },
  ];
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (init?.method === "POST" && String(url).endsWith("/reassign"))
      return json({ moved: 3, sourceDeleted: true });
    if (init?.method === "POST")
      return json({
        id: 9,
        projectCount: 0,
        ...JSON.parse(init.body as string),
      });
    if (init?.method === "PUT")
      return json({ ...categories[0], ...JSON.parse(init.body as string) });
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    return json(categories);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = () =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={["/projects/categories"]}>
        <Routes>
          <Route path="/projects" element={<p>Projects</p>} />
          <Route
            path="/projects/categories"
            element={<ProjectCategoriesPage />}
          />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );

const sent = (method: string) =>
  fetchMock.mock.calls.filter(
    ([, i]) => (i as RequestInit | undefined)?.method === method,
  );
const body = (method: string) =>
  JSON.parse((sent(method)[0]![1] as RequestInit).body as string);

const menuFor = async (name: string, item: string) => {
  const user = userEvent.setup();
  open();
  await screen.findByText(name);
  await user.click(
    screen.getByRole("button", {
      name: new RegExp(`actions for ${name}`, "i"),
    }),
  );
  await user.click(await screen.findByRole("menuitem", { name: item }));
  return user;
};

describe("project categories page", () => {
  it("lists every category with how many projects use it", async () => {
    open();
    expect(await screen.findByText("Building")).toBeInTheDocument();
    expect(screen.getByText("3 projects")).toBeInTheDocument();
    expect(screen.getByText("1 project")).toBeInTheDocument();
    expect(screen.getByText("0 projects")).toBeInTheDocument();
  });

  it("invites the first category when there are none", async () => {
    categories = [];
    open();
    expect(await screen.findByText("No categories yet")).toBeInTheDocument();
  });

  it("creates a category from its name alone", async () => {
    const user = userEvent.setup();
    open();
    await user.click(
      await screen.findByRole("button", { name: /new category/i }),
    );
    await user.type(screen.getByLabelText("Name"), " Welfare ");
    await user.click(screen.getByRole("button", { name: "Create category" }));
    await waitFor(() => expect(sent("POST")).toHaveLength(1));
    expect(body("POST")).toEqual({ name: "Welfare" });
  });

  it("asks for a name before saving", async () => {
    const user = userEvent.setup();
    open();
    await user.click(
      await screen.findByRole("button", { name: /new category/i }),
    );
    await user.click(screen.getByRole("button", { name: "Create category" }));
    expect(await screen.findByText("Enter a name")).toBeInTheDocument();
    expect(sent("POST")).toHaveLength(0);
  });

  it("renames a category", async () => {
    const user = await menuFor("Building", "Edit category");
    const name = await screen.findByLabelText("Name");
    await user.clear(name);
    await user.type(name, "Construction");
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(sent("PUT")).toHaveLength(1));
    expect(body("PUT")).toEqual({ name: "Construction" });
  });

  it("deletes a category nobody uses", async () => {
    const user = await menuFor("Missions", "Delete category");
    const dialog = await screen.findByRole("alertdialog");
    await user.click(within(dialog).getByRole("button", { name: /^delete/i }));
    await waitFor(() => expect(sent("DELETE")).toHaveLength(1));
  });

  it("will not delete a category in use, and moves its projects to another and deletes it", async () => {
    const user = await menuFor("Building", "Delete category");
    expect(
      await screen.findByText("Building is still in use"),
    ).toBeInTheDocument();
    expect(sent("DELETE")).toHaveLength(0);
    await user.click(screen.getByRole("button", { name: "Move 3 projects" }));
    const confirm = await screen.findByRole("button", {
      name: "Move and delete",
    });
    expect(confirm).toBeDisabled();
    await user.click(screen.getByRole("combobox", { name: "Move to" }));
    expect(
      screen.queryByRole("option", { name: "Building" }),
    ).not.toBeInTheDocument();
    await user.click(await screen.findByRole("option", { name: "Outreach" }));
    await user.click(screen.getByRole("button", { name: "Move and delete" }));
    await waitFor(() => expect(sent("POST")).toHaveLength(1));
    expect(body("POST")).toEqual({ targetCategoryId: 2, deleteSource: true });
  });

  it("says to create another category first when there is none to move to", async () => {
    categories = [{ id: 1, name: "Building", projectCount: 3 }];
    const user = await menuFor("Building", "Delete category");
    await user.click(
      await screen.findByRole("button", { name: "Move 3 projects" }),
    );
    expect(
      await screen.findByText(/no other category to move them to/i),
    ).toBeInTheDocument();
  });
});

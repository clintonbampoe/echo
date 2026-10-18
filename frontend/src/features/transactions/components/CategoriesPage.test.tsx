import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Category } from "../api";
import { CategoriesPage } from "./CategoriesPage";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const categories: Category[] = [
  { id: 1, name: "Offerings", categoryType: "Income", transactionCount: 38 },
  { id: 2, name: "Utilities", categoryType: "Expense", transactionCount: 14 },
  { id: 3, name: "Donations", categoryType: "Income", transactionCount: 0 },
];

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (_url: string, init?: RequestInit) => {
    if (init?.method === "POST" && String(_url).endsWith("/reassign"))
      return json({ moved: 14, sourceDeleted: true });
    if (init?.method === "POST")
      return json({
        id: 9,
        ...JSON.parse(init.body as string),
        transactionCount: 0,
      });
    if (init?.method === "PUT")
      return json({ ...categories[0], ...JSON.parse(init.body as string) });
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    return json(categories);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = (address = "/finance/categories") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <Routes>
          <Route path="/finance" element={<p>Finance</p>} />
          <Route path="/finance/categories" element={<CategoriesPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );

const sent = (method: string) =>
  fetchMock.mock.calls.filter(
    ([, i]) => (i as RequestInit | undefined)?.method === method,
  );

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

describe("categories page", () => {
  it("lists every category with its type and how many transactions use it", async () => {
    open();
    expect(await screen.findByText("Offerings")).toBeInTheDocument();
    expect(screen.getByText("38 transactions")).toBeInTheDocument();
    expect(screen.getByText("0 transactions")).toBeInTheDocument();
  });

  it("shows only one type when asked in the address", async () => {
    open("/finance/categories?type=Expense");
    expect(await screen.findByText("Utilities")).toBeInTheDocument();
    expect(screen.queryByText("Offerings")).not.toBeInTheDocument();
  });

  it("creates a category", async () => {
    const user = userEvent.setup();
    open();
    await user.click(
      await screen.findByRole("button", { name: /new category/i }),
    );
    await user.type(screen.getByLabelText("Name"), " Missions ");
    await user.click(
      within(screen.getByRole("dialog")).getByRole("radio", {
        name: /expense/i,
      }),
    );
    await user.click(screen.getByRole("button", { name: "Create category" }));
    await waitFor(() => expect(sent("POST")).toHaveLength(1));
    expect(
      JSON.parse((sent("POST")[0]![1] as RequestInit).body as string),
    ).toEqual({ name: "Missions", categoryType: "Expense" });
  });

  it("renames a category and sends only the name", async () => {
    const user = await menuFor("Offerings", "Edit category");
    const name = await screen.findByLabelText("Name");
    await user.clear(name);
    await user.type(name, "Gifts");
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(sent("PUT")).toHaveLength(1));
    expect(
      JSON.parse((sent("PUT")[0]![1] as RequestInit).body as string),
    ).toEqual({ name: "Gifts" });
  });

  it("deletes a category nobody uses", async () => {
    const user = await menuFor("Donations", "Delete category");
    const dialog = await screen.findByRole("alertdialog");
    await user.click(within(dialog).getByRole("button", { name: /^delete/i }));
    await waitFor(() => expect(sent("DELETE")).toHaveLength(1));
  });

  it("will not delete a category in use, and moves its transactions instead", async () => {
    const user = await menuFor("Utilities", "Delete category");
    expect(
      await screen.findByText("Utilities is still in use"),
    ).toBeInTheDocument();
    expect(sent("DELETE")).toHaveLength(0);
    await user.click(
      screen.getByRole("button", { name: "Move 14 transactions" }),
    );
    expect(
      await screen.findByText(/no other expense category/i),
    ).toBeInTheDocument();
  });

  it("moves an income category to another income category and deletes it", async () => {
    const user = await menuFor("Offerings", "Delete category");
    await user.click(
      await screen.findByRole("button", { name: "Move 38 transactions" }),
    );
    const confirm = await screen.findByRole("button", {
      name: "Move and delete",
    });
    expect(confirm).toBeDisabled();
    await user.click(screen.getByRole("combobox", { name: "Move to" }));
    await user.click(await screen.findByRole("option", { name: "Donations" }));
    await user.click(screen.getByRole("button", { name: "Move and delete" }));
    await waitFor(() => expect(sent("POST")).toHaveLength(1));
    expect(
      JSON.parse((sent("POST")[0]![1] as RequestInit).body as string),
    ).toEqual({ targetCategoryId: 3, deleteSource: true });
  });
});

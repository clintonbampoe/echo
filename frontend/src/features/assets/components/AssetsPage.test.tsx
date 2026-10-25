import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Asset, AssetCategory, AssetSummary } from "../api";
import { AssetsPage } from "./AssetsPage";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const base = { description: null, createdAt: "2026-01-01T00:00:00Z" };
const rows: Asset[] = [
  {
    ...base,
    id: "a1",
    name: "Behringer X32 mixing desk",
    categoryId: 1,
    categoryName: "Equipment",
    serialNumber: "BX32-88214",
    purchaseDate: "2023-03-12",
    purchaseCost: 42000,
    currentValue: 31500,
    status: "InUse",
  },
  {
    ...base,
    id: "a2",
    name: "Sanctuary chairs (300)",
    categoryId: 2,
    categoryName: "Furniture",
    serialNumber: null,
    purchaseDate: "2020-02-02",
    purchaseCost: 45000,
    currentValue: 18000,
    status: "InStorage",
  },
  {
    ...base,
    id: "a3",
    name: "Old piano",
    categoryId: 3,
    categoryName: "Instruments",
    serialNumber: null,
    purchaseDate: null,
    purchaseCost: 9000,
    currentValue: 0,
    status: "Liquidated",
  },
];
const categories: AssetCategory[] = [
  { id: 1, name: "Equipment", assetCount: 1 },
  { id: 2, name: "Furniture", assetCount: 1 },
  { id: 3, name: "Instruments", assetCount: 1 },
];
const summary: AssetSummary = {
  totalAssets: 2,
  totalCurrentValue: 49500,
  totalPurchaseCost: 87000,
  totalDepreciation: 37500,
};

let list: Asset[];
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
    if (u.pathname.endsWith("/asset-categories")) return json(categories);
    if (u.pathname.endsWith("/assets/summary")) return json(summary);
    const status = u.searchParams.get("status");
    return json({
      data: list.filter((a) => !status || a.status === status),
      hasMore: false,
      next: null,
    });
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

const open = (address = "/assets") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <Routes>
          <Route path="/assets" element={<AssetsPage />} />
          <Route
            path="/assets/categories"
            element={<p>The categories page</p>}
          />
          <Route path="/assets/:id" element={<p>The asset page</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );

const asked = (path: string) =>
  fetchMock.mock.calls
    .map(([u]) => new URL(u as string, "http://x"))
    .filter((u) => u.pathname.endsWith(path));

describe("assets page", () => {
  it("shows the numbers and every asset with its serial, category, status, date, cost and value", async () => {
    open();
    const table = await screen.findByRole("list", { name: "Assets" });
    const desk = within(table)
      .getByText("Behringer X32 mixing desk")
      .closest("li") as HTMLElement;
    expect(within(desk).getByText("Equipment")).toBeInTheDocument();
    expect(within(desk).getByText("31,500")).toBeInTheDocument();
    expect(within(desk).getByText("Mar 2023")).toBeInTheDocument();
    const strip = screen.getByRole("region", { name: "Assets summary" });
    expect(await within(strip).findByText("49,500")).toBeInTheDocument();
    expect(within(strip).getByText("87,000")).toBeInTheDocument();
    expect(within(strip).getByText("37,500")).toBeInTheDocument();
    expect(within(strip).getByText("All time")).toBeInTheDocument();
    expect(
      within(strip).getByText("Not counting liquidated"),
    ).toBeInTheDocument();
  });

  it("asks for all time at first, so an old asset is not left out", async () => {
    open();
    await screen.findByRole("list", { name: "Assets" });
    const call = asked("/v1/assets")[0];
    expect(call?.searchParams.get("from")).toBe("2000-01-01");
    expect(call?.searchParams.get("to")).toBe("2100-12-31");
  });

  it("narrows by the status in the address and asks the server for it", async () => {
    open("/assets?status=InStorage");
    const table = await screen.findByRole("list", { name: "Assets" });
    expect(
      within(table).getByText("Sanctuary chairs (300)"),
    ).toBeInTheDocument();
    expect(within(table).queryByText("Old piano")).not.toBeInTheDocument();
    expect(
      asked("/v1/assets").some(
        (u) => u.searchParams.get("status") === "InStorage",
      ),
    ).toBe(true);
  });

  it("counts liquidated assets in the numbers when the list is of them", async () => {
    open("/assets?status=Liquidated");
    const strip = await screen.findByRole("region", { name: "Assets summary" });
    expect(await within(strip).findByText("49,500")).toBeInTheDocument();
    expect(
      within(strip).queryByText("Not counting liquidated"),
    ).not.toBeInTheDocument();
  });

  it("opens an asset from its row", async () => {
    const user = userEvent.setup();
    open();
    await user.click(
      await screen.findByRole("link", { name: /Sanctuary chairs/ }),
    );
    expect(await screen.findByText("The asset page")).toBeInTheDocument();
  });

  it("invites the first asset when there are none and nothing is narrowing the list", async () => {
    list = [];
    open();
    expect(await screen.findByText("Add your first asset")).toBeInTheDocument();
  });

  it("says nothing matches when filters are narrowing an empty list", async () => {
    list = [];
    open("/assets?status=Active");
    expect(await screen.findByText("No assets found")).toBeInTheDocument();
  });

  it("leads to the categories page from the header, and from the form", async () => {
    const user = userEvent.setup();
    open();
    expect(
      await screen.findByRole("link", { name: "Categories" }),
    ).toHaveAttribute("href", "/assets/categories");
    await user.click(screen.getByRole("button", { name: /add asset/i }));
    expect(
      await screen.findByRole("link", { name: "Manage categories" }),
    ).toHaveAttribute("href", "/assets/categories");
  });
});

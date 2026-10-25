import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Asset } from "../api/assets";
import { AssetPage } from "./AssetPage";
import { anAsset } from "../testing/fixtures";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const desk = anAsset({
  id: "a1",
  name: "Behringer X32 mixing desk",
  categoryId: 1,
  categoryName: "Equipment",
  serialNumber: "BX32-88214",
  purchaseDate: "2023-03-12",
  purchaseCost: 42000,
  currentValue: 31500,
  status: "InUse",
  description: "Main mixing desk for the sanctuary.",
  createdAt: "2026-01-01T00:00:00Z",
});

let asset: Asset | "missing";
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  asset = desk;
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/asset-categories"))
      return json([{ id: 1, name: "Equipment", assetCount: 1 }]);
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    return asset === "missing"
      ? json({ title: "Not found", status: 404 }, 404)
      : json(asset);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = (address = "/assets/a1") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <Routes>
          <Route path="/assets" element={<p>The assets list</p>} />
          <Route path="/assets/:id" element={<AssetPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );

describe("asset page", () => {
  it("shows the asset, its status, its three numbers and its details", async () => {
    open();
    expect(
      await screen.findByRole("heading", { name: "Behringer X32 mixing desk" }),
    ).toBeInTheDocument();
    expect(screen.getAllByText("In use").length).toBeGreaterThan(0);
    const figures = screen.getByRole("region", { name: "Asset figures" });
    expect(within(figures).getByText("31.5k")).toBeInTheDocument();
    expect(within(figures).getByText("As last entered")).toBeInTheDocument();
    expect(within(figures).getByText("42k")).toBeInTheDocument();
    expect(within(figures).getByText("12 Mar 2023")).toBeInTheDocument();
    expect(within(figures).getByText("10.5k")).toBeInTheDocument();
    expect(within(figures).getByText("25% below cost")).toBeInTheDocument();
    const details = screen.getByRole("region", { name: "Asset details" });
    expect(within(details).getByText("BX32-88214")).toBeInTheDocument();
    expect(
      within(details).getByText("Main mixing desk for the sanctuary."),
    ).toBeInTheDocument();
    expect(
      within(details).getByText(/Echo does not depreciate it automatically/),
    ).toBeInTheDocument();
  });

  it("shows dashes for what the asset has no value for", async () => {
    asset = {
      ...desk,
      serialNumber: null,
      purchaseDate: null,
      description: null,
    };
    open();
    const details = await screen.findByRole("region", {
      name: "Asset details",
    });
    expect(within(details).getAllByText("–")).toHaveLength(3);
    expect(
      within(screen.getByRole("region", { name: "Asset figures" })).getByText(
        "No purchase date",
      ),
    ).toBeInTheDocument();
  });

  it("says it gained value when it is worth more than it cost", async () => {
    asset = { ...desk, purchaseCost: 100000, currentValue: 150000 };
    open();
    const figures = await screen.findByRole("region", {
      name: "Asset figures",
    });
    expect(within(figures).getByText("Gained value")).toBeInTheDocument();
    expect(within(figures).getByText("50% above cost")).toBeInTheDocument();
  });

  it("says the asset cannot be found, with the way back", async () => {
    asset = "missing";
    open();
    expect(
      await screen.findByText("We could not find this asset"),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Assets" })).toBeInTheDocument();
  });

  it("opens Edit, and Delete, which goes back to the list when done", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("heading", { name: "Behringer X32 mixing desk" });
    await user.click(screen.getByRole("button", { name: "Edit" }));
    expect(
      await screen.findByText("Edit asset", { selector: "h2" }),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Close" }));
    await user.click(
      screen.getByRole("button", {
        name: "More actions for Behringer X32 mixing desk",
      }),
    );
    await user.click(screen.getByRole("menuitem", { name: "Delete" }));
    expect(
      await screen.findByText("Delete Behringer X32 mixing desk?"),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Delete asset" }));
    expect(await screen.findByText("The assets list")).toBeInTheDocument();
    await waitFor(() =>
      expect(
        fetchMock.mock.calls.some(
          ([, i]) => (i as RequestInit | undefined)?.method === "DELETE",
        ),
      ).toBe(true),
    );
  });
});

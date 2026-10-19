import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import { pickDate } from "@/test/pickers";
import type { Asset } from "../api";
import { AssetSheet } from "./AssetSheet";
import { DeleteAssetDialog } from "./DeleteAssetDialog";
import { EditAssetSheet } from "./EditAssetSheet";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const desk: Asset = {
  id: "a1",
  name: "Behringer X32 mixing desk",
  categoryId: 1,
  categoryName: "Equipment",
  serialNumber: "BX32-88214",
  purchaseDate: "2023-03-12",
  purchaseCost: 42000,
  currentValue: 31500,
  status: "InUse",
  description: "Main desk.",
  createdAt: "2026-01-01T00:00:00Z",
};
const categories = [
  { id: 1, name: "Equipment", assetCount: 1 },
  { id: 2, name: "Vehicles", assetCount: 0 },
];

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  vi.useFakeTimers({ toFake: ["Date"], now: new Date("2026-10-19T10:00:00") });
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/asset-categories")) return json(categories);
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    return json(desk);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

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

describe("new asset sheet", () => {
  it("says what is missing and sends nothing until it is there", async () => {
    const user = userEvent.setup();
    wrap(<AssetSheet open onClose={() => {}} />);
    await user.click(screen.getByRole("button", { name: "Create asset" }));
    expect(await screen.findByText("Enter an asset name")).toBeInTheDocument();
    expect(screen.getAllByText("Choose a category")).toHaveLength(2);
    expect(
      screen.getByText("Enter a cost between 0 and 1,000,000"),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Enter a value between 0 and 1,000,000"),
    ).toBeInTheDocument();
    expect(screen.getByText("Fix 4 fields to continue")).toBeInTheDocument();
    expect(sent("POST").url).toBe("");
  });

  it("creates the asset with what was filled in, leaving out what was not, then closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<AssetSheet open onClose={onClose} />);
    await user.type(screen.getByLabelText("Asset name"), "Projector");
    await user.click(screen.getByRole("combobox", { name: /Category/ }));
    await user.click(await screen.findByRole("option", { name: "Equipment" }));
    await user.type(screen.getByLabelText(/Purchase cost/), "6,200");
    await user.type(screen.getByLabelText(/Current value/), "5400");
    await user.click(screen.getByRole("button", { name: "Create asset" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(sent("POST").body).toEqual({
      categoryId: 1,
      name: "Projector",
      status: "Active",
      purchaseCost: 6200,
      currentValue: 5400,
    });
  });

  it("sends the status, serial number, date and note when they are filled in", async () => {
    const user = userEvent.setup();
    wrap(<AssetSheet open onClose={() => {}} />);
    await user.type(screen.getByLabelText("Asset name"), "Church bus");
    await user.click(screen.getByRole("combobox", { name: /Category/ }));
    await user.click(await screen.findByRole("option", { name: "Vehicles" }));
    await user.click(screen.getByRole("combobox", { name: /Status/ }));
    await user.click(
      await screen.findByRole("option", { name: "Under maintenance" }),
    );
    await user.type(screen.getByLabelText(/Serial number/), "GT 4421-19");
    await pickDate(user, "Purchase date", "2021-08-04");
    await user.type(screen.getByLabelText(/Purchase cost/), "185000");
    await user.type(screen.getByLabelText(/Current value/), "120000");
    await user.type(screen.getByLabelText(/Description/), "Toyota Hiace");
    await user.click(screen.getByRole("button", { name: "Create asset" }));
    await waitFor(() => expect(sent("POST").url).not.toBe(""));
    expect(sent("POST").body).toEqual({
      categoryId: 2,
      name: "Church bus",
      status: "UnderMaintenance",
      serialNumber: "GT 4421-19",
      purchaseDate: "2021-08-04",
      purchaseCost: 185000,
      currentValue: 120000,
      description: "Toyota Hiace",
    });
  });

  it("stays open and says so when the server cannot save it", async () => {
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      const u = new URL(url, "http://x");
      if (u.pathname.endsWith("/asset-categories")) return json(categories);
      return init?.method === "POST"
        ? json({ title: "Boom", status: 500 }, 500)
        : json(desk);
    });
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<AssetSheet open onClose={onClose} />);
    await user.type(screen.getByLabelText("Asset name"), "Projector");
    await user.click(screen.getByRole("combobox", { name: /Category/ }));
    await user.click(await screen.findByRole("option", { name: "Equipment" }));
    await user.type(screen.getByLabelText(/Purchase cost/), "10");
    await user.type(screen.getByLabelText(/Current value/), "10");
    await user.click(screen.getByRole("button", { name: "Create asset" }));
    expect(
      await screen.findByText("We could not save this asset"),
    ).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });
});

describe("edit asset sheet", () => {
  it("starts with what the asset has and saves a change", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<EditAssetSheet asset={desk} onClose={onClose} />);
    expect(screen.getByLabelText("Asset name")).toHaveValue(
      "Behringer X32 mixing desk",
    );
    expect(screen.getByLabelText(/Purchase cost/)).toHaveValue("42,000.00");
    expect(screen.getByLabelText(/Current value/)).toHaveValue("31,500.00");
    expect(screen.getByLabelText(/Serial number/)).toHaveValue("BX32-88214");
    await user.clear(screen.getByLabelText(/Current value/));
    await user.type(screen.getByLabelText(/Current value/), "30000");
    await user.clear(screen.getByLabelText(/Serial number/));
    expect(screen.getByText("Unsaved changes")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const put = sent("PUT");
    expect(put.url).toContain("/v1/assets/a1");
    expect(put.body).toMatchObject({
      name: "Behringer X32 mixing desk",
      currentValue: 30000,
      purchaseCost: 42000,
      serialNumber: "",
      status: "InUse",
      purchaseDate: "2023-03-12",
      description: "Main desk.",
    });
  });

  it("does not let a set purchase date be taken away", async () => {
    const user = userEvent.setup();
    wrap(<EditAssetSheet asset={desk} onClose={() => {}} />);
    await user.click(screen.getByRole("button", { name: /^Purchase date/ }));
    await user.click(
      within(screen.getByRole("dialog", { name: "Purchase date" })).getByRole(
        "button",
        { name: "Clear" },
      ),
    );
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    expect(
      await screen.findByText("A purchase date can be changed, not removed"),
    ).toBeInTheDocument();
    expect(sent("PUT").url).toBe("");
  });
});

describe("delete asset dialog", () => {
  it("says what is kept and points to Liquidated for an asset that was sold or scrapped", () => {
    wrap(<DeleteAssetDialog asset={desk} onClose={() => {}} />);
    expect(
      screen.getByText("Delete Behringer X32 mixing desk?"),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/set the status to Liquidated instead/),
    ).toBeInTheDocument();
  });

  it("keeps the asset when asked to", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<DeleteAssetDialog asset={desk} onClose={onClose} />);
    await user.click(screen.getByRole("button", { name: "Keep asset" }));
    expect(onClose).toHaveBeenCalled();
    expect(sent("DELETE").url).toBe("");
  });

  it("deletes, closes and tells the page", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    const onDeleted = vi.fn();
    wrap(
      <DeleteAssetDialog
        asset={desk}
        onClose={onClose}
        onDeleted={onDeleted}
      />,
    );
    await user.click(screen.getByRole("button", { name: "Delete asset" }));
    await waitFor(() => expect(onDeleted).toHaveBeenCalled());
    expect(sent("DELETE").url).toContain("/v1/assets/a1");
    expect(onClose).toHaveBeenCalled();
  });
});

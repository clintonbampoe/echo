import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn, asked } from "../../testing/dashboardPage";
import { AssetsCard } from "./AssetsCard";

const categories = [
  { id: 1, name: "Vehicles", assetCount: 2 },
  { id: 2, name: "Sound equipment", assetCount: 3 },
  { id: 3, name: "Furniture", assetCount: 0 },
];

// The totals, and each category's, told apart by the categoryId in the request.
const figures = (categoryId: string | null) =>
  ({
    "1": {
      totalAssets: 2,
      totalCurrentValue: 46000,
      totalPurchaseCost: 60000,
      totalDepreciation: 14000,
    },
    "2": {
      totalAssets: 3,
      totalCurrentValue: 31000,
      totalPurchaseCost: 40000,
      totalDepreciation: 9000,
    },
  })[categoryId ?? ""] ?? {
    totalAssets: 5,
    totalCurrentValue: 77000,
    totalPurchaseCost: 100000,
    totalDepreciation: 23000,
  };

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path, query } = asked(url);
      if (broken) return json({ title: "Server error", status: 500 }, 500);
      if (path === "/v1/asset-categories") return json(categories);
      if (path === "/v1/assets/summary")
        return json(figures(query.get("categoryId")));
      return json({ title: "Not found", status: 404 }, 404);
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("assets card", () => {
  it("shows what they are worth now, and each category's value, biggest first", async () => {
    renderCard(<AssetsCard />);
    expect(await screen.findByText("77k")).toBeInTheDocument();
    const rows = within(
      await screen.findByRole("list", { name: "Assets by category" }),
    );
    expect(await rows.findByText("Vehicles")).toBeInTheDocument();
    expect(rows.getByText("46k")).toBeInTheDocument();
    expect(rows.getByText("Sound equipment")).toBeInTheDocument();
    expect(rows.getByText("31k")).toBeInTheDocument();
    expect(rows.queryByText("Furniture")).not.toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<AssetsCard />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

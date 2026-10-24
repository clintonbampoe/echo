import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderCard, signIn, asked } from "../dashboardTestKit";
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
  it("shows the value against the cost, and each category's share of the value", async () => {
    renderCard(<AssetsCard />);
    expect(
      await screen.findByText("77% of what they cost"),
    ).toBeInTheDocument();
    expect(screen.getByText("GH₵ 100,000")).toBeInTheDocument();
    const rows = within(await screen.findByRole("list"));
    expect(rows.getByText("Vehicles")).toBeInTheDocument();
    expect(rows.getByText("60%")).toBeInTheDocument();
    expect(rows.queryByText("Furniture")).not.toBeInTheDocument();
  });

  it("gives the value and the categories a card each when split", async () => {
    renderCard(<AssetsCard split />);
    expect(
      await screen.findByText("77% of what they cost"),
    ).toBeInTheDocument();
    expect(screen.getByRole("region", { name: "Assets" })).toBeInTheDocument();
    expect(
      screen.getByRole("region", { name: "Assets by category" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("radio", { name: "Count" })).toBeInTheDocument();
  });

  it("counts assets instead of their value when asked", async () => {
    renderCard(<AssetsCard />);
    await screen.findByRole("list");
    await userEvent.click(screen.getByRole("radio", { name: "Count" }));
    expect(
      await within(screen.getByRole("list")).findByText("60%"),
    ).toBeInTheDocument();
    expect(within(screen.getByRole("list")).getByText("3")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    renderCard(<AssetsCard />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

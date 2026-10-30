import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn, asked } from "../../testing/dashboardPage";
import { AssetsCard } from "./AssetsCard";

// Furniture holds nothing, so the server leaves it out.
const categories = [
  {
    categoryId: 1,
    categoryName: "Vehicles",
    count: 2,
    totalCurrentValue: 46000,
  },
  {
    categoryId: 2,
    categoryName: "Sound equipment",
    count: 3,
    totalCurrentValue: 31000,
  },
];

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path } = asked(url);
      if (broken) return json({ title: "Server error", status: 500 }, 500);
      if (path === "/v1/assets/summary/by-category") return json(categories);
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

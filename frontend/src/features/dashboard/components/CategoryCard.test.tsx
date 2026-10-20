import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderCard, signIn, asked } from "../dashboardTestKit";
import { CategoryCard } from "./CategoryCard";

const byCategory = [
  { categoryId: 1, categoryName: "Offerings", type: "Income", total: 21600 },
  { categoryId: 2, categoryName: "Donations", type: "Income", total: 12000 },
  { categoryId: 3, categoryName: "Hall rental", type: "Income", total: 7200 },
  { categoryId: 4, categoryName: "Events", type: "Income", total: 4800 },
  { categoryId: 5, categoryName: "Misc", type: "Income", total: 2400 },
  { categoryId: 6, categoryName: "Salaries", type: "Expense", total: 9000 },
];

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path } = asked(url);
      if (broken || path !== "/v1/transactions/summary")
        return json({ title: "Server error", status: 500 }, 500);
      return json({
        totalIncome: 48000,
        totalExpenses: 9000,
        net: 39000,
        byCategory,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () => renderCard(<CategoryCard today="2026-10-19" months={6} />);

describe("category card", () => {
  it("lists where the income came from, with each share", async () => {
    show();
    // Wait for the rows instead of the list: the list is on screen while it is still loading.
    await screen.findByText("Offerings");
    const rows = within(screen.getByRole("list"));
    expect(rows.getByText("45%")).toBeInTheDocument();
    expect(rows.getByText("GH₵ 21,600")).toBeInTheDocument();
    expect(
      screen.getByRole("img", { name: "Income by category" }),
    ).toBeInTheDocument();
  });

  it("switches to the expenses", async () => {
    show();
    await screen.findByRole("list");
    await userEvent.click(screen.getByRole("radio", { name: "Expenses" }));
    await screen.findByText("Salaries");
    const rows = within(screen.getByRole("list"));
    expect(rows.getByText("100%")).toBeInTheDocument();
    expect(screen.queryByText("Offerings")).not.toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    show();
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

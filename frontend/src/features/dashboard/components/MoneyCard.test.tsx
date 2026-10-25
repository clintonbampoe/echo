import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderCard, signIn, asked } from "../dashboardTestKit";
import { MoneyCard } from "./MoneyCard";

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path } = asked(url);
      if (broken) return json({ title: "Server error", status: 500 }, 500);
      if (path === "/v1/transactions/summary/monthly")
        return json([
          { year: 2026, month: 9, totalIncome: 20000, totalExpenses: 5000 },
          { year: 2026, month: 10, totalIncome: 30000, totalExpenses: 8000 },
        ]);
      return json({
        byCategory: [
          {
            categoryId: 1,
            categoryName: "Tithes",
            type: "Income",
            total: 30000,
          },
          {
            categoryId: 2,
            categoryName: "Offerings",
            type: "Income",
            total: 20000,
          },
          {
            categoryId: 3,
            categoryName: "Rent",
            type: "Expense",
            total: 13000,
          },
        ],
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () => renderCard(<MoneyCard today="2026-10-19" />);

describe("money card", () => {
  it("adds up the income of the months and says where it came from, with each share", async () => {
    show();
    expect(await screen.findByText("50,000")).toBeInTheDocument();
    const rows = within(
      screen.getByRole("list", { name: "Income by category" }),
    );
    expect(rows.getByText("Tithes")).toBeInTheDocument();
    expect(rows.getByText("60%")).toBeInTheDocument();
    expect(rows.getByText("40%")).toBeInTheDocument();
    expect(rows.queryByText("Rent")).not.toBeInTheDocument();
  });

  it("switches to expenses", async () => {
    show();
    await screen.findByText("50,000");
    await userEvent.click(screen.getByRole("radio", { name: "Expenses" }));
    expect(
      await screen.findByText("13,000", { selector: "p" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Where it went")).toBeInTheDocument();
    expect(
      within(
        screen.getByRole("list", { name: "Expenses by category" }),
      ).getByText("Rent"),
    ).toBeInTheDocument();
  });

  it("links to the finance page", async () => {
    show();
    expect(
      await screen.findByRole("link", { name: /Summary/ }),
    ).toHaveAttribute("href", "/finance");
  });

  it("says so when it could not load", async () => {
    broken = true;
    show();
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

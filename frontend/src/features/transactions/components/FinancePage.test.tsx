import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type {
  Category,
  Transaction,
  TransactionMonth,
  TransactionSummary,
} from "../api";
import { FinancePage } from "./FinancePage";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const categories: Category[] = [
  { id: 1, name: "Offerings", categoryType: "Income", transactionCount: 38 },
  { id: 2, name: "Utilities", categoryType: "Expense", transactionCount: 14 },
];
const rows: Transaction[] = [
  {
    id: "t1",
    categoryId: 2,
    categoryName: "Utilities",
    transactionType: "Expense",
    transactionDate: "2026-10-13",
    amount: 1240,
    description: "ECG electricity bill, September",
    createdAt: "2026-10-13T09:00:00Z",
  },
  {
    id: "t2",
    categoryId: 1,
    categoryName: "Offerings",
    transactionType: "Income",
    transactionDate: "2026-10-12",
    amount: 6850,
    description: "Sunday offering, 11 Oct",
    createdAt: "2026-10-12T09:00:00Z",
  },
];
const summary: TransactionSummary = {
  totalIncome: 6850,
  totalExpenses: 2140,
  net: 4710,
  mostActiveCategory: "Offerings",
  byCategory: [],
};
const months: TransactionMonth[] = [
  { year: 2026, month: 10, totalIncome: 6850, totalExpenses: 2140, net: 4710 },
];

let list: Transaction[];
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  list = rows;
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (init?.method === "POST") return json({ ...rows[0], id: "t9" });
    if (init?.method === "PUT")
      return json({ ...rows[0], ...JSON.parse(init.body as string) });
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    if (u.pathname.endsWith("/transaction-categories")) return json(categories);
    if (u.pathname.endsWith("/summary/monthly")) return json(months);
    if (u.pathname.endsWith("/summary")) return json(summary);
    const type = u.searchParams.get("transactionType");
    const category = u.searchParams.get("categoryId");
    const shown = list.filter(
      (t) =>
        (!type || t.transactionType === type) &&
        (!category || String(t.categoryId) === category),
    );
    return json({ data: shown, hasMore: false, next: null });
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = (address = "/finance") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <Routes>
          <Route path="/finance" element={<FinancePage />} />
          <Route path="/finance/categories" element={<p>The categories</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );

const asked = () =>
  fetchMock.mock.calls
    .map(([u]) => new URL(u as string, "http://x"))
    .filter((u) => u.pathname.endsWith("/v1/transactions"));

describe("finance page", () => {
  it("shows the numbers, the chart and every transaction with a signed amount", async () => {
    open();
    expect(
      await screen.findByRole("heading", { name: "Finance" }),
    ).toBeInTheDocument();
    expect(screen.getByText(/not part of these totals/)).toBeInTheDocument();
    const numbers = await screen.findByRole("region", {
      name: "Finance summary",
    });
    expect(within(numbers).getByText("6,850")).toBeInTheDocument();
    expect(within(numbers).getByText("+4,710")).toBeInTheDocument();
    expect(within(numbers).getByText("Transactions")).toBeInTheDocument();
    expect(
      await screen.findByRole("button", {
        name: "October 2026: income GH₵ 6,850, expenses GH₵ 2,140",
      }),
    ).toBeInTheDocument();
    const table = await screen.findByRole("list", { name: "Transactions" });
    expect(within(table).getByText("−1,240")).toBeInTheDocument();
    expect(within(table).getByText("+6,850")).toBeInTheDocument();
    expect(
      within(table).getByText("ECG electricity bill, September"),
    ).toBeInTheDocument();
  });

  it("asks for this month by default, and for the whole year when that is chosen", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("list", { name: "Transactions" });
    expect(asked()[0]?.searchParams.get("from")).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    await user.click(screen.getByRole("button", { name: /Range/ }));
    await user.click(await screen.findByRole("option", { name: "This year" }));
    await waitFor(() =>
      expect(asked().at(-1)?.searchParams.get("from")).toMatch(/^\d{4}-01-01$/),
    );
  });

  it("narrows the list to money in when Income is chosen", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("list", { name: "Transactions" });
    await user.click(screen.getByRole("button", { name: /Type/ }));
    await user.click(await screen.findByRole("option", { name: "Income" }));
    await waitFor(() =>
      expect(
        within(screen.getByRole("list", { name: "Transactions" })).queryByText(
          "Utilities",
        ),
      ).toBeNull(),
    );
    expect(asked().at(-1)?.searchParams.get("transactionType")).toBe("Income");
  });

  it("offers only the categories of the type on show", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("list", { name: "Transactions" });
    await user.click(screen.getByRole("button", { name: /Type/ }));
    await user.click(await screen.findByRole("option", { name: "Expenses" }));
    await user.click(screen.getByRole("button", { name: /Category/ }));
    expect(
      await screen.findByRole("option", { name: "Utilities" }),
    ).toBeInTheDocument();
    expect(screen.queryByRole("option", { name: "Offerings" })).toBeNull();
  });

  it("invites a first transaction when the period holds none", async () => {
    list = [];
    open();
    expect(
      await screen.findByText("No transactions this month"),
    ).toBeInTheDocument();
    expect(screen.queryByText("No transactions match")).toBeNull();
  });

  it("says nothing matches, rather than inviting a first one, when a filter is on", async () => {
    list = [];
    open("/finance?category=1");
    expect(
      await screen.findByText("No transactions match"),
    ).toBeInTheDocument();
  });

  it("links to the categories page", async () => {
    open();
    expect(
      await screen.findByRole("link", { name: "Categories" }),
    ).toHaveAttribute("href", "/finance/categories");
  });

  describe("recording, changing and deleting", () => {
    const sent = (method: string) => {
      const call = fetchMock.mock.calls.find(
        (c) => (c[1] as RequestInit | undefined)?.method === method,
      );
      return (
        call && {
          url: String(call[0]),
          body: call[1]?.body
            ? JSON.parse(String((call[1] as RequestInit).body))
            : undefined,
        }
      );
    };
    const pick = async (
      user: ReturnType<typeof userEvent.setup>,
      within_: HTMLElement,
      name: string,
    ) => {
      await user.click(
        within(within_).getByRole("combobox", { name: "Category" }),
      );
      await user.click(await screen.findByRole("option", { name }));
    };
    const openSheet = async (user: ReturnType<typeof userEvent.setup>) => {
      await screen.findByRole("list", { name: "Transactions" });
      await user.click(screen.getByRole("button", { name: /New transaction/ }));
      return screen.findByRole("dialog");
    };

    it("records money in with the category of that type", async () => {
      const user = userEvent.setup();
      open();
      const sheet = await openSheet(user);
      expect(
        within(sheet).getByRole("radio", { name: "Income" }),
      ).toBeChecked();
      expect(
        within(sheet).getByText(/Showing income categories only/),
      ).toBeInTheDocument();
      expect(
        within(sheet).getByRole("link", { name: "Manage categories" }),
      ).toHaveAttribute("href", "/finance/categories");
      await pick(user, sheet, "Offerings");
      await user.type(within(sheet).getByLabelText("Amount"), "500");
      await user.click(
        within(sheet).getByRole("button", { name: "Save transaction" }),
      );
      await waitFor(() => expect(screen.queryByRole("dialog")).toBeNull());
      expect(sent("POST")?.url).toContain("/v1/transactions");
      expect(sent("POST")?.body).toMatchObject({
        categoryId: 1,
        transactionType: "Income",
        amount: 500,
      });
    });

    it("says what is missing before it asks the server", async () => {
      const user = userEvent.setup();
      open();
      const sheet = await openSheet(user);
      await user.click(
        within(sheet).getByRole("button", { name: "Save transaction" }),
      );
      // The dropdown still says "Choose a category"; the message under it says the same.
      await waitFor(() =>
        expect(within(sheet).getAllByText("Choose a category")).toHaveLength(2),
      );
      expect(within(sheet).getByText("Enter the amount")).toBeInTheDocument();
      expect(
        within(sheet).getByText("Fix 2 fields to continue"),
      ).toBeInTheDocument();
      expect(sent("POST")).toBeUndefined();
    });

    it("clears a category that belongs to the other type, and says so", async () => {
      const user = userEvent.setup();
      open();
      const sheet = await openSheet(user);
      await pick(user, sheet, "Offerings");
      await user.click(within(sheet).getByRole("radio", { name: "Expense" }));
      expect(
        within(sheet).getByText(
          "Category cleared because it belongs to the other type. Choose again.",
        ),
      ).toBeInTheDocument();
      expect(
        within(sheet).getByRole("combobox", { name: "Category" }),
      ).toHaveTextContent("Choose a category");
    });

    it("keeps the type and the day for the next one, and says what was recorded", async () => {
      const user = userEvent.setup();
      open();
      const sheet = await openSheet(user);
      await user.click(within(sheet).getByRole("radio", { name: "Expense" }));
      await pick(user, sheet, "Utilities");
      await user.type(within(sheet).getByLabelText("Amount"), "1240");
      await user.click(
        within(sheet).getByRole("button", { name: "Save and add another" }),
      );
      expect(
        await within(sheet).findByText(
          "Recorded GH₵ 1,240 as an expense under Utilities. Type and date are kept for the next one.",
        ),
      ).toBeInTheDocument();
      expect(
        within(sheet).getByRole("radio", { name: "Expense" }),
      ).toBeChecked();
      expect(within(sheet).getByLabelText("Amount")).toHaveValue("");
      expect(
        within(sheet).getByRole("combobox", { name: "Category" }),
      ).toHaveTextContent("Choose a category");
    });

    it("opens a transaction to read first, with edit and delete as the next step", async () => {
      const user = userEvent.setup();
      open();
      await user.click(
        await screen.findByRole("button", {
          name: "ECG electricity bill, September",
        }),
      );
      const panel = await screen.findByRole("dialog");
      expect(within(panel).getByText("GH₵ 1,240")).toBeInTheDocument();
      expect(
        within(panel).getByText("Expense · Utilities"),
      ).toBeInTheDocument();
      expect(
        within(panel).getAllByText("ECG electricity bill, September"),
      ).toHaveLength(2);
      expect(within(panel).queryByLabelText("Amount")).toBeNull();
      await user.click(within(panel).getByRole("button", { name: "Edit" }));
      const sheet = await screen.findByRole("dialog", {
        name: /Edit transaction/,
      });
      expect(within(sheet).getByLabelText("Amount")).toHaveValue("1,240.00");
      expect(
        within(sheet).getByRole("radio", { name: "Expense" }),
      ).toBeChecked();
      await user.clear(within(sheet).getByLabelText("Amount"));
      await user.type(within(sheet).getByLabelText("Amount"), "1500");
      await user.click(
        within(sheet).getByRole("button", { name: "Save changes" }),
      );
      await waitFor(() =>
        expect(sent("PUT")?.url).toContain("/v1/transactions/t1"),
      );
      expect(sent("PUT")?.body).toMatchObject({
        categoryId: 2,
        transactionType: "Expense",
        amount: 1500,
      });
    });

    it("deletes from the transaction after asking", async () => {
      const user = userEvent.setup();
      open();
      await user.click(
        await screen.findByRole("button", {
          name: "ECG electricity bill, September",
        }),
      );
      await user.click(await screen.findByRole("button", { name: "Delete" }));
      const ask = await screen.findByRole("alertdialog");
      expect(
        within(ask).getByText(
          /ECG electricity bill, September for GHS 1,240 will be removed from October.s totals/,
        ),
      ).toBeInTheDocument();
      await user.click(
        within(ask).getByRole("button", { name: "Delete transaction" }),
      );
      await waitFor(() =>
        expect(sent("DELETE")?.url).toContain("/v1/transactions/t1"),
      );
    });
  });
});

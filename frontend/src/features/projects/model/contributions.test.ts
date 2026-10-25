import {
  contributionFilters,
  contributionStats,
  givenDates,
  methodName,
  remaining,
  toContributionFilters,
  methodParts,
} from "./contributions";

describe("contribution view", () => {
  it("works out what is left to raise, and how far past the target it went", () => {
    expect(remaining({ raisedAmount: 48200, targetAmount: 80000 })).toEqual({
      label: "Still to raise",
      amount: 31800,
    });
    expect(remaining({ raisedAmount: 80000, targetAmount: 80000 })).toEqual({
      label: "Still to raise",
      amount: 0,
    });
    expect(remaining({ raisedAmount: 85000, targetAmount: 80000 })).toEqual({
      label: "Over the target by",
      amount: 5000,
    });
  });

  it("names the method the way the app does, and shows one it does not know as sent", () => {
    expect(methodName("MobileMoney")).toBe("Mobile money");
    expect(methodName("Crypto")).toBe("Crypto");
    expect(methodName(null)).toBe("–");
  });

  it("shows dashes until the numbers arrive, then the numbers", () => {
    expect(contributionStats(undefined).map((s) => s.value)).toEqual([
      "–",
      "–",
      "–",
      "–",
    ]);
    const stats = contributionStats({
      totalContributed: 26800,
      totalContributions: 6,
      averageAmount: 4466.67,
      mostUsedPaymentMethod: "MobileMoney",
    });
    expect(stats.map((s) => s.value)).toEqual([
      "GH₵ 26.8k",
      "6",
      "GH₵ 4.47k",
      "Mobile money",
    ]);
  });

  it("starts at all time and turns the address into API filters", () => {
    expect(contributionFilters.isFiltered(contributionFilters.empty)).toBe(
      false,
    );
    expect(givenDates("all", "2026-10-19").from).toBe("2000-01-01");
    expect(givenDates("month", "2026-10-19")).toEqual({
      from: "2026-10-01",
      to: "2026-10-19",
    });
    expect(givenDates("year", "2026-10-19")).toEqual({
      from: "2026-01-01",
      to: "2026-10-19",
    });
    expect(givenDates("last-year", "2026-10-19")).toEqual({
      from: "2025-01-01",
      to: "2025-12-31",
    });
    expect(
      toContributionFilters(
        "p1",
        { method: "Cash", window: "year" },
        "2026-10-19",
      ),
    ).toEqual({
      projectId: "p1",
      paymentMethod: "Cash",
      from: "2026-01-01",
      to: "2026-10-19",
    });
    expect(
      toContributionFilters(
        "p1",
        { method: "nonsense", window: "all" },
        "2026-10-19",
      ).paymentMethod,
    ).toBeUndefined();
  });
});

describe("methodParts", () => {
  it("ranks the ways of giving, leaves out those that brought in nothing and folds the fifth into a closed Other", () => {
    const parts = methodParts([
      { method: "Cash", total: 300 },
      { method: "MobileMoney", total: 900 },
      { method: "Cheque", total: 0 },
      { method: "BankTransfer", total: 200 },
      { method: "CreditCard", total: 100 },
    ]);
    expect(parts.map((p) => [p.key, p.value])).toEqual([
      ["MobileMoney", 900],
      ["Cash", 300],
      ["BankTransfer", 200],
      ["CreditCard", 100],
    ]);
    const folded = methodParts([
      { method: "MobileMoney", total: 500 },
      { method: "Cash", total: 400 },
      { method: "BankTransfer", total: 300 },
      { method: "CreditCard", total: 200 },
      { method: "Cheque", total: 100 },
    ]);
    expect(folded.at(-1)).toMatchObject({
      key: "other",
      value: 100,
      closed: true,
    });
  });
});

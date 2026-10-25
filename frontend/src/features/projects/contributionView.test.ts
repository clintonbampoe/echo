import {
  contributionFilters,
  contributionStats,
  givenDates,
  methodName,
  remaining,
  toContributionFilters,
} from "./contributionView";

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
      "GH₵ 27k",
      "6",
      "GH₵ 4k",
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

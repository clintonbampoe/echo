import {
  emptyTithe,
  fromTithe,
  nextTithe,
  periodOf,
  titheSchema,
  toTitheInput,
  toTitheUpdate,
  yearOptions,
} from "./titheForm";

const today = "2026-10-14";
const schema = titheSchema(today);
const good = { ...emptyTithe(today), memberId: "m1", amount: "150" };

const problems = (values: object) => {
  const result = schema.safeParse(values);
  return result.success ? [] : result.error.issues.map((i) => i.message);
};

describe("tithe form", () => {
  it("starts on today, for this month, paid in cash", () => {
    expect(emptyTithe(today)).toMatchObject({
      collectionDate: today,
      forMonth: "October",
      forYear: "2026",
      paymentMethod: "Cash",
      amount: "",
    });
  });

  it("accepts a whole or two-place amount, with thousands commas", () => {
    expect(problems(good)).toEqual([]);
    expect(problems({ ...good, amount: "1,250.50" })).toEqual([]);
  });

  it("asks for a member and an amount", () => {
    expect(problems(emptyTithe(today))).toEqual([
      "Choose the member who gave",
      "Enter the amount",
    ]);
  });

  it.each([
    ["abc", "Enter an amount like 150 or 150.50"],
    ["10.555", "Enter an amount like 150 or 150.50"],
    ["0", "The amount must be more than zero"],
    ["1000000.01", "The amount cannot be more than 1,000,000"],
  ])("rejects the amount %s", (amount, message) => {
    expect(problems({ ...good, amount })).toEqual([message]);
  });

  it("rejects a day in the future", () => {
    expect(problems({ ...good, collectionDate: "2026-10-15" })).toEqual([
      "The day it was received cannot be in the future",
    ]);
  });

  it("turns the form into what the API takes", () => {
    expect(
      toTitheInput({
        ...good,
        amount: "1,250.50",
        description: "  for the family  ",
      }),
    ).toEqual({
      memberId: "m1",
      amount: 1250.5,
      forYear: 2026,
      forMonth: "October",
      paymentMethod: "Cash",
      collectionDate: today,
      description: "for the family",
    });
    expect(toTitheInput(good).description).toBeUndefined();
  });

  it("leaves the member out of an update", () => {
    expect(toTitheUpdate(good)).not.toHaveProperty("memberId");
    expect(toTitheUpdate({ ...good, amount: "200" })).toMatchObject({
      amount: 200,
      forMonth: "October",
    });
  });

  it("fills the edit form from a tithe", () => {
    const values = fromTithe({
      id: "t",
      memberId: "m1",
      memberName: "Ama",
      amount: 150,
      forYear: 2026,
      forMonth: "September",
      paymentMethod: "MobileMoney",
      collectionDate: "2026-10-04T00:00:00",
      createdAt: "",
    });
    expect(values).toMatchObject({
      amount: "150",
      collectionDate: "2026-10-04",
      forMonth: "September",
      description: "",
    });
  });

  it("keeps the day, month and method for the next tithe, and clears the member, amount and note", () => {
    const next = nextTithe({
      ...good,
      description: "x",
      collectionDate: "2026-10-11",
      forMonth: "September",
    });
    expect(next).toMatchObject({
      memberId: "",
      collectionDate: "2026-10-11",
      forMonth: "September",
      amount: "",
      description: "",
    });
  });

  it("offers five years back to next year, and the year being edited when it is older", () => {
    expect(yearOptions(today).map((o) => o.value)).toEqual([
      "2027",
      "2026",
      "2025",
      "2024",
      "2023",
      "2022",
      "2021",
    ]);
    expect(yearOptions(today, "2015").map((o) => o.value)).toContain("2015");
  });

  it("reads the month and year a day falls in", () => {
    expect(periodOf("2026-03-09")).toEqual({
      forMonth: "March",
      forYear: "2026",
    });
  });
});

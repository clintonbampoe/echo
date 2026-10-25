import {
  contributionSchema,
  emptyContribution,
  fromContribution,
  nextContribution,
  toContributionInput,
  toContributionUpdate,
} from "./contribution";
import { aContribution } from "../testing/fixtures";

const gift = aContribution({
  id: "c1",
  projectId: "p1",
  projectName: "Roof",
  amount: 4200,
  dateContributed: "2026-10-11T00:00:00",
  paymentMethod: "MobileMoney",
  description: "Sunday appeal",
  createdAt: "2026-10-11T00:00:00Z",
});
const problems = (amount: string) => {
  const r = contributionSchema.safeParse({
    ...emptyContribution("2026-10-19"),
    amount,
  });
  return r.success ? [] : r.error.issues.map((i) => i.message);
};

describe("contribution form", () => {
  it("starts as cash today with nothing else filled in", () => {
    expect(emptyContribution("2026-10-19")).toEqual({
      amount: "",
      dateContributed: "2026-10-19",
      paymentMethod: "Cash",
      description: "",
    });
  });

  it("asks for the amount, and for one the server will take", () => {
    expect(problems("")).toEqual(["Enter the amount"]);
    expect(problems("abc")).toEqual([
      "Enter an amount between 0.01 and 1,000,000",
    ]);
    expect(problems("0")).toEqual([
      "Enter an amount between 0.01 and 1,000,000",
    ]);
    expect(problems("1,000,000.01")).toEqual([
      "Enter an amount between 0.01 and 1,000,000",
    ]);
    expect(problems("1,250.50")).toEqual([]);
    expect(problems("0.01")).toEqual([]);
  });

  it("needs a date", () => {
    const r = contributionSchema.safeParse({
      ...emptyContribution(""),
      amount: "5",
    });
    expect(r.success).toBe(false);
  });

  it("turns a contribution into the form and back", () => {
    const values = fromContribution(gift);
    expect(values).toEqual({
      amount: "4,200.00",
      dateContributed: "2026-10-11",
      paymentMethod: "MobileMoney",
      description: "Sunday appeal",
    });
    expect(toContributionInput("p1", values)).toEqual({
      projectId: "p1",
      amount: 4200,
      dateContributed: "2026-10-11",
      paymentMethod: "MobileMoney",
      description: "Sunday appeal",
    });
  });

  it("sends no note as null when recording (the server needs the key), and as empty text when saving changes", () => {
    const values = { ...fromContribution(gift), description: "  " };
    expect(toContributionInput("p1", values)).toHaveProperty(
      "description",
      null,
    );
    expect(toContributionUpdate(values)).toEqual({
      amount: 4200,
      dateContributed: "2026-10-11",
      paymentMethod: "MobileMoney",
      description: "",
    });
    expect(toContributionUpdate(values)).not.toHaveProperty("projectId");
  });

  it("keeps the day and method for the next gift of a sitting", () => {
    expect(
      nextContribution({
        amount: "50",
        dateContributed: "2026-10-11",
        paymentMethod: "Cheque",
        description: "x",
      }),
    ).toEqual({
      amount: "",
      dateContributed: "2026-10-11",
      paymentMethod: "Cheque",
      description: "",
    });
  });
});

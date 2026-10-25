import { shareOf } from "./shares";

const row = (key: string, total: number) => ({
  key,
  name: key.toUpperCase(),
  total,
});

describe("shareOf", () => {
  it("orders the rows biggest first, with their share of the whole", () => {
    const { total, slices } = shareOf([row("a", 25), row("b", 75)]);
    expect(total).toBe(100);
    expect(slices.map((s) => [s.name, s.percent])).toEqual([
      ["B", 75],
      ["A", 25],
    ]);
  });

  it("leaves out rows with nothing in them", () => {
    expect(shareOf([row("a", 0), row("b", 10)]).slices).toHaveLength(1);
  });

  it("puts everything after the biggest four under Other", () => {
    const { slices } = shareOf([
      row("a", 50),
      row("b", 40),
      row("c", 30),
      row("d", 20),
      row("e", 10),
      row("f", 5),
    ]);
    expect(slices).toHaveLength(5);
    expect(slices[4]).toMatchObject({ name: "Other", total: 15 });
  });

  it("has no slices for nothing", () => {
    expect(shareOf([])).toEqual({ total: 0, slices: [] });
  });
});

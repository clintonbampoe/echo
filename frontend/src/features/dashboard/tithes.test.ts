import { footerOf, titheView } from "./tithes";

const figures = (total: number, tithers = 214, average = 52.8) => ({
  total,
  tithers,
  average,
});

describe("footerOf", () => {
  it("says how far it moved against the stretch before", () => {
    expect(footerOf(11300, 10640, "week")).toBe("6.2% up on the week before");
    expect(footerOf(900, 1000, "month")).toBe("10% down on the month before");
    expect(footerOf(50, 50, "week")).toBe("Same as the week before");
  });

  it("says nothing when the stretch before was zero", () => {
    expect(footerOf(100, 0, "week")).toBe("");
  });
});

describe("titheView", () => {
  it("writes the latest figures and rounds the average to whole cedis", () => {
    expect(titheView([figures(10640), figures(11300)], "week")).toEqual({
      total: "GH₵ 11,300",
      tithers: "214",
      average: "GH₵ 53",
      footer: "6.2% up on the week before",
    });
  });

  it("writes the figures of the stretch picked", () => {
    expect(titheView([figures(10640), figures(11300)], "week", 0)).toEqual({
      total: "GH₵ 10,640",
      tithers: "214",
      average: "GH₵ 53",
      footer: "",
    });
  });

  it("has nothing to say until the figures arrive", () => {
    expect(titheView(undefined, "week")).toEqual({ footer: "" });
  });
});

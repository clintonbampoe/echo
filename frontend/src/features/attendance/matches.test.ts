import { splitName, standing } from "./matches";

describe("where a found person stands", () => {
  it("tells a person marked just now from one already on the list", () => {
    expect(standing("a", new Set(["a"]), new Set(["a"]))).toBe("now");
    expect(standing("b", new Set(), new Set(["b"]))).toBe("already");
    expect(standing("c", new Set(), new Set())).toBe("none");
  });
});

describe("a new visitor's name", () => {
  it("splits into a first and a last name", () => {
    expect(splitName("  Ama Serwaa  Boateng ")).toEqual({
      firstName: "Ama",
      lastName: "Serwaa Boateng",
    });
  });

  it("needs both", () => {
    expect(splitName("Ama")).toBeNull();
    expect(splitName("  ")).toBeNull();
  });
});

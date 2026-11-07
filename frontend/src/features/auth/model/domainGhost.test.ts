import { domainRest } from "./domainGhost";

describe("domainRest", () => {
  it("completes a known domain", () => {
    expect(domainRest("abena@gm")).toBe("ail.com");
    expect(domainRest("abena@Out")).toBe("look.com");
  });
  it("stays quiet when nothing fits", () => {
    expect(domainRest("abena")).toBe("");
    expect(domainRest("abena@")).toBe("");
    expect(domainRest("abena@gmail.com")).toBe("");
    expect(domainRest("abena@church.org")).toBe("");
    expect(domainRest("a@b@gm")).toBe("");
  });
});

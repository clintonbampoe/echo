import { paths } from "./paths";

describe("paths", () => {
  it("gives each record its own address", () => {
    expect(paths.member("m1")).toBe("/members/m1");
    expect(paths.event("e1")).toBe("/events/e1");
    expect(paths.organization("o1")).toBe("/organizations/o1");
    expect(paths.session("s1")).toBe("/attendance/sessions/s1");
    expect(paths.project("p1")).toBe("/projects/p1");
  });
});

import { roleLabel } from "./labels";

describe("labels", () => {
  it("names the roles in plain words", () => {
    expect(roleLabel("Admin")).toBe("Administrator");
    expect(roleLabel("Clerk")).toBe("Clerk");
  });
});

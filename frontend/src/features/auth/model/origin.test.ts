import { beforeEach, describe, expect, it } from "vitest";
import { forgetOrigin, recallOrigin, rememberOrigin, safePath } from "./origin";

describe("safePath", () => {
  it("accepts same-site paths only", () => {
    expect(safePath("/members?page=2")).toBe("/members?page=2");
    expect(safePath("//evil.example")).toBeUndefined();
    expect(safePath("https://evil.example")).toBeUndefined();
    expect(safePath(undefined)).toBeUndefined();
  });
});

describe("origin", () => {
  beforeEach(() => localStorage.clear());
  it("is remembered for the tab the email link opens, and can be forgotten", () => {
    rememberOrigin("/members");
    expect(recallOrigin()).toBe("/members");
    forgetOrigin();
    expect(recallOrigin()).toBeUndefined();
  });
  it("does not trust what storage holds", () => {
    localStorage.setItem("echo.verify.from", "//evil.example");
    expect(recallOrigin()).toBeUndefined();
  });
});

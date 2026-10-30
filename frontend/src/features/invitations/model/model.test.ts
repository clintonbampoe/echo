import { describe, expect, it } from "vitest";
import type { Invitation } from "../api/invitations";
import { joinLink, subLine, titleOf } from "./display";
import { invitationStats } from "./figures";

const NOW = Date.parse("2026-10-24T12:00:00Z");
const day = (n: number) => new Date(NOW + n * 86_400_000).toISOString();
const inv = (over: Partial<Invitation> = {}): Invitation => ({
  id: "i1",
  label: null,
  allowedRole: "Clerk",
  createdByName: "Kwabena",
  createdAt: day(-3),
  expiresAt: day(20),
  status: "Active",
  useCount: 0,
  ...over,
});

describe("titleOf", () => {
  it("uses the label when there is one, else the role", () => {
    expect(titleOf(inv({ label: "Youth team" }))).toBe("Youth team");
    expect(titleOf(inv())).toBe("Clerk invitation");
  });
});

describe("subLine", () => {
  it("says not used yet and a date for a far expiry", () => {
    expect(subLine(inv(), NOW)).toMatch(/^Clerk · not used yet · expires \d/);
  });
  it("counts days when close", () => {
    expect(subLine(inv({ expiresAt: day(2), useCount: 1 }), NOW)).toBe(
      "Clerk · used 1 time · expires in 2 days",
    );
  });
  it("shows the limit when there is one", () => {
    expect(subLine(inv({ maxUses: 10, useCount: 3 }), NOW)).toMatch(
      /^Clerk · 3 of 10 used · /,
    );
  });
  it("says never expires when there is no expiry date", () => {
    expect(subLine(inv({ expiresAt: null }), NOW)).toBe(
      "Clerk · not used yet · never expires",
    );
  });
  it("says all places taken once the limit is reached", () => {
    expect(
      subLine(inv({ status: "Exhausted", maxUses: 5, useCount: 5 }), NOW),
    ).toBe("Clerk · 5 of 5 used · all places taken");
  });
  it("says switched off for revoked", () => {
    expect(subLine(inv({ status: "Revoked", useCount: 6 }), NOW)).toContain(
      "switched off",
    );
  });
});

describe("invitationStats", () => {
  it("shows the server's counts", () => {
    const stats = invitationStats({
      active: 2,
      peopleJoined: 15,
      expiringSoon: 1,
    });
    expect(stats.map((s) => s.value)).toEqual(["2", "15", "1"]);
  });
  it("shows dashes while they load", () => {
    expect(invitationStats(undefined).map((s) => s.value)).toEqual([
      "–",
      "–",
      "–",
    ]);
  });
});

describe("joinLink", () => {
  it("points at the join screen with the code", () => {
    expect(joinLink("K7QX-92MD-PW4T", "https://echo.example")).toBe(
      "https://echo.example/join/K7QX-92MD-PW4T",
    );
  });
});

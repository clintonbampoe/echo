import { describe, expect, it } from "vitest";
import type { Stat } from "@/ui";
import type { Invitation } from "../api/invitations";
import { subLine, titleOf } from "./display";
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
  it("says switched off for revoked", () => {
    expect(subLine(inv({ status: "Revoked", useCount: 6 }), NOW)).toContain(
      "switched off",
    );
  });
});

describe("invitationStats", () => {
  it("counts active, joined and expiring within a week", () => {
    const rows = [
      inv({ useCount: 9 }),
      inv({ id: "2", expiresAt: day(3), useCount: 2 }),
      inv({ id: "3", status: "Revoked", useCount: 4 }),
    ];
    const [active, joined, soon] = invitationStats(rows, NOW) as [
      Stat,
      Stat,
      Stat,
    ];
    expect(active.value).toBe("2");
    expect(joined.value).toBe("15");
    expect(soon.value).toBe("1");
  });
});

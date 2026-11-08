import { describe, expect, it } from "vitest";
import type { RoleCount, StaffSummary, StaffUser } from "../api/staff";
import { flag } from "./display";
import { messageFor } from "./errors";
import { ringCentre, shareOf, staffLine } from "./figures";
import { staffFilters, toQuery } from "./filters";
import { countOf, roleFromSlug, roleParts, roleSlug } from "./roles";

const user = (over: Partial<StaffUser> = {}): StaffUser => ({
  id: "1",
  name: "Ama Mensah",
  emailAddress: "ama@echo.test",
  role: "Clerk",
  verifiedAt: "2026-03-03T10:00:00Z",
  disabledAt: null,
  createdAt: "2026-03-03T10:00:00Z",
  ...over,
});

const roles: RoleCount[] = [
  { role: "Admin", count: 2, disabled: 0 },
  { role: "Clerk", count: 4, disabled: 1 },
  { role: "Accountant", count: 0, disabled: 0 },
  { role: "Member", count: 16, disabled: 0 },
];

describe("flag", () => {
  it("says nothing for an ordinary account", () => {
    expect(flag(user(), false)).toBeNull();
  });
  it("marks your own account first", () => {
    expect(
      flag(user({ disabledAt: "2026-04-01T00:00:00Z" }), true)?.label,
    ).toBe("You");
  });
  it("marks a disabled account, then an unverified one", () => {
    expect(
      flag(user({ disabledAt: "2026-04-01T00:00:00Z" }), false)?.label,
    ).toBe("Disabled");
    expect(flag(user({ verifiedAt: null }), false)?.label).toBe("Unverified");
  });
});

describe("roles", () => {
  it("splits the bar biggest first and leaves out an empty role", () => {
    const parts = roleParts(roles);
    expect(parts.map((p) => p.key)).toEqual([
      "Member",
      "Clerk",
      "Admin",
      "Accountant",
    ]);
    expect(parts[0]?.value).toBe(16);
  });
  it("finds a role's count", () => {
    expect(countOf(roles, "Clerk")).toBe(4);
    expect(countOf(undefined, "Clerk")).toBeUndefined();
  });
});

describe("figures", () => {
  it("rounds a share to whole percent and shows a dash while loading", () => {
    expect(shareOf(4, 24)).toBe("17%");
    expect(shareOf(undefined, 24)).toBe("–");
    expect(shareOf(0, 0)).toBe("–");
  });
  it("says how many people and how many are disabled", () => {
    const s: StaffSummary = { total: 24, disabled: 1, roles };
    expect(staffLine(s)).toBe("24 people · 1 disabled");
    expect(staffLine({ ...s, disabled: 0 })).toBe("24 people");
    expect(staffLine(undefined)).toBeUndefined();
  });
});

describe("filters", () => {
  const read = (q: string) => staffFilters.read(new URLSearchParams(q));
  it("turns the address into what the lists ask the server for", () => {
    expect(toQuery(read(""))).toEqual({ status: undefined, q: undefined });
    expect(toQuery(read("status=Disabled&q=%20ama%20"))).toEqual({
      status: "Disabled",
      q: "ama",
    });
    expect(toQuery(read("status=nonsense")).status).toBeUndefined();
  });
});

describe("role addresses", () => {
  it("reads a role from its address and nothing else", () => {
    expect(roleSlug("Accountant")).toBe("accountant");
    expect(roleFromSlug("clerk")).toBe("Clerk");
    expect(roleFromSlug("nonsense")).toBeUndefined();
    expect(roleFromSlug(undefined)).toBeUndefined();
  });
});

describe("ring centre", () => {
  const s: StaffSummary = { total: 24, disabled: 1, roles };
  it("shows everyone, or the picked role's head count and share", () => {
    expect(ringCentre(s, null)).toEqual({
      value: "24",
      note: "people on staff",
    });
    expect(ringCentre(s, "Member")).toEqual({
      label: "Member",
      value: "16",
      note: "67% of staff",
    });
  });
  it("shows a dash while the numbers load", () => {
    expect(ringCentre(undefined, null).value).toBe("–");
  });
});

describe("messageFor", () => {
  it("explains the two refusals in words", () => {
    expect(messageFor({ errorCode: "LAST_ADMIN" })).toMatch(/only admin/);
    expect(messageFor({ errorCode: "CANNOT_MODIFY_SELF" })).toMatch(/your own/);
    expect(messageFor(new Error("x"))).toMatch(/Try again/);
  });
});

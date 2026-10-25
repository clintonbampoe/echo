import {
  createdStat,
  ledByStat,
  membersStat,
  organizationsStat,
  upcomingStat,
} from "./stat";

describe("organizationsStat", () => {
  it("is the count and up to three names", () => {
    expect(organizationsStat(["Choir", "Ushers"], false)).toMatchObject({
      value: "2",
      note: "Choir, Ushers",
    });
    expect(organizationsStat(["A", "B", "C", "D", "E"], false).note).toBe(
      "A, B, C +2",
    );
  });
  it("is a dash while loading", () => {
    expect(organizationsStat([], true).value).toBe("–");
  });
});

describe("organization page stats", () => {
  const today = "2026-10-14";

  it("counts the members", () => {
    expect(membersStat({ memberCount: 1204 })).toMatchObject({
      value: "1.2k",
      note: "In this group",
    });
  });

  it("names the first leader and who shares the lead", () => {
    expect(ledByStat(["Kofi Mensah"], false)).toMatchObject({
      value: "Kofi Mensah",
      note: "Leads this group",
    });
    expect(ledByStat(["Kofi Mensah", "Ama Boateng"], false).note).toBe(
      "and Ama Boateng",
    );
    expect(
      ledByStat(["Kofi Mensah", "Ama Boateng", "Yaw Owusu"], false).note,
    ).toBe("and 2 others");
  });

  it("says so when there is no leader, and waits while loading", () => {
    expect(ledByStat([], false)).toMatchObject({
      value: "–",
      note: "No leader yet",
    });
    expect(ledByStat([], true)).toMatchObject({ value: "–", note: "Loading" });
  });

  it("counts only the events in the next 60 days", () => {
    const starts = ["2026-11-14", "2026-12-13", "2026-12-14", "2027-03-01"];
    expect(upcomingStat(starts, false, today, false)).toMatchObject({
      value: "2",
      note: "Next 60 days",
    });
    expect(upcomingStat([], false, today, false).value).toBe("0");
    expect(upcomingStat([], false, today, true).value).toBe("–");
  });

  it("adds a plus when more events are still to load and every one so far is in the window", () => {
    expect(
      upcomingStat(["2026-11-01", "2026-11-02"], true, today, false).value,
    ).toBe("2+");
    expect(
      upcomingStat(["2026-11-01", "2027-03-01"], true, today, false).value,
    ).toBe("1");
  });

  it("gives the day it was created and how long ago", () => {
    expect(createdStat({ createdAt: "2016-02-03T09:00:00Z" }, today)).toEqual({
      label: "Created",
      value: "03 Feb 2016",
      note: "10 years ago",
    });
  });
});

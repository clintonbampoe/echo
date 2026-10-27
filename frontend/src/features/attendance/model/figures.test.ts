import { attendancePercent, attendanceStat, lastSeenStat } from "./figures";

describe("attendancePercent", () => {
  it("rounds, caps at 100, and is nothing when no sessions were held", () => {
    expect(attendancePercent(38, 44)).toBe(86);
    expect(attendancePercent(50, 44)).toBe(100);
    expect(attendancePercent(0, 0)).toBeNull();
  });
});

describe("attendance stats", () => {
  it("attendance: percent and the numbers behind it", () => {
    expect(attendanceStat(38, 44)).toEqual({
      label: "Attendance",
      value: "86",
      unit: "%",
      note: "38 of 44 sessions this year",
    });
    expect(attendanceStat(undefined, 44).value).toBe("–");
    expect(attendanceStat(0, 0).note).toBe("No sessions held yet this year");
  });

  it("last seen: the date and the session, or why there is none", () => {
    expect(
      lastSeenStat(
        { sessionDate: "2026-10-11", name: "Sunday Service" },
        false,
      ),
    ).toMatchObject({ value: "11 Oct", note: "Sunday Service" });
    expect(lastSeenStat(undefined, false).note).toBe("No attendance yet");
    expect(lastSeenStat(undefined, true).note).toBe("");
  });
});

import type { Session } from "./api";
import {
  emptySession,
  fromSession,
  labelSchema,
  lockedTimes,
  sessionSchema,
  toLabelInput,
  toSessionInput,
  toSessionUpdate,
} from "./sessionForm";

const session: Session = {
  id: "s1",
  sessionLabelId: 3,
  sessionLabelName: "Sunday Service",
  category: "Service",
  date: "2026-10-11",
  name: null,
  topic: "Walking in Faith",
  notes: null,
  startsAt: "08:00:00",
  endsAt: "10:30:00",
  status: "Scheduled",
  createdAt: "",
  attendanceCount: 0,
};

const check = (
  values: Parameters<ReturnType<typeof sessionSchema>["safeParse"]>[0],
  locked?: Parameters<typeof sessionSchema>[0],
) => sessionSchema(locked).safeParse(values);
const messages = (r: ReturnType<typeof check>) =>
  r.success ? [] : r.error.issues.map((i) => i.message);

describe("the session form", () => {
  it("starts on the chosen day with nothing filled in", () => {
    expect(emptySession("2026-10-12")).toMatchObject({
      sessionLabelId: "",
      date: "2026-10-12",
      name: "",
      startsAt: "",
    });
  });

  it("needs a type and a date", () => {
    expect(messages(check({ ...emptySession(""), date: "" }))).toEqual([
      "Choose a session type",
      "Enter the date of the session",
    ]);
  });

  it("wants the end after the start", () => {
    const v = {
      ...emptySession("2026-10-11"),
      sessionLabelId: "3",
      startsAt: "10:00",
      endsAt: "09:00",
    };
    expect(messages(check(v))).toEqual(["The end must be after the start"]);
    expect(messages(check({ ...v, endsAt: "11:00" }))).toEqual([]);
  });

  it("lets a time change but not go away", () => {
    const v = { ...fromSession(session), startsAt: "" };
    expect(messages(check(v, lockedTimes(session)))).toEqual([
      "A time can be changed, not removed",
    ]);
    expect(messages(check(v))).toEqual([]);
  });

  it("turns the form into what the API accepts", () => {
    const v = {
      ...emptySession("2026-10-11"),
      sessionLabelId: "3",
      name: " Easter ",
      startsAt: "08:00",
      topic: "",
      notes: " ",
    };
    expect(toSessionInput(v)).toEqual({
      sessionLabelId: 3,
      date: "2026-10-11",
      name: "Easter",
      topic: undefined,
      notes: undefined,
      startsAt: "08:00:00",
      endsAt: undefined,
    });
  });

  it("clears text on an update by sending it empty, and leaves unset times out", () => {
    expect(toSessionUpdate({ ...fromSession(session), topic: "" })).toEqual({
      name: "",
      topic: "",
      notes: "",
      startsAt: "08:00:00",
      endsAt: "10:30:00",
    });
    expect(
      toSessionUpdate({ ...emptySession("2026-10-11"), sessionLabelId: "3" })
        .startsAt,
    ).toBeUndefined();
  });

  it("fills the form from a session", () => {
    expect(fromSession(session)).toEqual({
      sessionLabelId: "3",
      date: "2026-10-11",
      name: "",
      topic: "Walking in Faith",
      startsAt: "08:00",
      endsAt: "10:30",
      notes: "",
    });
  });
});

describe("the session type form", () => {
  it("needs a name and a category", () => {
    const r = labelSchema.safeParse({ name: " ", category: "" });
    expect(r.success ? [] : r.error.issues.map((i) => i.message)).toEqual([
      "Enter a name",
      "Choose a category",
    ]);
  });

  it("trims the name", () => {
    expect(toLabelInput({ name: " Bible Study ", category: "Study" })).toEqual({
      name: "Bible Study",
      category: "Study",
    });
  });
});

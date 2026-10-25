import { buildCards, change, type Amount, type Numbers } from "./cards";
import { periodOf } from "./periods";

const amount = (value?: number, failed = false): Amount => ({
  value,
  failed,
  retry: () => {},
});

const numbers = (over: Partial<Numbers> = {}): Numbers => ({
  members: amount(1284),
  newMembers: amount(9),
  present: amount(912),
  presentBefore: amount(828),
  tithes: amount(11300),
  tithesBefore: amount(10640),
  net: amount(5120),
  netBefore: amount(5300),
  ...over,
});

const lastWeek = periodOf("last-week");

describe("change", () => {
  it("says how far a number moved, in whole percent", () => {
    expect(change(912, 828)).toEqual({
      text: "10%",
      tone: "positive",
      arrow: "up",
    });
    expect(change(5120, 5300)).toEqual({
      text: "3%",
      tone: "negative",
      arrow: "down",
    });
  });

  it("says nothing when it started from zero, and 0% when it did not move", () => {
    expect(change(100, 0)).toBeUndefined();
    expect(change(50, 50)).toEqual({ text: "0%", tone: "neutral" });
  });

  it("counts a smaller loss as a rise", () => {
    expect(change(-100, -200)).toMatchObject({ text: "50%", tone: "positive" });
  });
});

describe("buildCards", () => {
  it("shows the four numbers, each with what it is compared against", () => {
    const [members, present, tithes, net] = buildCards(numbers(), lastWeek);
    expect(members).toMatchObject({
      value: "1k",
      chip: { text: "9 new", tone: "positive" },
      note: "last week",
    });
    expect(present).toMatchObject({
      value: "912",
      chip: { text: "10%" },
      note: "vs the week before",
    });
    expect(tithes?.value).toBe("GH₵ 11k");
    expect(net).toMatchObject({
      value: "GH₵ 5k",
      chip: { tone: "negative" },
    });
  });

  it("marks a card loading until its number is here", () => {
    const [, present] = buildCards(numbers({ present: amount() }), lastWeek);
    expect(present?.state).toBe("loading");
  });

  it("says so on a card that could not load, and leaves the others alone", () => {
    const [, present, tithes] = buildCards(
      numbers({ present: amount(undefined, true) }),
      lastWeek,
    );
    expect(present).toMatchObject({
      state: "failed",
      value: "–",
      note: "Could not load",
    });
    expect(tithes?.state).toBe("ready");
  });

  it("keeps the number and drops the chip when the earlier stretch is missing", () => {
    const [, present] = buildCards(
      numbers({ presentBefore: amount(undefined, true) }),
      lastWeek,
    );
    expect(present).toMatchObject({ value: "912", chip: undefined });
  });
});

import { describe, expect, it } from "vitest";
import { greetingFor } from "./greeting";
import { headline } from "./headline";

const base = {
  phone: false,
  hour: 9,
  fullName: "Kwame Boateng",
  church: "Grace Chapel",
};

describe("headline", () => {
  it("greets, with the church under the name when there is nothing to note", () => {
    expect(headline(base)).toEqual({
      title: `${greetingFor(9)}, Kwame`,
      line: "Grace Chapel",
    });
  });

  it("lists what needs a look", () => {
    expect(headline({ ...base, atRisk: 1, newVisitors: 3 }).line).toBe(
      "1 project at risk · 3 new visitors this month",
    );
  });

  it("welcomes a church with nothing recorded, shorter on a phone", () => {
    expect(headline({ ...base, welcomeLeft: 4 })).toEqual({
      title: "Welcome, Kwame",
      line: "Grace Chapel is set up. Four more steps and Echo starts giving answers back.",
    });
    expect(headline({ ...base, welcomeLeft: 4, phone: true }).line).toBe(
      "Grace Chapel is set up. Four more steps to go.",
    );
  });
});

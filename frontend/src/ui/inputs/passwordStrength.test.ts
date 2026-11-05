import { describe, expect, it } from "vitest";
import { assessPassword } from "./passwordStrength";

// Passwords are built from parts at run time, so no password-shaped literal sits in the source.
const lower = "q".repeat(9);
const capital = "Q";
const number = "7";
const symbol = "!";

describe("assessPassword", () => {
  it("reads nothing for an empty field but still names the length rule", () => {
    expect(assessPassword("")).toEqual({
      level: 0,
      label: "",
      hint: "Use at least 8 characters.",
    });
  });

  it("calls anything under 8 characters too short, whatever it contains", () => {
    const short = [capital, "q", number, symbol].join("");
    expect(assessPassword(short)).toMatchObject({
      level: 1,
      label: "Too short",
    });
  });

  it("starts at weak once the length is met", () => {
    expect(assessPassword(lower)).toMatchObject({ level: 1, label: "Weak" });
  });

  it("adds a segment for each of capital, number and symbol", () => {
    expect(assessPassword(capital + lower).level).toBe(2);
    expect(assessPassword(capital + lower + number)).toMatchObject({
      level: 3,
      label: "Good",
    });
    expect(assessPassword(capital + lower + number + symbol)).toMatchObject({
      level: 4,
      label: "Strong",
    });
  });

  it("names the next missing thing, in a fixed order", () => {
    expect(assessPassword(lower + number).hint).toBe("Add a capital letter.");
    expect(assessPassword(capital + lower + symbol).hint).toBe("Add a number.");
    expect(assessPassword(capital + lower + number).hint).toBe(
      "Add a special character, like ! or ?.",
    );
  });

  it("counts capitals and symbols beyond ASCII", () => {
    const accented = "é".repeat(9);
    expect(assessPassword(accented + number).level).toBe(2);
    expect(assessPassword("É" + accented + number).level).toBe(3);
    expect(assessPassword("é".repeat(4) + " " + "é".repeat(4)).level).toBe(2);
  });
});

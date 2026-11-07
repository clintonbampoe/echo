import { greetingFor } from "./greeting";

describe("greetingFor", () => {
  it("follows the hour", () => {
    expect(greetingFor(6)).toBe("Good morning");
    expect(greetingFor(12)).toBe("Good afternoon");
    expect(greetingFor(19)).toBe("Good evening");
  });
  it("is kind to people up late", () => {
    expect(greetingFor(23)).toBe("Late one. Glad you are here.");
    expect(greetingFor(2)).toBe("Late one. Glad you are here.");
  });
});

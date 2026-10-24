import { afterEach, describe, expect, it } from "vitest";
import {
  getPreferences,
  resetPreferences,
  setPreferences,
} from "./preferences";
import { formatMoney } from "./format";

afterEach(resetPreferences);

describe("preferences", () => {
  it("writes density and motion onto <html> and remembers them", () => {
    setPreferences({ density: "compact", reduceMotion: true });
    expect(document.documentElement.dataset.density).toBe("compact");
    expect(document.documentElement.dataset.reduceMotion).toBe("true");
    expect(
      JSON.parse(localStorage.getItem("echo.preferences") ?? "{}").density,
    ).toBe("compact");
  });

  it("drops the currency symbol from money when switched off", () => {
    expect(formatMoney(6400)).toContain("₵");
    setPreferences({ currencySymbol: false });
    expect(formatMoney(6400)).not.toContain("₵");
    expect(getPreferences().currencySymbol).toBe(false);
  });
});

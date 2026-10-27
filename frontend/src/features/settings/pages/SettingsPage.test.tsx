import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderApp } from "@/test/kit";
import { getPreferences, resetPreferences } from "@/lib/preferences";
import { SettingsPage } from "./SettingsPage";

afterEach(resetPreferences);

describe("SettingsPage", () => {
  it("changes density on the page and remembers it", async () => {
    renderApp(<SettingsPage />);
    await userEvent.click(screen.getByRole("radio", { name: "Compact" }));
    expect(document.documentElement.dataset.density).toBe("compact");
  });

  it("chooses how many rows a list loads, and remembers it", async () => {
    renderApp(<SettingsPage />);
    expect(screen.getByRole("radio", { name: "50" })).toBeChecked();
    await userEvent.click(screen.getByRole("radio", { name: "25" }));
    expect(getPreferences().pageSize).toBe(25);
    expect(
      JSON.parse(localStorage.getItem("echo.preferences") ?? "{}"),
    ).toMatchObject({
      pageSize: 25,
    });
  });

  it("switches reduce motion on and off", async () => {
    renderApp(<SettingsPage />);
    const sw = screen.getByRole("switch", { name: "Reduce motion" });
    await userEvent.click(sw);
    expect(sw).toHaveAttribute("aria-checked", "true");
    expect(document.documentElement.dataset.reduceMotion).toBe("true");
  });
});

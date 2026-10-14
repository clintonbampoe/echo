import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import { LandingPage } from "./LandingPage";

const renderPage = () =>
  render(
    <MemoryRouter>
      <LandingPage />
    </MemoryRouter>,
  );

describe("LandingPage", () => {
  it("asks a signed-out visitor to sign in or register", () => {
    renderPage();
    const nav = screen.getByRole("navigation", { name: "Main" });
    expect(within(nav).getByRole("link", { name: "Sign in" })).toHaveAttribute(
      "href",
      "/signin",
    );
    expect(
      within(nav).getByRole("link", { name: "Get started" }),
    ).toHaveAttribute("href", "/signup");
    expect(
      screen.getAllByRole("link", { name: /Create your church account/ })[0],
    ).toHaveAttribute("href", "/signup");
  });

  it("offers a signed-in person a way straight back into the app", () => {
    saveTokens({
      accessToken: "a",
      accessTokenExpiresAt: "2099-01-01T00:00:00Z",
      refreshToken: "r",
      refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
    });
    renderPage();
    const nav = screen.getByRole("navigation", { name: "Main" });
    expect(
      within(nav).getByRole("link", { name: "Open Echo" }),
    ).toHaveAttribute("href", "/dashboard");
    expect(
      within(nav).queryByRole("link", { name: "Sign in" }),
    ).not.toBeInTheDocument();
  });

  it("does not treat an expired session as signed in", () => {
    saveTokens({
      accessToken: "a",
      accessTokenExpiresAt: "2000-01-01T00:00:00Z",
      refreshToken: "r",
      refreshTokenExpiresAt: "2000-01-01T00:00:00Z",
    });
    renderPage();
    expect(
      screen.queryByRole("link", { name: "Open Echo" }),
    ).not.toBeInTheDocument();
  });

  it("switches the product screen with the tabs and with the arrow keys", async () => {
    renderPage();
    const attendance = screen.getByRole("tab", { name: "Attendance" });
    expect(attendance).toHaveAttribute("aria-selected", "true");
    expect(screen.getByRole("tabpanel", { name: "Attendance" })).toBeVisible();

    await userEvent.click(screen.getByRole("tab", { name: "Finance" }));
    expect(screen.getByRole("tabpanel", { name: "Finance" })).toBeVisible();
    expect(
      screen.queryByRole("tabpanel", { name: "Attendance" }),
    ).not.toBeInTheDocument();

    await userEvent.keyboard("{ArrowRight}");
    expect(screen.getByRole("tab", { name: "Projects" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
  });

  it("shows the verse and its reference", () => {
    renderPage();
    expect(screen.getByText("Matthew 18:20")).toBeInTheDocument();
  });
});

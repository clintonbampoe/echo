import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { clearTokens, saveTokens } from "@/lib/tokens";
import { websiteRoutes } from "./routes";

const tokens = {
  accessToken: "a",
  accessTokenExpiresAt: "2099-01-01T00:00:00Z",
  refreshToken: "r",
  refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
};

function renderAt(path: string) {
  const router = createMemoryRouter(websiteRoutes, { initialEntries: [path] });
  render(<RouterProvider router={router} />);
  return router;
}

beforeEach(() => {
  clearTokens();
  localStorage.clear();
  // jsdom does not scroll.
  window.scrollTo = vi.fn();
});

describe("the website frame", () => {
  it("leads the home page with the promise", async () => {
    renderAt("/");
    expect(
      await screen.findByRole("heading", {
        level: 1,
        name: /Your church,\s*answered\./,
      }),
    ).toBeInTheDocument();
  });

  it("links the main pages from the header", async () => {
    renderAt("/");
    const nav = await screen.findByRole("navigation", { name: "Main" });
    for (const [name, href] of [
      ["Features", "/features"],
      ["Docs", "/docs"],
      ["Roadmap", "/roadmap"],
      ["Community", "/community"],
    ]) {
      expect(within(nav).getByRole("link", { name })).toHaveAttribute(
        "href",
        href,
      );
    }
  });

  it("offers sign in and get started to someone who is signed out", async () => {
    renderAt("/");
    expect(
      await screen.findByRole("link", { name: "Sign in" }),
    ).toHaveAttribute("href", "/signin");
    expect(screen.getByRole("link", { name: "Get started" })).toHaveAttribute(
      "href",
      "/signup",
    );
  });

  it("offers Open Echo to someone who is signed in", async () => {
    saveTokens(tokens);
    renderAt("/");
    expect(
      await screen.findByRole("link", { name: "Open Echo" }),
    ).toHaveAttribute("href", "/dashboard");
    expect(
      screen.queryByRole("link", { name: "Sign in" }),
    ).not.toBeInTheDocument();
  });

  it("does not treat an expired session as signed in", async () => {
    saveTokens({ ...tokens, refreshTokenExpiresAt: "2000-01-01T00:00:00Z" });
    renderAt("/");
    expect(
      await screen.findByRole("link", { name: "Sign in" }),
    ).toBeInTheDocument();
  });

  it("lets the person switch between light and dark and remembers it", async () => {
    renderAt("/");
    await userEvent.click(
      await screen.findByRole("button", {
        name: /switch to (dark|light) mode/i,
      }),
    );
    const chosen = localStorage.getItem("echo-theme");
    expect(chosen === "light" || chosen === "dark").toBe(true);
    expect(document.documentElement.dataset.surface).toBe(chosen);
  });
});

describe("every page", () => {
  it.each([
    ["/features", /Everything a church\s*runs on\./],
    ["/docs", /How can we\s*help\?/],
    ["/roadmap", /Where Echo\s*is going\./],
    ["/changelog", /What is new\./],
    ["/community", /Built in the open,\s*with you\./],
    ["/about", /Quiet software for\s*loud communities\./],
    ["/contact", /Talk to a human\./],
    ["/security", /Trusted with\s*your people\./],
    ["/privacy", /Privacy policy/],
    ["/terms", /Terms of service/],
  ])("%s has its title", async (path, title) => {
    renderAt(path);
    expect(
      await screen.findByRole("heading", { level: 1, name: title }),
    ).toBeInTheDocument();
  });
});

describe("docs", () => {
  it("narrows the topics as the person types", async () => {
    renderAt("/docs");
    expect(await screen.findByText("Self-hosting")).toBeInTheDocument();
    await userEvent.type(screen.getByRole("searchbox"), "tithe");
    expect(screen.queryByText("Self-hosting")).not.toBeInTheDocument();
    expect(screen.getByText("How do I record a tithe?")).toBeInTheDocument();
  });
});

describe("contact", () => {
  it("asks for the missing details before opening the email app", async () => {
    renderAt("/contact");
    await userEvent.click(
      await screen.findByRole("button", { name: "Send message" }),
    );
    expect(await screen.findByText("Enter your name.")).toBeInTheDocument();
    expect(screen.getByText("Write a message.")).toBeInTheDocument();
  });
});

import { render, screen, waitFor, within } from "@testing-library/react";
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
      ["About", "/about"],
      ["Community", "/community"],
      ["Pricing", "/pricing"],
      ["Security", "/security"],
      ["Terms", "/terms"],
      ["Support Echo", "/contact?topic=support"],
    ]) {
      expect(within(nav).getByRole("link", { name })).toHaveAttribute(
        "href",
        href,
      );
    }
  });

  it("offers only Get started and the light/dark button to someone who is signed out", async () => {
    renderAt("/");
    const header = within(await screen.findByRole("banner"));
    expect(
      header.queryByRole("link", { name: "Sign in" }),
    ).not.toBeInTheDocument();
    expect(
      header.getByRole("button", { name: /Switch to dark mode/ }),
    ).toBeInTheDocument();
    expect(header.getByRole("link", { name: "Get started" })).toHaveAttribute(
      "href",
      "/signup",
    );
  });

  it("folds every page and Get started into the open menu", async () => {
    renderAt("/");
    await userEvent.click(await screen.findByRole("button", { name: "Menu" }));
    // The header's own links and the menu's copy of them.
    const header = within(screen.getByRole("banner"));
    expect(header.getAllByRole("link", { name: "Get started" })).toHaveLength(
      2,
    );
    expect(header.getAllByRole("link", { name: "Pricing" })).toHaveLength(2);
  });

  it("closes the open menu with Escape or a tap outside it", async () => {
    renderAt("/");
    const menu = await screen.findByRole("button", { name: "Menu" });
    await userEvent.click(menu);
    expect(menu).toHaveAttribute("aria-expanded", "true");
    await userEvent.keyboard("{Escape}");
    expect(menu).toHaveAttribute("aria-expanded", "false");

    await userEvent.click(menu);
    await userEvent.click(screen.getByRole("button", { name: "Close menu" }));
    expect(menu).toHaveAttribute("aria-expanded", "false");
    expect(screen.queryByRole("button", { name: "Close menu" })).toBeNull();
  });

  it("tells the story from idea to every church in six steps", async () => {
    renderAt("/community");
    expect(
      await screen.findByRole("heading", {
        level: 2,
        name: "From your idea to every church.",
      }),
    ).toBeInTheDocument();
    expect(screen.getAllByText(/^Step \d of 6$/)).toHaveLength(6);
  });

  it("steps through the story with the arrow keys and lets go at the ends", async () => {
    vi.stubGlobal("matchMedia", () => ({
      matches: false,
      addEventListener: () => {},
      removeEventListener: () => {},
    }));
    renderAt("/community");
    const story = await screen.findByRole("group", {
      name: /How a change reaches every church/,
    });
    Element.prototype.scrollTo = vi.fn();
    story.focus();
    // At the first step, going back is left to the page.
    const back = new KeyboardEvent("keydown", {
      key: "ArrowUp",
      bubbles: true,
      cancelable: true,
    });
    story.dispatchEvent(back);
    expect(back.defaultPrevented).toBe(false);
    // Going forward is taken, so the page does not scroll as well.
    const forward = new KeyboardEvent("keydown", {
      key: "ArrowDown",
      bubbles: true,
      cancelable: true,
    });
    story.dispatchEvent(forward);
    expect(forward.defaultPrevented).toBe(true);
  });

  it("folds a feature's points into one row on a phone", async () => {
    vi.stubGlobal("matchMedia", (q: string) => ({
      matches: q.includes("599"),
      addEventListener: () => {},
      removeEventListener: () => {},
    }));
    renderAt("/features");
    expect(
      (await screen.findAllByText("What it covers")).length,
    ).toBeGreaterThan(5);
  });

  it("offers Open Echo to someone who is signed in", async () => {
    saveTokens(tokens);
    renderAt("/");
    const header = within(await screen.findByRole("banner"));
    expect(header.getByRole("link", { name: "Open Echo" })).toHaveAttribute(
      "href",
      "/dashboard",
    );
    expect(
      header.queryByRole("link", { name: "Sign in" }),
    ).not.toBeInTheDocument();
  });

  it("does not treat an expired session as signed in", async () => {
    saveTokens({ ...tokens, refreshTokenExpiresAt: "2000-01-01T00:00:00Z" });
    renderAt("/");
    const header = within(await screen.findByRole("banner"));
    expect(
      header.getByRole("link", { name: "Get started" }),
    ).toBeInTheDocument();
  });
});

describe("every page", () => {
  it.each([
    ["/features", /Everything a church\s*runs on\./],
    ["/docs", /How can we\s*help\?/],
    ["/roadmap", /Where Echo\s*is going\./],
    ["/changelog", /What is new\./],
    ["/community", /Echo belongs to the churches that use it/],
    ["/about", /Built so your church can spend its time on people/],
    ["/contact", /Talk to us/],
    ["/security", /How we handle your data/],
    ["/privacy", /Privacy policy/],
    ["/terms", /Terms of use/],
  ])("%s has its title", async (path, title) => {
    renderAt(path);
    expect(
      await screen.findByRole("heading", { level: 1, name: title }),
    ).toBeInTheDocument();
  });
});

describe("terms", () => {
  it("lists the rules in order, and marks one as read once it is opened", async () => {
    renderAt("/terms");
    const row = (await screen.findByText("Your data")).closest("details");
    expect(row).not.toBeNull();
    expect(row).not.toHaveAttribute("data-read");
    row!.open = true;
    row!.dispatchEvent(new Event("toggle"));
    await waitFor(() => expect(row).toHaveAttribute("data-read"));
    expect(screen.getByText("02")).toBeInTheDocument();
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

import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { asked, json, renderCard, signIn } from "../../testing/dashboardPage";
import { GettingStarted } from "./GettingStarted";

type Done = {
  members?: number;
  present?: number;
  tithes?: number;
  invites?: number;
};
let role: string;
let done: Done;

// What the server says for each path, from what the test has set up.
function answer(path: string): unknown {
  const answers: Record<string, () => unknown> = {
    "/v1/members/summary": () => ({
      totalMembers: done.members ?? 0,
      ageGroups: [],
    }),
    "/v1/attendance/summary": () => ({
      totalPresent: done.present ?? 0,
      previousTotalPresent: 0,
    }),
    "/v1/tithes/summary": () => ({
      totalCollected: done.tithes ?? 0,
      previousTotalCollected: 0,
    }),
    "/v1/auth/invitations/summary": () => ({
      active: done.invites ?? 0,
      peopleJoined: 0,
      expiringSoon: 0,
    }),
  };
  if (path.includes("/users/me"))
    return { id: "1", name: "Ama Mensah", email: "a@b.c", role };
  if (path.includes("/congregation")) return { id: "2", name: "Grace Chapel" };
  return answers[path]?.() ?? {};
}

beforeEach(() => {
  role = "Admin";
  done = {};
  localStorage.clear();
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => json(answer(asked(url).path))),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("getting started card", () => {
  it("lists the five steps, with the church done and the first member next", async () => {
    renderCard(<GettingStarted />);
    expect(await screen.findByText("1 of 5 done")).toBeInTheDocument();
    expect(screen.getAllByRole("listitem")).toHaveLength(5);
    expect(screen.getByRole("link", { name: "Add a member" })).toHaveAttribute(
      "href",
      "/members?new=1",
    );
    // The other steps that wait are pressable as a whole row.
    expect(
      screen.getByRole("link", { name: /Record a tithe/ }),
    ).toHaveAttribute("href", "/tithes?new=1");
  });

  it("moves the filled button to the next step as things get done", async () => {
    done = { members: 12, present: 30 };
    renderCard(<GettingStarted />);
    expect(await screen.findByText("3 of 5 done")).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Add a member" })).toBeNull();
    expect(
      screen.getByRole("link", { name: "Record a tithe" }),
    ).toBeInTheDocument();
  });

  it("says the church is set up when all is done, and hides for good on this device", async () => {
    done = { members: 1, present: 1, tithes: 1, invites: 1 };
    const { unmount } = renderCard(<GettingStarted />);
    expect(await screen.findByText("You are set up")).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole("button", { name: "Hide this card" }),
    );
    expect(screen.queryByText("You are set up")).toBeNull();
    unmount();
    renderCard(<GettingStarted />);
    await new Promise((r) => setTimeout(r, 50));
    expect(screen.queryByText("You are set up")).toBeNull();
  });

  it("is not shown to anyone but an admin", async () => {
    role = "Clerk";
    renderCard(<GettingStarted />);
    await new Promise((r) => setTimeout(r, 50));
    expect(screen.queryByText("Get your church set up")).toBeNull();
    expect(
      vi
        .mocked(fetch)
        .mock.calls.some(([u]) => String(u).includes("invitations")),
    ).toBe(false);
  });
});

import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, pathOf, renderApp, signIn } from "@/test/kit";
import { InvitationsPage } from "./InvitationsPage";

const row = {
  id: "i1",
  label: "Youth team",
  allowedRole: "Clerk",
  createdByName: "Kwabena",
  createdAt: "2026-10-20T00:00:00Z",
  expiresAt: "2099-01-01T00:00:00Z",
  status: "Active",
  useCount: 3,
};

beforeEach(signIn);
afterEach(() => vi.unstubAllGlobals());

function serve() {
  const fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const path = pathOf(url);
    if (path === "/v1/auth/invitations" && init?.method === "POST")
      return json({
        id: "i2",
        token: "GRACE-1234",
        allowedRole: "Clerk",
        expiresAt: "2099-01-01T00:00:00Z",
      });
    if (path === "/v1/auth/invitations")
      return json({ data: [row], hasMore: false, next: null });
    return json({});
  });
  vi.stubGlobal("fetch", fetchMock);
  return fetchMock;
}

describe("InvitationsPage", () => {
  it("lists invitations with their usage", async () => {
    serve();
    renderApp(<InvitationsPage />);
    expect(await screen.findByText("Youth team")).toBeInTheDocument();
    expect(screen.getByText(/used 3 times/)).toBeInTheDocument();
    expect(screen.getByText("Admin only")).toBeInTheDocument();
  });

  it("creates an invitation and shows the code once", async () => {
    const fetchMock = serve();
    renderApp(<InvitationsPage />);
    await screen.findByText("Youth team");
    await userEvent.click(
      screen.getByRole("button", { name: "Invite people" }),
    );
    await userEvent.click(
      await screen.findByRole("button", { name: "Create invitation" }),
    );
    expect(await screen.findByText("GRACE-1234")).toBeInTheDocument();
    expect(
      fetchMock.mock.calls.some(
        ([, i]) => (i as RequestInit | undefined)?.method === "POST",
      ),
    ).toBe(true);
  });
});

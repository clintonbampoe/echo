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
  code: "K7QX-92MD-PW4T",
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
    if (path === "/v1/auth/invitations/summary")
      return json({ active: 4, peopleJoined: 21, expiringSoon: 2 });
    if (path === "/v1/auth/invitations/i1") return json(row);
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

  it("opens an invitation to show its code, link and details", async () => {
    const fetchMock = serve();
    renderApp(<InvitationsPage />);
    await userEvent.click(
      await screen.findByRole("button", { name: /^Youth team/ }),
    );
    expect(await screen.findByLabelText("Invitation code")).toHaveTextContent(
      "K7QX-92MD-PW4T",
    );
    expect(screen.getByText(/\/join\/K7QX-92MD-PW4T$/)).toBeInTheDocument();
    expect(screen.getByText("Kwabena")).toBeInTheDocument();
    expect(
      fetchMock.mock.calls.some(([url]) =>
        pathOf(url as string).endsWith("/invitations/i1"),
      ),
    ).toBe(true);
  });

  it("shows the server's summary figures", async () => {
    serve();
    renderApp(<InvitationsPage />);
    expect(await screen.findByText("21")).toBeInTheDocument();
  });

  it("creates an invitation with a limit and no expiry", async () => {
    const fetchMock = serve();
    renderApp(<InvitationsPage />);
    await screen.findByText("Youth team");
    await userEvent.click(
      screen.getByRole("button", { name: "Invite people" }),
    );
    await userEvent.type(await screen.findByLabelText(/Limit/), "25");
    await userEvent.click(
      await screen.findByRole("button", { name: "Create invitation" }),
    );
    await screen.findByText("GRACE-1234");
    const post = fetchMock.mock.calls.find(
      ([, i]) => (i as RequestInit | undefined)?.method === "POST",
    )!;
    expect(JSON.parse((post[1] as RequestInit).body as string)).toMatchObject({
      allowedRole: "Clerk",
      expiryDays: 7,
      maxUses: 25,
    });
  });

  it("creates an invitation and shows the code", async () => {
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

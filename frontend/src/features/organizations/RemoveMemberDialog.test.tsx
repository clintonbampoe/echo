import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import type { Organization, OrganizationMember } from "./api";
import { RemoveMemberDialog } from "./RemoveMemberDialog";

const choir: Organization = {
  id: "o1",
  name: "Church Choir",
  createdAt: "2016-02-03T09:00:00Z",
  memberCount: 61,
};
const comfort: OrganizationMember = {
  id: "om2",
  memberId: "m2",
  memberName: "Comfort Addo",
  organizationId: "o1",
  organizationName: "Church Choir",
  role: "Secretary",
  joinedAt: "2018-03-11",
  createdAt: "2018-03-11",
};

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async () => new Response(null, { status: 204 }));
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const wrap = (onClose = () => {}) =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <RemoveMemberDialog
        organization={choir}
        member={comfort}
        onClose={onClose}
      />
    </QueryClientProvider>,
  );

describe("remove member dialog", () => {
  it("names the person and the group, and says they stay in the church", () => {
    wrap();
    expect(
      screen.getByText("Remove Comfort Addo from Church Choir?"),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/They stay a member of the church/),
    ).toBeInTheDocument();
  });

  it("keeps them in the group when asked to", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(onClose);
    await user.click(screen.getByRole("button", { name: "Keep in group" }));
    expect(onClose).toHaveBeenCalled();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("removes the membership, not the person, then closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(onClose);
    await user.click(screen.getByRole("button", { name: "Remove from group" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(init.method).toBe("DELETE");
    expect(url).toContain("/v1/organization-members/om2");
  });
});

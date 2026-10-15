import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import type { Organization } from "./api";
import { DeleteOrganizationDialog } from "./DeleteOrganizationDialog";

const choir: Organization = {
  id: "o1",
  name: "Church Choir",
  createdAt: "2016-02-03T09:00:00Z",
  memberCount: 61,
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

const wrap = (ui: React.ReactElement) =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      {ui}
    </QueryClientProvider>,
  );

const deleted = () =>
  fetchMock.mock.calls.some(
    ([, i]) => (i as RequestInit | undefined)?.method === "DELETE",
  );

describe("delete organization dialog", () => {
  it("says how many people stay in the church", () => {
    wrap(<DeleteOrganizationDialog organization={choir} onClose={() => {}} />);
    expect(screen.getByText("Delete Church Choir?")).toBeInTheDocument();
    expect(
      screen.getByText(/Its 61 members stay in the church/),
    ).toBeInTheDocument();
  });

  it("uses the singular for a group of one", () => {
    wrap(
      <DeleteOrganizationDialog
        organization={{ ...choir, memberCount: 1 }}
        onClose={() => {}}
      />,
    );
    expect(
      screen.getByText(/Its 1 member stays in the church/),
    ).toBeInTheDocument();
  });

  it("keeps the organization when asked to", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<DeleteOrganizationDialog organization={choir} onClose={onClose} />);
    expect(
      screen.getByRole("button", { name: "Keep organization" }),
    ).toHaveFocus();
    await user.click(screen.getByRole("button", { name: "Keep organization" }));
    expect(onClose).toHaveBeenCalled();
    expect(deleted()).toBe(false);
  });

  it("deletes, closes and tells the page", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    const onDeleted = vi.fn();
    wrap(
      <DeleteOrganizationDialog
        organization={choir}
        onClose={onClose}
        onDeleted={onDeleted}
      />,
    );
    await user.click(
      screen.getByRole("button", { name: "Delete organization" }),
    );
    await waitFor(() => expect(onDeleted).toHaveBeenCalled());
    expect(onClose).toHaveBeenCalled();
    expect(
      fetchMock.mock.calls.some(
        ([u, i]) =>
          (i as RequestInit).method === "DELETE" &&
          String(u).includes("/v1/organizations/o1"),
      ),
    ).toBe(true);
  });
});

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import { AddMemberSheet } from "./AddMemberSheet";
import type { Organization } from "../../api/organizations";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });
const choir: Organization = {
  id: "o1",
  name: "Church Choir",
  createdAt: "2016-02-03T09:00:00Z",
  memberCount: 61,
};
const found = [
  { id: "m1", name: "Kofi Mensah", phoneNumber: "024 412 0098" },
  { id: "m2", name: "Kojo Antwi", phoneNumber: "020 551 7733" },
];

let post: (() => Response) | null;
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  post = () => json({ id: "om1" });
  fetchMock = vi.fn(async (_url: string, init?: RequestInit) =>
    init?.method === "POST" ? post!() : json(found),
  );
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
      <AddMemberSheet
        organization={choir}
        taken={new Set(["m1"])}
        onClose={onClose}
      />
    </QueryClientProvider>,
  );

async function pickKojo(user: ReturnType<typeof userEvent.setup>) {
  await user.type(
    screen.getByRole("searchbox", { name: "Search members" }),
    "ko",
  );
  await user.click(await screen.findByRole("button", { name: /Kojo Antwi/ }));
}

const posted = () =>
  fetchMock.mock.calls.find(
    ([, i]) => (i as RequestInit | undefined)?.method === "POST",
  ) as [string, RequestInit] | undefined;

describe("add member sheet", () => {
  it("asks for a member and sends nothing until one is chosen", async () => {
    const user = userEvent.setup();
    wrap();
    await user.click(screen.getByRole("button", { name: "Add member" }));
    expect(await screen.findByText("Choose a member")).toBeInTheDocument();
    expect(posted()).toBeUndefined();
  });

  it("adds the chosen member with the chosen role, then closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(onClose);
    await pickKojo(user);
    await user.click(screen.getByRole("radio", { name: "Secretary" }));
    await user.click(screen.getByRole("button", { name: "Add member" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const [url, init] = posted()!;
    expect(url).toContain("/v1/organization-members");
    expect(JSON.parse(init.body as string)).toMatchObject({
      memberId: "m2",
      organizationId: "o1",
      role: "Secretary",
    });
  });

  it("says so, and stays open, when they were added by someone else a moment ago", async () => {
    post = () => json({ title: "Conflict", status: 409 }, 409);
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(onClose);
    await pickKojo(user);
    await user.click(screen.getByRole("button", { name: "Add member" }));
    expect(
      await screen.findByText("Kojo Antwi is already in Church Choir"),
    ).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });

  it("shows a plain message, not the raw error, for any other failure", async () => {
    post = () => json({ title: "Boom", status: 500 }, 500);
    const user = userEvent.setup();
    wrap();
    await pickKojo(user);
    await user.click(screen.getByRole("button", { name: "Add member" }));
    expect(
      await screen.findByText("We could not add this member"),
    ).toBeInTheDocument();
  });
});

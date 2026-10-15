import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import type { Organization } from "./api";
import { EditOrganizationSheet } from "./EditOrganizationSheet";
import { OrganizationSheet } from "./OrganizationSheet";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const choir: Organization = {
  id: "o1",
  name: "Church Choir",
  description: "Leads worship on Sundays.",
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
  fetchMock = vi.fn(async () => json(choir));
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

const bodyOf = () =>
  JSON.parse(
    (fetchMock.mock.calls[0] as [string, RequestInit])[1].body as string,
  );

describe("new organization sheet", () => {
  it("asks for a name and sends nothing until there is one", async () => {
    const user = userEvent.setup();
    wrap(<OrganizationSheet open onClose={() => {}} />);
    await user.click(
      screen.getByRole("button", { name: "Create organization" }),
    );
    expect(await screen.findByText("Enter a name")).toBeInTheDocument();
    expect(screen.getByText("Fix 1 field to continue")).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("sends only what was filled in, then closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<OrganizationSheet open onClose={onClose} />);
    await user.type(screen.getByLabelText("Name"), "Media Team");
    await user.click(
      screen.getByRole("button", { name: "Create organization" }),
    );
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(bodyOf()).toEqual({ name: "Media Team" });
  });

  it("counts the description as it is typed", async () => {
    const user = userEvent.setup();
    wrap(<OrganizationSheet open onClose={() => {}} />);
    await user.type(screen.getByLabelText("Description"), "Sound");
    expect(screen.getByText("5 / 2000")).toBeInTheDocument();
  });

  it("shows what the server says is wrong, not a raw error, and stays open", async () => {
    fetchMock.mockImplementation(async () =>
      json({ title: "Boom", status: 500 }, 500),
    );
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<OrganizationSheet open onClose={onClose} />);
    await user.type(screen.getByLabelText("Name"), "Media Team");
    await user.click(
      screen.getByRole("button", { name: "Create organization" }),
    );
    expect(
      await screen.findByText("We could not save this organization"),
    ).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });
});

describe("edit organization sheet", () => {
  it("starts with what the group has and saves a change", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<EditOrganizationSheet organization={choir} onClose={onClose} />);
    expect(screen.getByLabelText("Name")).toHaveValue("Church Choir");
    expect(screen.getByLabelText("Description")).toHaveValue(
      "Leads worship on Sundays.",
    );
    await user.clear(screen.getByLabelText("Name"));
    await user.type(screen.getByLabelText("Name"), "Praise Choir");
    expect(screen.getByText("Unsaved changes")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toContain("/v1/organizations/o1");
    expect(init.method).toBe("PUT");
    expect(bodyOf()).toMatchObject({ name: "Praise Choir" });
  });
});

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import type { Visitor } from "./api";
import { EditVisitorSheet } from "./EditVisitorSheet";
import { VisitorSheet } from "./VisitorSheet";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const esi: Visitor = {
  id: "v1",
  name: "Esi Mensah",
  firstName: "Esi",
  lastName: "Mensah",
  phoneNumber: "055 212 3344",
  createdAt: "2026-09-20T09:00:00Z",
  visitCount: 3,
};

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async () => json(esi));
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

describe("new visitor sheet", () => {
  it("asks for a name and sends nothing until there is one", async () => {
    const user = userEvent.setup();
    wrap(<VisitorSheet open onClose={() => {}} />);
    await user.click(screen.getByRole("button", { name: "Create visitor" }));
    expect(await screen.findByText("Enter a first name")).toBeInTheDocument();
    expect(screen.getByText("Enter a last name")).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("sends only what was filled in, then closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<VisitorSheet open onClose={onClose} />);
    await user.type(screen.getByLabelText("First name"), "Esi");
    await user.type(screen.getByLabelText("Last name"), "Mensah");
    await user.type(screen.getByLabelText("Notes"), "Came with Abena.");
    await user.click(screen.getByRole("button", { name: "Create visitor" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(bodyOf()).toEqual({
      firstName: "Esi",
      lastName: "Mensah",
      notes: "Came with Abena.",
    });
  });

  it("counts the notes as they are typed", async () => {
    const user = userEvent.setup();
    wrap(<VisitorSheet open onClose={() => {}} />);
    await user.type(screen.getByLabelText("Notes"), "Hello");
    expect(screen.getByText("5 / 2000")).toBeInTheDocument();
  });
});

describe("edit visitor sheet", () => {
  it("starts with what the visitor has and saves a change", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<EditVisitorSheet visitor={esi} onClose={onClose} />);
    expect(screen.getByLabelText("First name")).toHaveValue("Esi");
    expect(screen.getByLabelText("Phone")).toHaveValue("055 212 3344");
    await user.clear(screen.getByLabelText("Phone"));
    await user.type(screen.getByLabelText("Phone"), "024 887 1102");
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(bodyOf()).toMatchObject({ phoneNumber: "024 887 1102" });
  });
});

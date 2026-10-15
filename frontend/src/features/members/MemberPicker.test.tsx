import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { saveTokens } from "@/lib/tokens";
import type { MemberMatch } from "./api";
import { MemberPicker } from "./MemberPicker";

const found: MemberMatch[] = [
  { id: "m1", name: "Kofi Mensah", phoneNumber: "024 412 0098" },
  { id: "m2", name: "Kojo Antwi", phoneNumber: "020 551 7733" },
];

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(
    async () => new Response(JSON.stringify(found), { status: 200 }),
  );
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

function Harness({ taken }: { taken?: ReadonlySet<string> }) {
  const [value, setValue] = useState<MemberMatch | null>(null);
  return (
    <MemberPicker
      value={value}
      onChange={setValue}
      taken={taken}
      takenText="Already in this group"
    />
  );
}

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

describe("member picker", () => {
  it("asks for two letters and searches nothing before that", async () => {
    const user = userEvent.setup();
    wrap(<Harness />);
    expect(screen.getByText("Type at least two letters.")).toBeInTheDocument();
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "k",
    );
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("lists matches, picks one and can change the pick", async () => {
    const user = userEvent.setup();
    wrap(<Harness />);
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "ko",
    );
    await user.click(await screen.findByRole("button", { name: /Kojo Antwi/ }));
    expect(screen.getByText("Kojo Antwi")).toBeInTheDocument();
    expect(screen.queryByRole("searchbox")).toBeNull();
    await user.click(screen.getByRole("button", { name: "Change" }));
    expect(
      screen.getByRole("searchbox", { name: "Search members" }),
    ).toBeInTheDocument();
  });

  it("marks people who cannot be picked and does not let them be", async () => {
    const user = userEvent.setup();
    wrap(<Harness taken={new Set(["m1"])} />);
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "ko",
    );
    const taken = await screen.findByRole("button", { name: /Kofi Mensah/ });
    expect(taken).toBeDisabled();
    expect(screen.getByText("Already in this group")).toBeInTheDocument();
  });

  it("says when nobody matches", async () => {
    fetchMock.mockImplementation(
      async () => new Response("[]", { status: 200 }),
    );
    const user = userEvent.setup();
    wrap(<Harness />);
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "zz",
    );
    expect(await screen.findByText(/No member found/)).toBeInTheDocument();
  });

  it("says so, without the raw error, when the search fails", async () => {
    fetchMock.mockImplementation(
      async () =>
        new Response(JSON.stringify({ title: "Boom" }), { status: 500 }),
    );
    const user = userEvent.setup();
    wrap(<Harness />);
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "ko",
    );
    expect(
      await screen.findByText(/We could not search right now/),
    ).toBeInTheDocument();
    expect(screen.queryByText("Boom")).toBeNull();
  });
});

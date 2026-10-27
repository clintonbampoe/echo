import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import { aCheckIn, aSession } from "../../testing/fixtures";
import { CameLastTime } from "./CameLastTime";

const json = (body: unknown) => new Response(JSON.stringify(body));
const page = (data: unknown[]) => json({ data, hasMore: false, next: null });

const previous = aSession({ id: "s0", date: "2026-10-11" });
const today = aSession({ id: "s1", date: "2026-10-18", status: "Scheduled" });
const came = [
  aCheckIn({ id: "c1", personId: "m1", personName: "Kwame Asante" }),
  aCheckIn({ id: "c2", personId: "m2", personName: "Esi Owusu" }),
  aCheckIn({ id: "c3", personId: "m3", personName: "Yaw Darko" }),
];

beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) =>
      new URL(url, "http://x").pathname.endsWith("/sessions")
        ? page([previous])
        : page(came),
    ),
  );
});
afterEach(() => vi.unstubAllGlobals());

const draw = (present: string[] = []) => {
  const onMark = vi.fn();
  const onMarkAll = vi.fn();
  render(
    <QueryClientProvider client={new QueryClient()}>
      <CameLastTime
        session={today}
        kind="Member"
        present={new Set(present)}
        busy={false}
        onMark={onMark}
        onMarkAll={onMarkAll}
      />
    </QueryClientProvider>,
  );
  return { onMark, onMarkAll };
};

describe("came last time", () => {
  it("says where the list is from and offers each person who is not here yet", async () => {
    draw(["m2"]);
    expect(
      await screen.findByRole("heading", { name: "Came last time" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("At Sunday Service on 11 Oct and not marked yet."),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Mark Kwame Asante present" }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Mark Esi Owusu present" }),
    ).toBeNull();
  });

  it("marks one person, or everyone listed with Mark all", async () => {
    const user = userEvent.setup();
    const { onMark, onMarkAll } = draw(["m2"]);
    await user.click(
      await screen.findByRole("button", { name: "Mark Yaw Darko present" }),
    );
    expect(onMark).toHaveBeenCalledWith({ id: "m3", name: "Yaw Darko" });
    await user.click(screen.getByRole("button", { name: "Mark all 2" }));
    expect(onMarkAll).toHaveBeenCalledWith([
      { id: "m1", name: "Kwame Asante" },
      { id: "m3", name: "Yaw Darko" },
    ]);
  });

  it("shows nothing when everyone is already here", async () => {
    draw(["m1", "m2", "m3"]);
    await waitFor(() => expect(fetch).toHaveBeenCalledTimes(2));
    expect(
      screen.queryByRole("heading", { name: "Came last time" }),
    ).toBeNull();
  });
});

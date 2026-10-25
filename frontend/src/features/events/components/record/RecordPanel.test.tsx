import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import { RecordPanel } from "./RecordPanel";
import type { EventItem } from "../../api/events";
import { anEvent } from "../../testing/fixtures";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const retreat = anEvent({
  id: "e1",
  name: "Prayer Retreat",
  organizationId: "o1",
  organizationName: "Prayer Warriors",
  organizerId: "m7",
  organizerName: "Efua Quaye",
  startDate: "2099-10-03",
  endDate: "2099-10-03",
  startTime: null,
  endTime: null,
  location: null,
  capacity: 60,
  description: null,
  createdAt: "2020-01-01T00:00:00Z",
  registrationCount: 47,
});
const matches = [
  { id: "m1", name: "Kofi Mensah", phoneNumber: "024 774 1305" },
  { id: "m2", name: "Kojo Antwi", phoneNumber: "024 887 1102" },
];

let post: (body: Record<string, unknown>) => Response;
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  post = () => json({}, 201);
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (init?.method === "POST")
      return post(JSON.parse(init.body as string) as Record<string, unknown>);
    if (new URL(url, "http://x").pathname.endsWith("/members/search"))
      return json(matches);
    return json({});
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = (
  event: EventItem = retreat,
  registered: string[] = [],
  checkedIn: string[] = [],
) =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter>
        <RecordPanel
          event={event}
          initial="register"
          registered={new Set(registered)}
          checkedIn={new Set(checkedIn)}
        />
      </MemoryRouter>
    </QueryClientProvider>,
  );

const search = async (user: ReturnType<typeof userEvent.setup>, text = "ko") =>
  user.type(screen.getByRole("searchbox", { name: "Search members" }), text);
const posted = () =>
  fetchMock.mock.calls.find(
    ([, i]) => (i as RequestInit | undefined)?.method === "POST",
  ) as [string, RequestInit] | undefined;

describe("register or check in", () => {
  it("registers a member with a tap, marks them, and counts them", async () => {
    const user = userEvent.setup();
    open(retreat, ["m1"]);
    await search(user);
    expect(await screen.findByText("Already registered")).toBeInTheDocument();
    await user.click(
      await screen.findByRole("button", { name: "Register Kojo Antwi" }),
    );
    expect(
      await screen.findByText("1 registered just now"),
    ).toBeInTheDocument();
    expect(screen.getByText("Registered")).toBeInTheDocument();
    expect(posted()?.[0]).toContain("/v1/event-registrations");
    expect(JSON.parse(posted()?.[1].body as string)).toMatchObject({
      memberId: "m2",
      eventId: "e1",
    });
  });

  it("checks a member in, whether or not they signed up", async () => {
    const user = userEvent.setup();
    open(retreat, [], ["m1"]);
    await user.click(screen.getByRole("radio", { name: "Check in" }));
    await search(user);
    expect(await screen.findByText("Already checked in")).toBeInTheDocument();
    expect(
      screen.getByText(
        "Not registered? You can still check them in. They show as a walk-in.",
      ),
    ).toBeInTheDocument();
    await user.click(
      await screen.findByRole("button", { name: "Check in Kojo Antwi" }),
    );
    expect(
      await screen.findByText("1 checked in just now"),
    ).toBeInTheDocument();
    expect(posted()?.[0]).toContain("/v1/event-attendance");
    expect(JSON.parse(posted()?.[1].body as string)).toMatchObject({
      memberId: "m2",
      eventId: "e1",
      checkInTime: expect.stringMatching(/^\d\d:\d\d:00$/),
    });
  });

  it("closes registration when every spot is taken, but still allows check in", async () => {
    const user = userEvent.setup();
    open({ ...retreat, registrationCount: 60 });
    expect(screen.getByText("Registration is closed.")).toBeInTheDocument();
    expect(screen.getByText(/All 60 spots are taken/)).toBeInTheDocument();
    await search(user);
    expect(
      await screen.findByRole("button", { name: "Register Kojo Antwi" }),
    ).toBeDisabled();
    expect(
      screen.getByText("Check in still works while registration is closed."),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("radio", { name: "Check in" }));
    expect(screen.queryByText("Registration is closed.")).toBeNull();
    expect(
      await screen.findByRole("button", { name: "Check in Kojo Antwi" }),
    ).toBeEnabled();
  });

  it("says so when the event fills up while someone is registering", async () => {
    post = () => json({ errorCode: "EVENT_FULL", detail: "Full" }, 409);
    const user = userEvent.setup();
    open();
    await search(user);
    await user.click(
      await screen.findByRole("button", { name: "Register Kojo Antwi" }),
    );
    expect(
      await screen.findByText("Registration is closed"),
    ).toBeInTheDocument();
    await waitFor(() =>
      expect(screen.getByText("Nothing recorded yet")).toBeInTheDocument(),
    );
  });

  it("asks for two letters before it searches", () => {
    open();
    expect(screen.getByText("Type at least two letters.")).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });
});

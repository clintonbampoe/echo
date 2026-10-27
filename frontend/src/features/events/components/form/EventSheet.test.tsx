import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import { pickDate, pickTime } from "@/test/pickers";
import { DeleteEventDialog } from "../dialogs/DeleteEventDialog";
import { EditEventSheet } from "./EditEventSheet";
import { EventSheet } from "./EventSheet";
import { anEvent } from "../../testing/fixtures";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const rally = anEvent({
  id: "e1",
  name: "Youth Rally",
  organizationId: "o1",
  organizationName: "Youth Ministry",
  organizerId: "m1",
  organizerName: "Ama Mensah",
  startDate: "2026-11-14",
  endDate: "2026-11-14",
  startTime: "17:00:00",
  location: "Main Auditorium",
  capacity: 150,
  description: "Praise night.",
  createdAt: "2026-01-01T00:00:00Z",
  registrationCount: 84,
});

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/organizations"))
      return json({
        data: [
          {
            id: "o1",
            name: "Youth Ministry",
            createdAt: "2016-02-03T09:00:00Z",
            memberCount: 4,
          },
        ],
        hasMore: false,
        next: null,
      });
    if (u.pathname.endsWith("/members/search"))
      return json([
        { id: "m1", name: "Ama Mensah", phoneNumber: "0244000111" },
      ]);
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    return json(rally);
  });
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

const sent = (method: string) => {
  const call = fetchMock.mock.calls.find(
    ([, i]) => (i as RequestInit | undefined)?.method === method,
  ) as [string, RequestInit] | undefined;
  return {
    url: call?.[0] ?? "",
    body: call?.[1].body
      ? (JSON.parse(call[1].body as string) as Record<string, unknown>)
      : undefined,
  };
};

describe("new event sheet", () => {
  it("says what is missing and sends nothing until it is there", async () => {
    const user = userEvent.setup();
    wrap(<EventSheet open onClose={() => {}} />);
    await user.click(screen.getByRole("button", { name: "Create event" }));
    expect(await screen.findByText("Enter a name")).toBeInTheDocument();
    expect(screen.getAllByText("Choose a group")).toHaveLength(2);
    expect(screen.getByText("Choose an organizer")).toBeInTheDocument();
    expect(screen.getByText("Fix 5 fields to continue")).toBeInTheDocument();
    expect(sent("POST").url).toBe("");
  });

  it("creates the event with what was filled in, then closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<EventSheet open onClose={onClose} />);
    await user.type(screen.getByLabelText("Name"), "Youth Rally");
    await user.click(screen.getByRole("combobox", { name: /Organization/ }));
    await user.click(
      await screen.findByRole("option", { name: "Youth Ministry" }),
    );
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "am",
    );
    await user.click(await screen.findByRole("button", { name: /Ama Mensah/ }));
    await pickDate(user, "Starts on", "2026-11-14");
    await pickTime(user, "Starts at", "17:00");
    await user.type(screen.getByLabelText(/Capacity/), "150");
    await user.click(screen.getByRole("button", { name: "Create event" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(sent("POST").body).toEqual({
      organizationId: "o1",
      organizerId: "m1",
      name: "Youth Rally",
      startDate: "2026-11-14",
      endDate: "2026-11-14",
      startTime: "17:00:00",
      capacity: 150,
    });
  });

  it("stays open and says so when the server cannot save it", async () => {
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      const u = new URL(url, "http://x");
      if (u.pathname.endsWith("/organizations"))
        return json({
          data: [
            {
              id: "o1",
              name: "Youth Ministry",
              createdAt: "2016-02-03T09:00:00Z",
              memberCount: 4,
            },
          ],
          hasMore: false,
          next: null,
        });
      if (u.pathname.endsWith("/members/search"))
        return json([
          { id: "m1", name: "Ama Mensah", phoneNumber: "0244000111" },
        ]);
      return init?.method === "POST"
        ? json({ title: "Boom", status: 500 }, 500)
        : json(rally);
    });
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<EventSheet open onClose={onClose} />);
    await user.type(screen.getByLabelText("Name"), "Youth Rally");
    await user.click(screen.getByRole("combobox", { name: /Organization/ }));
    await user.click(
      await screen.findByRole("option", { name: "Youth Ministry" }),
    );
    await user.type(
      screen.getByRole("searchbox", { name: "Search members" }),
      "am",
    );
    await user.click(await screen.findByRole("button", { name: /Ama Mensah/ }));
    await pickDate(user, "Starts on", "2026-11-14");
    await user.click(screen.getByRole("button", { name: "Create event" }));
    expect(
      await screen.findByText("We could not save this event"),
    ).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });
});

describe("edit event sheet", () => {
  it("starts with what the event has and saves a change", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<EditEventSheet event={rally} onClose={onClose} />);
    expect(screen.getByLabelText("Name")).toHaveValue("Youth Rally");
    expect(screen.getByLabelText(/Capacity/)).toHaveValue("150");
    expect(screen.getByText("Ama Mensah")).toBeInTheDocument();
    await user.clear(screen.getByLabelText("Name"));
    await user.type(screen.getByLabelText("Name"), "Youth Night");
    await user.clear(screen.getByLabelText(/Location/));
    expect(screen.getByText("Unsaved changes")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const put = sent("PUT");
    expect(put.url).toContain("/v1/events/e1");
    expect(put.body).toMatchObject({
      name: "Youth Night",
      location: "",
      capacity: 150,
      startTime: "17:00:00",
    });
  });

  it("does not let the limit be taken away", async () => {
    const user = userEvent.setup();
    wrap(<EditEventSheet event={rally} onClose={() => {}} />);
    await user.clear(screen.getByLabelText(/Capacity/));
    await user.click(screen.getByRole("button", { name: "Save changes" }));
    expect(
      await screen.findByText("A limit can be changed, not removed"),
    ).toBeInTheDocument();
    expect(sent("PUT").url).toBe("");
  });
});

describe("delete event dialog", () => {
  it("says what is kept, with the counts when it knows them", () => {
    wrap(<DeleteEventDialog event={rally} checkIns={43} onClose={() => {}} />);
    expect(screen.getByText("Delete Youth Rally?")).toBeInTheDocument();
    expect(
      screen.getByText(/Its 84 registrations and 43 check-ins are kept/),
    ).toBeInTheDocument();
  });

  it("keeps the event when asked to", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<DeleteEventDialog event={rally} onClose={onClose} />);
    expect(
      screen.getByText(/Its registrations and check-ins are kept/),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Keep event" }));
    expect(onClose).toHaveBeenCalled();
    expect(sent("DELETE").url).toBe("");
  });

  it("deletes, closes and tells the page", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    const onDeleted = vi.fn();
    wrap(
      <DeleteEventDialog
        event={rally}
        onClose={onClose}
        onDeleted={onDeleted}
      />,
    );
    await user.click(screen.getByRole("button", { name: "Delete event" }));
    await waitFor(() => expect(onDeleted).toHaveBeenCalled());
    expect(onClose).toHaveBeenCalled();
    expect(sent("DELETE").url).toContain("/v1/events/e1");
  });
});

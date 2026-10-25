import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Contribution } from "../api/contributions";
import type { Project } from "../api/projects";
import { ProjectPage } from "./ProjectPage";
import { aProject } from "../testing/fixtures";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const roof = aProject({
  id: "p1",
  name: "Church roof renovation",
  categoryId: 1,
  categoryName: "Building",
  managerId: "m1",
  managerName: "Ama Boateng",
  status: "AtRisk",
  targetAmount: 80000,
  raisedAmount: 48200,
  startDate: "2026-03-01",
  endDate: "2026-12-31",
  description: "Replace the sanctuary roof sheets.",
  createdAt: "2026-01-01T00:00:00Z",
});
const gift = (
  id: string,
  day: string,
  amount: number,
  paymentMethod: Contribution["paymentMethod"],
  description: string | null,
): Contribution => ({
  id,
  projectId: "p1",
  projectName: roof.name,
  amount,
  dateContributed: day,
  paymentMethod,
  description,
  createdAt: "2026-01-01T00:00:00Z",
});
const gifts = [
  gift("c1", "2026-10-11", 4200, "MobileMoney", "Sunday appeal"),
  gift("c2", "2026-10-04", 2850, "Cash", null),
];
const summary = {
  totalContributed: 7050,
  totalContributions: 2,
  averageAmount: 3525,
  mostUsedPaymentMethod: "MobileMoney",
};

// The answers to saving and deleting a contribution: the server echoes what was sent.
type Sent = {
  dateContributed: string;
  amount: number;
  paymentMethod: Contribution["paymentMethod"];
  description?: string;
};
const echoes: Record<string, (u: URL, body: Sent) => Response | undefined> = {
  DELETE: () => new Response(null, { status: 204 }),
  POST: (u, b) =>
    u.pathname.endsWith("/project-contributions")
      ? json(
          gift(
            "c9",
            b.dateContributed,
            b.amount,
            b.paymentMethod,
            b.description ?? null,
          ),
          201,
        )
      : undefined,
  PUT: (u, b) =>
    u.pathname.includes("/project-contributions/")
      ? json({ ...gifts[0], ...b })
      : undefined,
};
const write = (u: URL, init?: RequestInit) =>
  echoes[init?.method ?? ""]?.(
    u,
    init?.body ? (JSON.parse(init.body as string) as Sent) : ({} as Sent),
  );

let project: Project | "missing";
let given: Contribution[];
let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  vi.useFakeTimers({ toFake: ["Date"], now: new Date("2026-10-19T10:00:00") });
  project = roof;
  given = gifts;
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    const written = write(u, init);
    if (written) return written;
    if (u.pathname.endsWith("/project-categories"))
      return json([{ id: 1, name: "Building", projectCount: 1 }]);
    if (u.pathname.endsWith("/project-contributions/summary"))
      return json(summary);
    if (u.pathname.endsWith("/project-contributions")) {
      const method = u.searchParams.get("paymentMethod");
      return json({
        data: given.filter((c) => !method || c.paymentMethod === method),
        hasMore: false,
        next: null,
      });
    }
    return project === "missing"
      ? json({ title: "Not found", status: 404 }, 404)
      : json(project);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

const open = (address = "/projects/p1") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <Routes>
          <Route path="/projects" element={<p>The projects list</p>} />
          <Route path="/projects/:id" element={<ProjectPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );

// The page opens on Details; the other tabs are one click away.
async function goTo(name: string) {
  await userEvent.setup().click(await screen.findByRole("tab", { name }));
}
async function openOn(name: string, address?: string) {
  open(address);
  await goTo(name);
}

const asked = (path: string) =>
  fetchMock.mock.calls
    .map(([u]) => new URL(u as string, "http://x"))
    .filter((u) => u.pathname.endsWith(path));

describe("project page", () => {
  it("shows the project, how far along it is, its details and its contributions", async () => {
    open();
    expect(
      await screen.findByRole("heading", { name: "Church roof renovation" }),
    ).toBeInTheDocument();
    expect(screen.getAllByText("At risk").length).toBeGreaterThan(0);
    expect(
      screen.getByText("Replace the sanctuary roof sheets."),
    ).toBeInTheDocument();
    const figures = screen.getByRole("region", { name: "Project figures" });
    expect(await within(figures).findByText("48.2k")).toBeInTheDocument();
    expect(within(figures).getByText("80k")).toBeInTheDocument();
    const details = screen.getByRole("region", { name: "Project details" });
    expect(within(details).getByText("Ama Boateng")).toBeInTheDocument();
    expect(within(details).getByText("01 Mar 2026")).toBeInTheDocument();
    await goTo("Summary");
    const progress = screen.getByRole("region", {
      name: "Raised against target",
    });
    expect(
      within(progress).getByText("Still to raise GH₵ 31.8k"),
    ).toBeInTheDocument();
    expect(within(progress).getByText(/60%/)).toBeInTheDocument();
    await goTo("Contributions");
    const table = await screen.findByRole("list", { name: "Contributions" });
    expect(within(table).getByText("Sunday appeal")).toBeInTheDocument();
    expect(within(table).getByText("GH₵ 4.2k")).toBeInTheDocument();
    const strip = screen.getByRole("region", { name: "Contributions summary" });
    expect(await within(strip).findByText("GH₵ 7.05k")).toBeInTheDocument();
    expect(within(strip).getByText("Mobile money")).toBeInTheDocument();
  });

  it("asks for all time at first, and for one method when it is chosen", async () => {
    await openOn("Contributions");
    await screen.findByRole("list", { name: "Contributions" });
    expect(asked("/project-contributions")[0]?.searchParams.get("from")).toBe(
      "2000-01-01",
    );
    expect(
      asked("/project-contributions")[0]?.searchParams.get("projectId"),
    ).toBe("p1");
    const user = userEvent.setup();
    await user.click(screen.getByRole("button", { name: /^Method/ }));
    await user.click(await screen.findByRole("option", { name: "Cash" }));
    await waitFor(() =>
      expect(
        asked("/project-contributions").some(
          (u) => u.searchParams.get("paymentMethod") === "Cash",
        ),
      ).toBe(true),
    );
    expect(await screen.findByText("GH₵ 2.85k")).toBeInTheDocument();
    expect(screen.queryByText("Sunday appeal")).not.toBeInTheDocument();
  });

  it("says it is over the target when it is", async () => {
    project = { ...roof, raisedAmount: 85000 };
    await openOn("Summary");
    const progress = await screen.findByRole("region", {
      name: "Raised against target",
    });
    expect(
      within(progress).getByText("Over the target by GH₵ 5k"),
    ).toBeInTheDocument();
    expect(within(progress).getByText(/106%/)).toBeInTheDocument();
  });

  it("invites the first contribution when there are none, with nothing to filter", async () => {
    given = [];
    await openOn("Contributions");
    expect(await screen.findByText("No contributions yet")).toBeInTheDocument();
    expect(
      screen.queryByRole("region", { name: "Contributions summary" }),
    ).not.toBeInTheDocument();
  });

  it("says nothing matches when a filter leaves none", async () => {
    given = [];
    await openOn("Contributions", "/projects/p1?method=Cheque");
    expect(
      await screen.findByText("No contributions found"),
    ).toBeInTheDocument();
  });

  it("says the project cannot be found, with the way back", async () => {
    project = "missing";
    open();
    expect(
      await screen.findByText("We could not find this project"),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Projects" })).toBeInTheDocument();
  });

  it("opens Edit, and Delete, which goes back to the list when done", async () => {
    const user = userEvent.setup();
    open();
    await screen.findByRole("heading", { name: "Church roof renovation" });
    const more = () =>
      user.click(
        screen.getByRole("button", {
          name: "More actions for Church roof renovation",
        }),
      );
    await more();
    await user.click(screen.getByRole("menuitem", { name: "Edit" }));
    expect(
      await screen.findByText("Edit Church roof renovation"),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Close" }));
    await more();
    await user.click(screen.getByRole("menuitem", { name: "Delete" }));
    expect(
      await screen.findByText(/Its 2 contributions are kept in history/),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Delete project" }));
    expect(await screen.findByText("The projects list")).toBeInTheDocument();
  });

  describe("recording and changing contributions", () => {
    const sent = (method: string) =>
      fetchMock.mock.calls.filter(
        ([, i]) => (i as RequestInit | undefined)?.method === method,
      );

    it("records a contribution from the header: amount, method and note, for this project", async () => {
      const user = userEvent.setup();
      open();
      await screen.findByRole("heading", { name: "Church roof renovation" });
      await user.click(
        screen.getByRole("button", { name: "Add contribution" }),
      );
      expect(
        await screen.findByText(
          "Given towards Church roof renovation. Not linked to a member.",
        ),
      ).toBeInTheDocument();
      expect(
        within(screen.getByRole("dialog")).getByLabelText("Project"),
      ).toBeDisabled();
      await user.type(screen.getByLabelText("Amount"), "1,250.50");
      await user.click(
        within(screen.getByRole("dialog")).getByRole("combobox", {
          name: "Payment method",
        }),
      );
      await user.click(await screen.findByRole("option", { name: "Cheque" }));
      await user.type(screen.getByLabelText(/Note/), "Alumni cheque");
      await user.click(
        screen.getAllByRole("button", { name: "Record contribution" }).at(-1)!,
      );
      await waitFor(() => expect(sent("POST")).toHaveLength(1));
      expect(
        JSON.parse((sent("POST")[0]![1] as RequestInit).body as string),
      ).toEqual({
        projectId: "p1",
        amount: 1250.5,
        dateContributed: "2026-10-19",
        paymentMethod: "Cheque",
        description: "Alumni cheque",
      });
      await waitFor(() =>
        expect(
          screen.queryByText(
            "Given towards Church roof renovation. Not linked to a member.",
          ),
        ).not.toBeInTheDocument(),
      );
    });

    it("asks for the amount before saving", async () => {
      const user = userEvent.setup();
      open();
      await screen.findByRole("heading", { name: "Church roof renovation" });
      await user.click(
        screen.getByRole("button", { name: "Add contribution" }),
      );
      await user.click(
        screen.getAllByRole("button", { name: "Record contribution" }).at(-1)!,
      );
      expect(await screen.findByText("Enter the amount")).toBeInTheDocument();
      expect(screen.getByText("Fix 1 field to continue")).toBeInTheDocument();
      expect(sent("POST")).toHaveLength(0);
    });

    it("keeps the day and method for the next one with Save and add another", async () => {
      const user = userEvent.setup();
      open();
      await screen.findByRole("heading", { name: "Church roof renovation" });
      await user.click(
        screen.getByRole("button", { name: "Add contribution" }),
      );
      await user.click(
        within(screen.getByRole("dialog")).getByRole("combobox", {
          name: "Payment method",
        }),
      );
      await user.click(
        await screen.findByRole("option", { name: "Mobile money" }),
      );
      await user.type(screen.getByLabelText("Amount"), "300");
      await user.click(
        screen.getByRole("button", { name: "Save and add another" }),
      );
      expect(
        await screen.findByText(
          "Recorded GH₵ 300. Date and method are kept for the next one.",
        ),
      ).toBeInTheDocument();
      expect(screen.getByLabelText("Amount")).toHaveValue("");
      expect(
        within(screen.getByRole("dialog")).getByRole("combobox", {
          name: "Payment method",
        }),
      ).toHaveTextContent("Mobile money");
    });

    it("invites the first contribution from the empty state", async () => {
      given = [];
      const user = userEvent.setup();
      await openOn("Contributions");
      await screen.findByText("No contributions yet");
      await user.click(
        screen.getByRole("button", { name: "Record contribution" }),
      );
      expect(
        await screen.findByText(
          "Given towards Church roof renovation. Not linked to a member.",
        ),
      ).toBeInTheDocument();
    });

    it("opens a contribution to read, then edits it, sending only what the form holds and no project", async () => {
      const user = userEvent.setup();
      await openOn("Contributions");
      const table = await screen.findByRole("list", { name: "Contributions" });
      await user.click(
        within(table).getAllByRole("button", { name: /Mobile money/ })[0]!,
      );
      expect(
        await screen.findByText("Towards Church roof renovation"),
      ).toBeInTheDocument();
      const panel = screen.getByRole("dialog");
      expect(within(panel).getByText("Sunday appeal")).toBeInTheDocument();
      expect(within(panel).getByText("Mobile money")).toBeInTheDocument();
      await user.click(
        within(panel).getByRole("button", { name: "Edit contribution" }),
      );
      const amount = await screen.findByLabelText("Amount");
      expect(amount).toHaveValue("4,200.00");
      await user.clear(amount);
      await user.type(amount, "4300");
      await user.click(screen.getByRole("button", { name: "Save changes" }));
      await waitFor(() => expect(sent("PUT")).toHaveLength(1));
      const [url, init] = sent("PUT")[0]!;
      expect(new URL(url as string, "http://x").pathname).toMatch(
        /\/v1\/project-contributions\/c1$/,
      );
      expect(JSON.parse((init as RequestInit).body as string)).toEqual({
        amount: 4300,
        dateContributed: "2026-10-11",
        paymentMethod: "MobileMoney",
        description: "Sunday appeal",
      });
    });

    it("goes straight to editing or deleting from the ⋯ menu", async () => {
      const user = userEvent.setup();
      await openOn("Contributions");
      await screen.findByRole("list", { name: "Contributions" });
      await user.click(
        screen.getByRole("button", {
          name: "Actions for GH₵ 2.85k on 04 Oct 2026",
        }),
      );
      await user.click(
        await screen.findByRole("menuitem", { name: "Edit contribution" }),
      );
      expect(
        await screen.findByText("Edit contribution", { selector: "h2" }),
      ).toBeInTheDocument();
      await user.click(screen.getByRole("button", { name: "Close" }));
      await user.click(
        screen.getByRole("button", {
          name: "Actions for GH₵ 2.85k on 04 Oct 2026",
        }),
      );
      await user.click(
        await screen.findByRole("menuitem", { name: "Delete contribution" }),
      );
      expect(
        await screen.findByText("Delete this contribution?"),
      ).toBeInTheDocument();
      await user.click(
        screen.getByRole("button", { name: "Delete contribution" }),
      );
      await waitFor(() => expect(sent("DELETE")).toHaveLength(1));
      expect(
        new URL(sent("DELETE")[0]![0] as string, "http://x").pathname,
      ).toMatch(/\/v1\/project-contributions\/c2$/);
    });

    it("deletes from the panel, and Keep leaves it be", async () => {
      const user = userEvent.setup();
      await openOn("Contributions");
      const table = await screen.findByRole("list", { name: "Contributions" });
      await user.click(
        within(table).getAllByRole("button", { name: /Mobile money/ })[0]!,
      );
      await user.click(
        await screen.findByRole("button", { name: "Delete contribution" }),
      );
      await user.click(
        await screen.findByRole("button", { name: "Keep contribution" }),
      );
      expect(sent("DELETE")).toHaveLength(0);
    });
  });
});

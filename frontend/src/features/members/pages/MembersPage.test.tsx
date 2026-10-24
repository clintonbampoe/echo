import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, pathOf, renderApp, signIn } from "@/test/kit";
import { aMember } from "../testing/fixtures";
import { MembersPage } from "./MembersPage";

const summary = {
  totalMembers: 60,
  activeMembers: 55,
  femaleCount: 54,
  maleCount: 46,
  averageAge: 38.4,
};

let requested: string[];
function server(
  members = [
    aMember(),
    aMember({ id: "m2", name: "Abena Mensah", status: "Inactive" }),
  ],
) {
  requested = [];
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      requested.push(url);
      const path = pathOf(url);
      if (path === "/v1/members/summary") {
        // Last year had fewer new members than this year.
        return json(
          url.includes("from=") ? { ...summary, totalMembers: 46 } : summary,
        );
      }
      if (path === "/v1/members")
        return json({ data: members, hasMore: false, next: null });
      if (path.includes("/congregation"))
        return json({ id: "2", name: "Grace" });
      return json({});
    }),
  );
}

beforeEach(signIn);
afterEach(() => vi.unstubAllGlobals());

describe("members page", () => {
  it("shows the four figures, with how this year compares with last", async () => {
    server();
    renderApp(<MembersPage />, "/members");
    const strip = await screen.findByRole("region", { name: "Member summary" });
    expect(await within(strip).findByText("New this year")).toBeInTheDocument();
    expect(await within(strip).findByText("30%")).toBeInTheDocument();
    expect(within(strip).getByText("54")).toBeInTheDocument();
    expect(within(strip).getByText("46")).toBeInTheDocument();
    expect(within(strip).getByText("38")).toBeInTheDocument();
  });

  it("lists the members, with a chip only for the ones who are not active", async () => {
    server();
    renderApp(<MembersPage />, "/members");
    expect(
      await screen.findByRole("link", { name: "Abena Mensah" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Inactive")).toBeInTheDocument();
    expect(screen.getByText("Showing 2 members")).toBeInTheDocument();
  });

  it("does not ask for last year's numbers once the list is on another range", async () => {
    server();
    renderApp(<MembersPage />, "/members?window=all");
    await screen.findByRole("link", { name: "Abena Mensah" });
    const summaries = requested.filter(
      (u) => pathOf(u) === "/v1/members/summary",
    );
    expect(summaries).toHaveLength(1);
  });

  it("opens the Add member sheet from the header button", async () => {
    const user = userEvent.setup();
    server();
    renderApp(<MembersPage />, "/members");
    await user.click(await screen.findByRole("button", { name: "Add member" }));
    expect(
      await screen.findByRole("dialog", { name: "Add member" }),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Close" }));
    expect(screen.queryByRole("dialog", { name: "Add member" })).toBeNull();
  });

  it("has no row menu: rows open the profile", async () => {
    server();
    renderApp(<MembersPage />, "/members");
    await screen.findByRole("link", { name: "Kwame Asante" });
    expect(screen.queryByRole("button", { name: /Actions for/ })).toBeNull();
  });

  it("says so when nothing matches the filters", async () => {
    server([]);
    renderApp(<MembersPage />, "/members?status=Inactive");
    expect(await screen.findByText("No members match")).toBeInTheDocument();
  });
});

import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { server, show, signedIn } from "../testing/organizationPage";

beforeEach(signedIn);
afterEach(() => vi.unstubAllGlobals());

describe("organization page", () => {
  it("shows the group, who leads it and its members", async () => {
    show(server());
    expect(
      await screen.findByRole("heading", { name: "Church Choir" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Leads worship on Sundays.")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("tab", { name: /Members/ }));
    expect(
      await screen.findByRole("button", { name: /^Comfort Addo$/ }),
    ).toBeInTheDocument();
    expect(screen.getByText("Jan 2016")).toBeInTheDocument();
    expect(screen.getByText("Secretary")).toBeInTheDocument();
    const strip = screen.getByRole("region", { name: "Organization numbers" });
    expect(within(strip).getByText("61")).toBeInTheDocument();
    expect(await within(strip).findByText("Kofi Mensah")).toBeInTheDocument();
    expect(within(strip).getByText("03 Feb 2016")).toBeInTheDocument();
  });

  it("lists the events the group is hosting on the Events tab, each linking to its page", async () => {
    const user = userEvent.setup();
    show(server());
    await user.click(await screen.findByRole("tab", { name: /Events/ }));
    expect(
      await screen.findByRole("link", { name: "Christmas Carols" }),
    ).toHaveAttribute("href", "/events/e1");
    expect(screen.getByText("Main Auditorium")).toBeInTheDocument();
    expect(screen.getByText("1 upcoming event")).toBeInTheDocument();
  });

  it("deletes the group and goes back to the list", async () => {
    const user = userEvent.setup();
    const fetchMock = show(server());
    await user.click(
      await screen.findByRole("button", {
        name: "More actions for Church Choir",
      }),
    );
    await user.click(screen.getByRole("menuitem", { name: "Delete" }));
    await user.click(
      await screen.findByRole("button", { name: "Delete organization" }),
    );
    expect(await screen.findByText("The list")).toBeInTheDocument();
    expect(
      fetchMock.mock.calls.some(
        ([u, i]) =>
          (i as RequestInit | undefined)?.method === "DELETE" &&
          String(u).includes("/v1/organizations/o1"),
      ),
    ).toBe(true);
  });

  it("opens the edit sheet filled with what the group has", async () => {
    const user = userEvent.setup();
    show(server());
    await user.click(
      await screen.findByRole("button", {
        name: "More actions for Church Choir",
      }),
    );
    await user.click(screen.getByRole("menuitem", { name: "Edit" }));
    expect(await screen.findByLabelText("Name")).toHaveValue("Church Choir");
  });

  it("says when the group is gone, and links back to the list", async () => {
    show(server(404));
    expect(
      await screen.findByText("We could not find this organization"),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Organizations" })).toHaveAttribute(
      "href",
      "/organizations",
    );
  });
});

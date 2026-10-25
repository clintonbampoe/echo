import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn } from "../dashboardTestKit";
import { ProjectsCard } from "./ProjectsCard";

const project = (
  id: string,
  name: string,
  raisedAmount: number,
  targetAmount = 16000,
  status = "Active",
) => ({ id, name, raisedAmount, targetAmount, status });

let answer: () => Response;
beforeEach(() => {
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => answer()),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("projects card", () => {
  it("lists what each project has raised, with its share", async () => {
    answer = () =>
      json({
        data: [
          project("p1", "Borehole", 12000, 16000, "AtRisk"),
          project("p2", "Church roof", 4000),
        ],
        hasMore: false,
        next: null,
      });
    renderCard(<ProjectsCard />);
    // Wait for the rows instead of the list: the list is on screen while it is still loading.
    await screen.findByText("Borehole");
    const rows = within(
      screen.getByRole("list", { name: "Raised by project" }),
    );
    expect(rows.getByText("12k")).toBeInTheDocument();
    expect(rows.getByText("75%")).toBeInTheDocument();
    expect(rows.getByText("At risk")).toBeInTheDocument();
    expect(rows.getByText("4k")).toBeInTheDocument();
    expect(rows.getByText("25%")).toBeInTheDocument();
    expect(screen.getByText("16k")).toBeInTheDocument();
  });

  it("says so when nothing has been raised", async () => {
    answer = () =>
      json({
        data: [project("p1", "Borehole", 0)],
        hasMore: false,
        next: null,
      });
    renderCard(<ProjectsCard />);
    expect(await screen.findByText("Nothing raised yet")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    answer = () => json({ title: "Server error", status: 500 }, 500);
    renderCard(<ProjectsCard />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});

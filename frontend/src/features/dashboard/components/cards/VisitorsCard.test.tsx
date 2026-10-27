import { screen } from "@testing-library/react";
import { json, renderCard, signIn, asked } from "../../testing/dashboardPage";
import { VisitorsCard } from "./VisitorsCard";

const visitor = (id: string, name: string, createdAt: string) => ({
  id,
  name,
  createdAt,
});

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path } = asked(url);
      if (broken) return json({ title: "Server error", status: 500 }, 500);
      if (path === "/v1/visitors/summary")
        return json({ newVisitors: 14, convertedVisitors: 9 });
      return json({
        data: [
          visitor("v1", "Adwoa Kyei", "2026-10-04T10:00:00Z"),
          visitor("v2", "Yaw Boateng", "2026-10-18T10:00:00Z"),
        ],
        hasMore: false,
        next: null,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("visitors card", () => {
  it("counts the last 30 days and lists the newest visitors first, each a link", async () => {
    renderCard(<VisitorsCard today="2026-10-19" />);
    expect(await screen.findByText("14")).toBeInTheDocument();
    expect(screen.getByText("9")).toBeInTheDocument();
    const links = await screen.findAllByRole("link");
    expect(links.map((l) => l.getAttribute("href"))).toEqual([
      "/visitors/v2",
      "/visitors/v1",
    ]);
    expect(links[0]).toHaveTextContent("Yaw Boateng");
    expect(links[0]).toHaveTextContent("Sun 18 Oct");
  });
});

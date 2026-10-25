import { render, screen, within } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { HistoryTrack } from "./HistoryTrack";
import { lastReached, type TrackNode } from "./track";

const node = (id: string, state: TrackNode["state"], href?: string) => ({
  id,
  name: `Node ${id}`,
  state,
  href,
});

const draw = (nodes: TrackNode[], status: "loading" | "failed" | "ready") =>
  render(
    <MemoryRouter>
      <HistoryTrack
        label="Recent sessions"
        title="Last 4 sessions"
        summary="2 of 4 attended"
        nodes={nodes}
        slots={4}
        status={status}
      />
    </MemoryRouter>,
  );

describe("history track", () => {
  it("says each node aloud and joins the ones that follow each other", () => {
    const { container } = draw(
      [node("1", "on"), node("2", "on"), node("3", "off"), node("4", "on")],
      "ready",
    );
    expect(screen.getByText("2 of 4 attended")).toBeInTheDocument();
    expect(screen.getAllByRole("img").map((n) => n.ariaLabel)).toEqual([
      "Node 1",
      "Node 2",
      "Node 3",
      "Node 4",
    ]);
    const joined = [...container.querySelectorAll("li")].map(
      (li) => li.dataset.joined,
    );
    expect(joined).toEqual(["false", "true", "false", "false"]);
  });

  it("opens a node that has somewhere to go", () => {
    draw([node("1", "on", "/attendance/sessions/1")], "ready");
    expect(screen.getByRole("link", { name: "Node 1" })).toHaveAttribute(
      "href",
      "/attendance/sessions/1",
    );
  });

  it("names a node in its pill, without making a screen reader say it twice", () => {
    draw([{ ...node("1", "on"), tip: "Sunday Service, 11 Oct" }], "ready");
    const pill = screen.getByText("Sunday Service, 11 Oct");
    expect(pill).toHaveAttribute("aria-hidden", "true");
    expect(
      within(screen.getByRole("img", { name: "Node 1" })).getByText(
        "Sunday Service, 11 Oct",
      ),
    ).toBe(pill);
  });

  it("draws bones while loading and says so when it could not load", () => {
    const { container, unmount } = draw([], "loading");
    expect(container.querySelectorAll("[aria-hidden] > span")).toHaveLength(4);
    expect(screen.getByRole("region")).toHaveAttribute("aria-busy", "true");
    unmount();
    draw([], "failed");
    expect(screen.getByText("Could not load")).toBeInTheDocument();
  });
});

describe("the nodes a phone shows", () => {
  const year = [
    ...Array.from({ length: 9 }, (_, i) => node(`m${i}`, "on")),
    ...Array.from({ length: 3 }, (_, i) => node(`f${i}`, "future")),
  ];

  it("ends at the latest one reached, not on months to come", () => {
    expect(lastReached(year).map((n) => n.id)).toEqual([
      "m3",
      "m4",
      "m5",
      "m6",
      "m7",
      "m8",
    ]);
  });

  it("shows the last six when nothing is in the future", () => {
    expect(lastReached(year.slice(0, 9)).map((n) => n.id)).toEqual([
      "m3",
      "m4",
      "m5",
      "m6",
      "m7",
      "m8",
    ]);
  });

  it("shows everything when there are fewer than six", () => {
    expect(lastReached(year.slice(0, 2))).toHaveLength(2);
  });
});

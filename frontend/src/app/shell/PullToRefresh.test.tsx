import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render } from "@testing-library/react";
import { PullToRefresh } from "./PullToRefresh";

const at = (x: number, y: number) => ({
  touches: [{ clientX: x, clientY: y }],
});

function setup() {
  const client = new QueryClient();
  const refetch = vi.spyOn(client, "invalidateQueries");
  render(
    <QueryClientProvider client={client}>
      <PullToRefresh />
    </QueryClientProvider>,
  );
  return refetch;
}

describe("PullToRefresh", () => {
  it("refreshes when the page is pulled far enough and let go", () => {
    const refetch = setup();
    fireEvent.touchStart(document.body, at(100, 100));
    fireEvent.touchMove(document.body, at(100, 300));
    fireEvent.touchEnd(document.body);
    expect(refetch).toHaveBeenCalledTimes(1);
  });

  it("does not refresh when the browser cancels the gesture", () => {
    const refetch = setup();
    fireEvent.touchStart(document.body, at(100, 100));
    fireEvent.touchMove(document.body, at(100, 300));
    fireEvent.touchCancel(document.body);
    fireEvent.touchEnd(document.body);
    expect(refetch).not.toHaveBeenCalled();
  });

  it("ignores a pinch", () => {
    const refetch = setup();
    fireEvent.touchStart(document.body, {
      touches: [
        { clientX: 100, clientY: 100 },
        { clientX: 200, clientY: 100 },
      ],
    });
    fireEvent.touchMove(document.body, at(100, 300));
    fireEvent.touchEnd(document.body);
    expect(refetch).not.toHaveBeenCalled();
  });
});

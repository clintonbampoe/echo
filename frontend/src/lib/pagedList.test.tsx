import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import type { ReactNode } from "react";
import type { Page } from "./api";
import { usePagedList } from "./paged";

describe("usePagedList", () => {
  it("keeps a loaded list fresh for a while, so Back shows it without asking again", async () => {
    const client = new QueryClient();
    const load = vi.fn(async (): Promise<Page<string>> => ({
      data: ["a"],
      hasMore: false,
      next: null,
    }));
    const wrapper = ({ children }: { children: ReactNode }) => (
      <QueryClientProvider client={client}>{children}</QueryClientProvider>
    );
    const first = renderHook(() => usePagedList(["things"], load), { wrapper });
    await waitFor(() => expect(first.result.current.rows).toEqual(["a"]));
    first.unmount();

    // The person comes Back: the rows are there at once and nothing is asked.
    const back = renderHook(() => usePagedList(["things"], load), { wrapper });
    expect(back.result.current.rows).toEqual(["a"]);
    expect(back.result.current.loading).toBe(false);
    expect(load).toHaveBeenCalledTimes(1);
  });
});

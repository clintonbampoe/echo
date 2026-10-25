import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ToastProvider } from "./Toast";
import { useToast } from "./toastContext";

function Trigger({ onUndo }: { onUndo?: () => void }) {
  const toast = useToast();
  return (
    <button
      onClick={() =>
        toast({
          message: "Member added",
          action: onUndo ? { label: "Undo", onClick: onUndo } : undefined,
        })
      }
    >
      Go
    </button>
  );
}

describe("toast", () => {
  it("says what happened and offers its action", async () => {
    const user = userEvent.setup();
    const onUndo = vi.fn();
    render(
      <ToastProvider>
        <Trigger onUndo={onUndo} />
      </ToastProvider>,
    );
    await user.click(screen.getByRole("button", { name: "Go" }));
    expect(screen.getByRole("status")).toHaveTextContent("Member added");

    await user.click(screen.getByRole("button", { name: "Undo" }));
    expect(onUndo).toHaveBeenCalled();
    expect(screen.queryByRole("status")).not.toBeInTheDocument();
  });

  it("replaces an earlier toast with a newer one", async () => {
    const user = userEvent.setup();
    render(
      <ToastProvider>
        <Trigger />
      </ToastProvider>,
    );
    await user.click(screen.getByRole("button", { name: "Go" }));
    await user.click(screen.getByRole("button", { name: "Go" }));
    expect(screen.getAllByRole("status")).toHaveLength(1);
  });

  it("goes away on its own", () => {
    vi.useFakeTimers();
    try {
      render(
        <ToastProvider>
          <Trigger />
        </ToastProvider>,
      );
      act(() => screen.getByRole("button", { name: "Go" }).click());
      expect(screen.getByRole("status")).toBeInTheDocument();
      act(() => vi.advanceTimersByTime(5100));
      expect(screen.queryByRole("status")).not.toBeInTheDocument();
    } finally {
      vi.useRealTimers();
    }
  });
});

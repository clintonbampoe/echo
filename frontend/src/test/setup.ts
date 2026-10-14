import "@testing-library/jest-dom/vitest";
import { afterEach } from "vitest";
import { cleanup } from "@testing-library/react";
import { clearTokens } from "@/lib/tokens";

afterEach(() => {
  cleanup();
  clearTokens();
  localStorage.clear();
});

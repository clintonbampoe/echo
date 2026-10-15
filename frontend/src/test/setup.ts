import "@testing-library/jest-dom/vitest";
import { afterEach } from "vitest";
import { cleanup, configure } from "@testing-library/react";
import { clearTokens } from "@/lib/tokens";

// A busy machine can take longer than a second to answer, so queries wait a little longer before giving up.
configure({ asyncUtilTimeout: 3000 });

afterEach(() => {
  cleanup();
  clearTokens();
  localStorage.clear();
  sessionStorage.clear();
});

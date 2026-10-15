import { fileURLToPath } from "node:url";
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) },
  },
  test: {
    environment: "jsdom",
    // One jsdom per worker instead of one per file, with each file still isolated.
    pool: "vmThreads",
    globals: true,
    setupFiles: ["./src/test/setup.ts"],
    css: false,
  },
});

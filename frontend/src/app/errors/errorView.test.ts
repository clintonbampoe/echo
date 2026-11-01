import { describe, expect, it } from "vitest";
import { ApiError } from "@/lib/api";
import { classify, errorViews } from "./errorView";

const api = (status: number) => new ApiError(status, `HTTP_${status}`, "x");

describe("classify", () => {
  it("knows a missing page, however it was found out", () => {
    expect(classify(api(404))).toBe("notFound");
    expect(
      classify({ status: 404, statusText: "", data: null, internal: true }),
    ).toBe("notFound");
  });
  it("knows a page the person may not open", () => {
    expect(classify(api(403))).toBe("forbidden");
  });
  it("knows no network from a server fault", () => {
    expect(classify(api(0))).toBe("offline");
    expect(classify(api(500))).toBe("server");
    expect(classify(api(503))).toBe("server");
  });
  it("knows an old tab whose files have been replaced", () => {
    expect(
      classify(
        new TypeError("Failed to fetch dynamically imported module: /a.js"),
      ),
    ).toBe("stale");
  });
  it("treats anything else as a crash", () => {
    expect(classify(new Error("boom"))).toBe("crash");
    expect(classify("boom")).toBe("crash");
    expect(classify(api(400))).toBe("crash");
  });
  it("has words for every kind", () => {
    for (const view of Object.values(errorViews)) {
      expect(view.title.length).toBeGreaterThan(0);
      expect(view.text.length).toBeGreaterThan(0);
    }
  });
});

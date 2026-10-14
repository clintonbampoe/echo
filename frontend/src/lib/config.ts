import { z } from "zod";

// Read once at startup. A missing or malformed value stops the app with a clear message
// instead of failing on the first request.
// Either a full URL (local dev) or a path on the same origin such as /api (production, behind nginx).
const schema = z.object({
  VITE_API_URL: z.string().refine((v) => v.startsWith("/") || URL.canParse(v)),
});

const DEV_DEFAULT = "http://localhost:5025/api";

const parsed = schema.safeParse({
  VITE_API_URL:
    import.meta.env.VITE_API_URL ??
    (import.meta.env.DEV ? DEV_DEFAULT : undefined),
});

if (!parsed.success) {
  throw new Error(
    `Invalid frontend config. Set VITE_API_URL (e.g. ${DEV_DEFAULT} or /api).`,
  );
}

export const config = {
  apiUrl: parsed.data.VITE_API_URL.replace(/\/+$/, ""),
};

export const authKeys = {
  invitation: (code: string) => ["auth", "invitation", code] as const,
  verified: ["auth", "verified"] as const,
  verify: (token: string) => ["auth", "verify", token] as const,
};

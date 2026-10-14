import { clearTokens, saveTokens } from "@/lib/tokens";
import { queryClient } from "./queryClient";

const tokens = {
  accessToken: "a",
  accessTokenExpiresAt: "2099-01-01T00:00:00Z",
  refreshToken: "r",
  refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
};

describe("query cache", () => {
  it("is emptied when the session ends, so the next person sees nothing of the last", () => {
    saveTokens(tokens);
    queryClient.setQueryData(["account", "me"], { name: "Ama" });
    clearTokens();
    expect(queryClient.getQueryData(["account", "me"])).toBeUndefined();
  });

  it("is kept while signed in", () => {
    queryClient.setQueryData(["account", "me"], { name: "Ama" });
    saveTokens(tokens);
    expect(queryClient.getQueryData(["account", "me"])).toEqual({
      name: "Ama",
    });
  });
});

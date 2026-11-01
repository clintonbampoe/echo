import { mailProviderFor } from "./mailProvider";

describe("mailProviderFor", () => {
  it("finds the provider from the domain, whatever the case", () => {
    expect(mailProviderFor("ama@gmail.com")?.name).toBe("Gmail");
    expect(mailProviderFor("Ama@GMAIL.com")?.url).toBe(
      "https://mail.google.com",
    );
  });

  it("groups a provider's other domains under one name", () => {
    expect(mailProviderFor("ama@hotmail.com")?.name).toBe("Outlook");
    expect(mailProviderFor("ama@pm.me")?.name).toBe("Proton Mail");
  });

  it("offers nothing for an address on a domain of its own", () => {
    expect(mailProviderFor("kwame@gracechapel.org")).toBeNull();
    expect(mailProviderFor("not-an-address")).toBeNull();
  });
});

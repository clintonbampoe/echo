import { paymentOptions } from "./options";

describe("payment options", () => {
  it("lists every method with the words people read", () => {
    expect(paymentOptions).toContainEqual({
      value: "MobileMoney",
      label: "Mobile money",
    });
    expect(paymentOptions).toHaveLength(5);
  });
});

import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Disclosure } from "./Disclosure";

describe("disclosure", () => {
  it("is closed to begin with and opens and closes on its button", async () => {
    const user = userEvent.setup();
    render(
      <Disclosure title="More details" hint="email, address">
        <label>
          Email <input />
        </label>
      </Disclosure>,
    );
    const button = screen.getByRole("button", { name: /More details/ });
    expect(button).toHaveAttribute("aria-expanded", "false");
    // Closed, the fields are still in the page but out of reach: not focusable, not read out.
    expect(screen.getByLabelText("Email").closest("[inert]")).not.toBeNull();

    await user.click(button);
    expect(button).toHaveAttribute("aria-expanded", "true");
    expect(screen.getByLabelText("Email").closest("[inert]")).toBeNull();

    await user.click(button);
    expect(screen.getByLabelText("Email").closest("[inert]")).not.toBeNull();
  });

  it("stays open while told to, so an error inside is never hidden", async () => {
    const user = userEvent.setup();
    render(
      <Disclosure title="More details" forceOpen>
        <input aria-label="City" />
      </Disclosure>,
    );
    await user.click(screen.getByRole("button", { name: /More details/ }));
    expect(screen.getByLabelText("City").closest("[inert]")).toBeNull();
  });

  it("keeps what was typed when it is closed again", async () => {
    const user = userEvent.setup();
    render(
      <Disclosure title="More details">
        <input aria-label="City" />
      </Disclosure>,
    );
    const button = screen.getByRole("button", { name: /More details/ });
    await user.click(button);
    await user.type(screen.getByLabelText("City"), "Accra");
    await user.click(button);
    await user.click(button);
    expect(screen.getByLabelText("City")).toHaveValue("Accra");
  });
});

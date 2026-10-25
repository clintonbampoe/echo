import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { Donut } from "./Donut";

describe("donut", () => {
  it("lists every slice with its amount and share, as a link when it goes somewhere", () => {
    render(
      <MemoryRouter>
        <Donut
          label="Income by category"
          slices={[
            {
              key: "1",
              label: "Offerings",
              value: 75,
              text: "GH₵ 75",
              to: "/finance?category=1",
            },
            { key: "other", label: "Other", value: 25, text: "GH₵ 25" },
          ]}
        />
      </MemoryRouter>,
    );
    const list = screen.getByRole("list", { name: "Income by category" });
    expect(list).toHaveTextContent("Offerings");
    expect(list).toHaveTextContent("75%");
    expect(screen.getByRole("link", { name: /Offerings/ })).toHaveAttribute(
      "href",
      "/finance?category=1",
    );
    expect(screen.queryByRole("link", { name: /Other/ })).toBeNull();
  });
});

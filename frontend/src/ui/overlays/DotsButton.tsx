import type { ComponentPropsWithRef } from "react";
import { Icon } from "../layout/Icon";
import styles from "./DotsButton.module.css";

type Props = Omit<ComponentPropsWithRef<"button">, "children" | "className">;

// The ⋯ button of a row. Always a circle (never an oval), 36px with a 44px touch area. It squashes like a drop on
// hover, shrinks on press, and stays filled while its menu is open. Add `liquid-dots` is not needed: a hovered
// `liquid-row` lights it already.
export function DotsButton({ type = "button", ...rest }: Props) {
  return (
    <button
      type={type}
      className={`${styles.dots} liquid-dots`}
      data-press="none"
      {...rest}
    >
      <Icon name="more" size={18} />
    </button>
  );
}

import { screen, within } from "@testing-library/react";
import type userEvent from "@testing-library/user-event";
import { dayParts, monthNames, spokenDay } from "@/lib/dates";
import { toClock } from "@/lib/time";

type User = ReturnType<typeof userEvent.setup>;

const escape = (text: string) => text.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
const opener = (label: string) =>
  screen.getByRole("button", { name: new RegExp(`^${escape(label)}`) });

// Picks a day in one of our date fields, the way a person does: open it, roll to the month and year, tap the day.
export async function pickDate(user: User, label: string, day: string) {
  await user.click(opener(label));
  const dialog = screen.getByRole("dialog", { name: label });
  const [year = 0, month = 1] = dayParts(day);
  await user.click(
    within(dialog).getByRole("button", { name: /^[A-Z][a-z]+ \d{4}$/ }),
  );
  await user.click(within(dialog).getByRole("option", { name: String(year) }));
  await user.click(
    within(dialog).getByRole("option", {
      name: (monthNames[month - 1] ?? "").slice(0, 3),
    }),
  );
  await user.click(within(dialog).getByRole("button", { name: "Done" }));
  await user.click(
    within(dialog).getByRole("button", { name: spokenDay(day) }),
  );
}

// Picks a time ("14:30") in one of our time fields: open it, roll the three wheels, press Done.
export async function pickTime(user: User, label: string, time: string) {
  await user.click(opener(label));
  const dialog = screen.getByRole("dialog", { name: label });
  const { hour, minute, pm } = toClock(time);
  await user.click(within(dialog).getByRole("option", { name: String(hour) }));
  await user.click(
    within(dialog).getByRole("option", {
      name: String(minute).padStart(2, "0"),
    }),
  );
  await user.click(
    within(dialog).getByRole("option", { name: pm ? "pm" : "am" }),
  );
  await user.click(within(dialog).getByRole("button", { name: "Done" }));
}

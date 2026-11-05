// How strong a new password looks as someone types it. This only informs: the one hard rule is the length,
// enforced by the form schema and the server. Capitals, numbers and symbols raise the reading but are never required.

export type Strength = {
  // 0 for an empty field, otherwise 1 to 4: how many segments of the meter are filled.
  level: 0 | 1 | 2 | 3 | 4;
  // The word beside the meter. Empty while the field is empty.
  label: string;
  // One short line naming the next thing worth adding.
  hint: string;
};

export const MIN_LENGTH = 8;

const LABELS = ["", "Weak", "Fair", "Good", "Strong"] as const;

export function assessPassword(password: string): Strength {
  if (password.length === 0) {
    return {
      level: 0,
      label: "",
      hint: `Use at least ${MIN_LENGTH} characters.`,
    };
  }
  if (password.length < MIN_LENGTH) {
    return {
      level: 1,
      label: "Too short",
      hint: `Use at least ${MIN_LENGTH} characters.`,
    };
  }
  const hasCapital = /\p{Lu}/u.test(password);
  const hasNumber = /\p{N}/u.test(password);
  const hasSymbol = /[^\p{L}\p{N}]/u.test(password);
  const level = (1 +
    Number(hasCapital) +
    Number(hasNumber) +
    Number(hasSymbol)) as 1 | 2 | 3 | 4;
  let hint = "Nice. That is a strong password.";
  if (!hasCapital) hint = "Add a capital letter.";
  else if (!hasNumber) hint = "Add a number.";
  else if (!hasSymbol) hint = "Add a special character, like ! or ?.";
  return { level, label: LABELS[level], hint };
}

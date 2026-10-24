// How a member is described in a list: short lines made of what we know. No React in this file.
import type { Member } from "../api/members";

const join = (parts: (string | null | undefined)[]) =>
  parts.filter(Boolean).join(" · ");

// The line under a name: "024 412 7788 · Accra".
export const contactLine = (m: Pick<Member, "phoneNumber" | "city">) =>
  join([m.phoneNumber, m.city]);

// "Member since Mar 2024", or nothing when we do not know when they joined.
export function memberSince(joined: string | null | undefined) {
  if (!joined) return "";
  const [year, month] = joined.split("-");
  const names = [
    "Jan", "Feb", "Mar", "Apr", "May", "Jun",
    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
  ];
  const name = names[Number(month) - 1];
  return name && year ? `Member since ${name} ${year}` : "";
}

// The line under a name on a profile: "024 412 7788 · Accra · Member since Mar 2024".
export const profileLine = (
  m: Pick<Member, "phoneNumber" | "city" | "joinedDate">,
) => join([m.phoneNumber, m.city, memberSince(m.joinedDate)]);

// The words that fit a member in a sentence about them: "She leaves your lists", "Her tithes are kept".
export function pronouns(gender: Member["gender"]) {
  if (gender === "Female")
    return { subject: "She", possessive: "Her", leaves: "leaves" };
  if (gender === "Male")
    return { subject: "He", possessive: "His", leaves: "leaves" };
  return { subject: "They", possessive: "Their", leaves: "leave" };
}

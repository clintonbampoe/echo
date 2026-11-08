// What to tell an admin when the server refuses a change to an account. No React in this file.
export function messageFor(e: unknown): string {
  const code = (e as { errorCode?: string }).errorCode;
  if (code === "LAST_ADMIN")
    return "This is the only admin who can sign in. Make someone else an admin first.";
  if (code === "CANNOT_MODIFY_SELF")
    return "You cannot change your own account. Ask another admin.";
  return "We could not save this. Try again.";
}

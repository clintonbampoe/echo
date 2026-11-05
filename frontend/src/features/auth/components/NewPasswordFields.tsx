import type { FieldErrors, UseFormRegisterReturn } from "react-hook-form";
import { PasswordField } from "@/ui";

type Props = {
  password: UseFormRegisterReturn;
  confirm: UseFormRegisterReturn;
  errors: FieldErrors<{ password: string; confirm: string }>;
};

// A new password asked for twice, so a typo cannot lock someone out of their own account.
export function NewPasswordFields({ password, confirm, errors }: Props) {
  return (
    <>
      <PasswordField
        label="Password"
        autoComplete="new-password"
        hint="At least 8 characters."
        error={errors.password?.message}
        {...password}
      />
      <PasswordField
        label="Confirm password"
        autoComplete="new-password"
        error={errors.confirm?.message}
        {...confirm}
      />
    </>
  );
}

import type { FieldErrors, UseFormRegisterReturn } from "react-hook-form";
import { PasswordField } from "@/ui";

type Props = {
  password: UseFormRegisterReturn;
  confirm: UseFormRegisterReturn;
  // The password so far, for the strength meter.
  value: string;
  errors: FieldErrors<{ password: string; confirm: string }>;
};

// A new password asked for twice, so a typo cannot lock someone out of their own account.
export function NewPasswordFields({ password, confirm, value, errors }: Props) {
  return (
    <>
      <PasswordField
        label="Password"
        autoComplete="new-password"
        strengthOf={value}
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

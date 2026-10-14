import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useSearchParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { Alert, Button, LinkButton, PasswordField } from "@/ui";
import { AuthHeading } from "../components/AuthHeading";
import { BackLink } from "../components/AuthLinks";
import { StatusScreen } from "../components/StatusScreen";
import { useResetPassword } from "../hooks";
import { resetSchema, type ResetValues } from "../schema";
import styles from "./form.module.css";

const expired = (
  <StatusScreen
    icon="clock"
    title="This link has expired"
    text="Reset links work once and expire after 1 hour. Request a new one to continue."
  >
    <LinkButton to="/forgot" size="lg" block>
      Request a new link
    </LinkButton>
    <BackLink />
  </StatusScreen>
);

// Where the link in the reset email lands: /reset-password?token=...
export function ResetPasswordPage() {
  const token = useSearchParams()[0].get("token") ?? "";
  const reset = useResetPassword();
  const [linkDead, setLinkDead] = useState(false);
  const [problem, setProblem] = useState("");
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<ResetValues>({ resolver: zodResolver(resetSchema) });

  async function onSubmit({ newPassword }: ResetValues) {
    setProblem("");
    try {
      await reset.mutateAsync({ token, newPassword });
    } catch (e) {
      if (!isApiError(e)) throw e;
      if (e.errorCode === "INVALID_TOKEN") setLinkDead(true);
      else if (e.fieldErrors.newPassword)
        setError("newPassword", { message: e.fieldErrors.newPassword });
      else setProblem(e.message);
    }
  }

  if (token === "" || linkDead) return expired;
  if (reset.isSuccess) {
    return (
      <StatusScreen
        icon="check"
        title="Password updated"
        text="You can now sign in with your new password. Any other devices have been signed out."
      >
        <LinkButton to="/signin" size="lg" block>
          Continue to sign in
        </LinkButton>
      </StatusScreen>
    );
  }

  return (
    <>
      <AuthHeading icon="key" title="Choose a new password">
        Pick something you have not used on Echo before.
      </AuthHeading>
      {problem && (
        <div className={styles.alert}>
          <Alert title="We could not reset your password">{problem}</Alert>
        </div>
      )}
      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className={`${styles.form} ${styles.tight}`}
      >
        <PasswordField
          label="New password"
          autoComplete="new-password"
          hint="At least 8 characters."
          error={errors.newPassword?.message}
          {...register("newPassword")}
        />
        <PasswordField
          label="Confirm password"
          autoComplete="new-password"
          error={errors.confirm?.message}
          {...register("confirm")}
        />
        <Button
          type="submit"
          size="lg"
          block
          loading={reset.isPending}
          className={styles.submit}
        >
          Reset password
        </Button>
      </form>
    </>
  );
}

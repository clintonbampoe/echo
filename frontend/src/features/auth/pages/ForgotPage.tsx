import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { isApiError } from "@/lib/api";
import { Button, Field } from "@/ui";
import { forgotPassword } from "../api";
import { AuthHeading } from "../components/AuthHeading";
import { BackLink } from "../components/AuthLinks";
import { ResendButton } from "../components/ResendButton";
import { StatusScreen } from "../components/StatusScreen";
import { useForgotPassword } from "../hooks";
import { emailOnlySchema, type EmailOnlyValues } from "../schema";
import styles from "./form.module.css";

export function ForgotPage() {
  const forgot = useForgotPassword();
  const [sentTo, setSentTo] = useState("");
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<EmailOnlyValues>({ resolver: zodResolver(emailOnlySchema) });

  async function onSubmit({ email }: EmailOnlyValues) {
    try {
      await forgot.mutateAsync(email);
      setSentTo(email);
    } catch (e) {
      setError("email", {
        message: isApiError(e) ? e.message : "Something went wrong. Try again.",
      });
    }
  }

  // The API answers the same whether or not the address has an account, so we do too.
  if (sentTo) {
    return (
      <StatusScreen
        icon="mail"
        title="Check your email"
        text={
          <>
            If an account exists for <strong>{sentTo}</strong>, a reset link is
            on its way. It expires in 1 hour.
          </>
        }
      >
        <ResendButton send={() => forgotPassword(sentTo)} />
        <p className={styles.note}>
          Nothing in your inbox? Check spam, or{" "}
          <button
            type="button"
            className={styles.inline}
            onClick={() => setSentTo("")}
          >
            try a different email
          </button>
          .
        </p>
        <BackLink />
      </StatusScreen>
    );
  }

  return (
    <>
      <AuthHeading icon="key" title="Forgot your password?">
        Enter your email and we will send you a link to choose a new one.
      </AuthHeading>
      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className={styles.form}
      >
        <Field
          label="Email address"
          type="email"
          autoComplete="email"
          placeholder="you@yourchurch.org"
          error={errors.email?.message}
          {...register("email")}
        />
        <Button type="submit" size="lg" block loading={forgot.isPending}>
          Send reset link
        </Button>
      </form>
      <BackLink />
    </>
  );
}

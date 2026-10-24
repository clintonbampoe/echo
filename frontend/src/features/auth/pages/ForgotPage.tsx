import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { isApiError } from "@/lib/api";
import { Button, Field } from "@/ui";
import { forgotPassword } from "../api";
import { BackLink } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import { ResendButton } from "../components/ResendButton";
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
      <AuthScreen
        title="Check your inbox"
        text={
          <>
            If an account exists for <strong>{sentTo}</strong>, a reset link is
            on its way. It expires in 1 hour.
          </>
        }
        links={<BackLink />}
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
      </AuthScreen>
    );
  }

  return (
    <AuthScreen
      title="Forgot your password?"
      text="Enter your email and we will send a link to reset it."
      links={<BackLink />}
    >
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
        <Button
          type="submit"
          size="lg"
          pill
          block
          loading={forgot.isPending}
          className={styles.submit}
        >
          Send reset link
        </Button>
      </form>
    </AuthScreen>
  );
}

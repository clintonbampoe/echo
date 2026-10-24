import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Link, useLocation, useNavigate } from "react-router-dom";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { Button, Field, PasswordField } from "@/ui";
import { AuthScreen } from "../components/AuthScreen";
import { AuthLinks } from "../components/AuthLinks";
import { SignInAlert } from "../components/SignInAlert";
import { useSignIn } from "../hooks";
import { signInSchema, type SignInValues } from "../schema";
import styles from "./form.module.css";

// Where to go after signing in: back to the page that asked for it, otherwise the dashboard.
// Only same-site paths are accepted, so a crafted link cannot send someone elsewhere.
function destination(state: unknown): string {
  const from = (state as { from?: unknown } | null)?.from;
  return typeof from === "string" &&
    from.startsWith("/") &&
    !from.startsWith("//")
    ? from
    : "/dashboard";
}

export function SignInPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const signIn = useSignIn();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    getValues,
    formState: { errors },
  } = useForm<SignInValues>({
    resolver: zodResolver(signInSchema),
  });

  async function onSubmit(values: SignInValues) {
    setProblem(null);
    try {
      await signIn.mutateAsync(values);
      navigate(destination(location.state), { replace: true });
    } catch (e) {
      handleSubmitError(
        e,
        setError,
        { email: "email", password: "password" },
        setProblem,
      );
    }
  }

  return (
    <AuthScreen
      title="Sign in to Echo"
      text="Welcome back. Pick up where you left off."
      links={
        <AuthLinks
          items={[
            { to: "/signup", label: "Create a church account" },
            { to: "/join", label: "Join with an invitation code" },
          ]}
        />
      }
    >
      {problem && <SignInAlert problem={problem} email={getValues("email")} />}
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
        <PasswordField
          label="Password"
          autoComplete="current-password"
          placeholder="Enter your password"
          error={errors.password?.message}
          aside={
            <Link to="/forgot" className={styles.aside}>
              Forgot password?
            </Link>
          }
          {...register("password")}
        />
        <Button
          type="submit"
          size="lg"
          pill
          block
          loading={signIn.isPending}
          className={styles.submit}
        >
          Sign in
        </Button>
      </form>
    </AuthScreen>
  );
}

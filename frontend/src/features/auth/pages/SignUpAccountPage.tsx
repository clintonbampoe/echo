import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { Button, Field, PasswordField } from "@/ui";
import { AuthHeading } from "../components/AuthHeading";
import { AuthPrompt } from "../components/AuthLinks";
import { StepBar } from "../components/StepBar";
import { accountSchema, type AccountValues } from "../schema";
import { useSignUp } from "./signUpContext";
import styles from "./form.module.css";

export function SignUpAccountPage() {
  const navigate = useNavigate();
  const { account, setAccount } = useSignUp();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AccountValues>({
    resolver: zodResolver(accountSchema),
    defaultValues: account ?? undefined,
  });

  function onSubmit(values: AccountValues) {
    setAccount(values);
    navigate("/signup/church");
  }

  return (
    <>
      <StepBar step={1} of={2} />
      <AuthHeading title="Create your account">
        You will be the first administrator of your church.
      </AuthHeading>
      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className={`${styles.form} ${styles.tight}`}
      >
        <div className={styles.row}>
          <Field
            label="First name"
            autoComplete="given-name"
            error={errors.firstName?.message}
            {...register("firstName")}
          />
          <Field
            label="Last name"
            autoComplete="family-name"
            error={errors.lastName?.message}
            {...register("lastName")}
          />
        </div>
        <Field
          label="Email address"
          type="email"
          autoComplete="email"
          placeholder="you@yourchurch.org"
          hint="We will send a verification link here."
          error={errors.email?.message}
          {...register("email")}
        />
        <PasswordField
          label="Password"
          autoComplete="new-password"
          hint="At least 8 characters."
          error={errors.password?.message}
          {...register("password")}
        />
        <Button type="submit" size="lg" block className={styles.submit}>
          Continue
        </Button>
      </form>
      <AuthPrompt to="/signin" action="Sign in">
        Already have an account?
      </AuthPrompt>
    </>
  );
}

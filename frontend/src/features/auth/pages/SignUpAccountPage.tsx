import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, useWatch } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { Button, Field } from "@/ui";
import { domainRest } from "../model/domainGhost";
import { AuthLinks } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import { NewPasswordFields } from "../components/NewPasswordFields";
import { accountSchema, type AccountValues } from "../forms/schema";
import { useSignUp } from "../hooks/signUp";
import styles from "./form.module.css";

export function SignUpAccountPage() {
  const navigate = useNavigate();
  const { account, setAccount } = useSignUp();
  const {
    register,
    handleSubmit,
    control,
    setValue,
    formState: { errors },
  } = useForm<AccountValues>({
    resolver: zodResolver(accountSchema),
    defaultValues: account ?? undefined,
  });

  const password = useWatch({ control, name: "password" });
  const email = useWatch({ control, name: "email" }) ?? "";
  const rest = domainRest(email);

  function onSubmit(values: AccountValues) {
    setAccount(values);
    navigate("/signup/church");
  }

  return (
    <AuthScreen
      title="Create your account"
      text="Step 1 of 2. You become your church's first administrator."
      links={
        <AuthLinks
          heading="Already with us?"
          items={[
            { to: "/signin", label: "Sign in" },
            { to: "/join", label: "Join with an invitation code" },
          ]}
        />
      }
    >
      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className={`${styles.form} stagger`}
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
          ghost={rest ? { typed: email, rest } : undefined}
          {...register("email")}
          onKeyDown={(e) => {
            if (e.key !== "Tab" || e.shiftKey || !rest) return;
            e.preventDefault();
            setValue("email", email + rest, { shouldValidate: true });
          }}
        />
        <NewPasswordFields
          password={register("password")}
          confirm={register("confirm")}
          value={password ?? ""}
          errors={errors}
        />
        <Button type="submit" size="lg" pill block className={styles.submit}>
          Continue
        </Button>
      </form>
    </AuthScreen>
  );
}

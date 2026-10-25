import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useSearchParams } from "react-router-dom";
import { z } from "zod";
import { required } from "@/lib/validation";
import { Button, Field, TextAreaField } from "@/ui";
import { ClosingCta } from "../components/chrome/ClosingCta";
import { PageHero } from "../components/chrome/PageHero";
import { Section } from "../components/chrome/Section";
import { TextLink } from "../components/chrome/TextLink";
import { CONTACT_EMAIL, GITHUB_URL } from "../content/site";
import styles from "./ContactPage.module.css";

const schema = z.object({
  name: required("Enter your name."),
  email: required("Enter your email address.").pipe(
    z.email("Enter a valid email address."),
  ),
  message: required("Write a message."),
});
type Values = z.infer<typeof schema>;

const SUBJECTS: Record<string, string> = {
  support: "Supporting Echo",
  private: "Private deployment",
};

// There is no mail service behind this page yet, so sending opens the person's own email app
// with the message already written. Nothing is sent from here.
export function ContactPage() {
  const topic = useSearchParams()[0].get("topic") ?? "";
  const subject = SUBJECTS[topic] ?? "Hello from the Echo website";
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<Values>({ resolver: zodResolver(schema) });

  function onSubmit(v: Values) {
    const body = `${v.message}\n\n${v.name}\n${v.email}`;
    window.location.assign(
      `mailto:${CONTACT_EMAIL}?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`,
    );
  }

  return (
    <>
      <PageHero eyebrow="Contact" title={["Talk to a human."]}>
        Questions, feedback or ideas. We read everything.
      </PageHero>
      <Section top={72}>
        <div className={styles.layout}>
          <form
            onSubmit={handleSubmit(onSubmit)}
            noValidate
            className={styles.form}
          >
            <Field
              label="Your name"
              autoComplete="name"
              placeholder="Full name"
              error={errors.name?.message}
              {...register("name")}
            />
            <Field
              label="Email"
              type="email"
              autoComplete="email"
              placeholder="you@church.org"
              error={errors.email?.message}
              {...register("email")}
            />
            <TextAreaField
              label="Message"
              placeholder="How can we help?"
              error={errors.message?.message}
              {...register("message")}
            />
            <Button type="submit" size="lg" pill className={styles.send}>
              Send message
            </Button>
            <p className={styles.note}>
              This opens your email app with the message ready to send.
            </p>
          </form>
          <dl className={styles.facts}>
            <div>
              <dt>Email</dt>
              <dd>
                <TextLink to={`mailto:${CONTACT_EMAIL}`} external>
                  {CONTACT_EMAIL}
                </TextLink>
              </dd>
            </div>
            <div>
              <dt>GitHub</dt>
              <dd>
                <TextLink to={GITHUB_URL} external>
                  clintonbampoe/echo
                </TextLink>
              </dd>
            </div>
            <div>
              <dt>Based in</dt>
              <dd>Accra, Ghana</dd>
            </div>
            <div>
              <dt>We reply</dt>
              <dd>Within a working day</dd>
            </div>
          </dl>
        </div>
      </Section>
      <ClosingCta />
    </>
  );
}

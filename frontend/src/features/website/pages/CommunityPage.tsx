import { LinkButton } from "@/ui";
import { ClosingCta } from "../components/chrome/ClosingCta";
import { Columns } from "../components/chrome/Columns";
import { Heading } from "../components/chrome/Heading";
import { PageHero } from "../components/chrome/PageHero";
import { Section } from "../components/chrome/Section";
import { COMMUNITY } from "../content/pages";
import { GITHUB_URL } from "../content/site";
import styles from "./CommunityPage.module.css";

export function CommunityPage() {
  return (
    <>
      <PageHero
        eyebrow="Community"
        title={["Built in the open,", "with you."]}
        actions={
          <>
            <LinkButton
              to={GITHUB_URL}
              size="xl"
              pill
              className={styles.primary}
            >
              View on GitHub
            </LinkButton>
            <LinkButton to="#support" size="xl" variant="secondary" pill>
              Support Echo
            </LinkButton>
          </>
        }
      >
        Echo is free and open source. Read it, shape it and help it grow.
      </PageHero>
      <Section top={120}>
        <Heading
          label="Free and open source"
          title="Free for every church. Open for everyone."
        />
        <Columns items={COMMUNITY.ways} />
        <p className={styles.note}>
          Churches, developers and designers shape Echo together. Every idea is
          an issue, every fix is a pull request.
        </p>
      </Section>
      <Section id="support" top={140}>
        <Heading
          label="Support Echo"
          title="Echo stays free because people keep it going."
        >
          Donations pay for development, the community tools and the time it
          takes to make Echo better for churches. Donors are listed on this page
          if they wish.
        </Heading>
        <LinkButton
          to="/contact?topic=support"
          size="xl"
          pill
          className={styles.primary}
        >
          Give to Echo
        </LinkButton>
      </Section>
      <Section id="private" top={140}>
        <Heading
          label="For full privacy"
          title="Prefer to keep every record on your own server?"
        >
          Echo works for every church on the hosted version, free. If you would
          rather hold the data yourself, we deploy and look after a private copy
          for you.
        </Heading>
        <Columns items={COMMUNITY.private} count={2} />
        <p className={styles.note}>
          A paid plan for churches that want privacy above all. Most churches do
          not need it.
        </p>
        <LinkButton
          to="/contact?topic=private"
          size="xl"
          variant="secondary"
          pill
        >
          Talk to us
        </LinkButton>
      </Section>
      <ClosingCta />
    </>
  );
}

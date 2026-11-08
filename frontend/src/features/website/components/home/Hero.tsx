import { Cta } from "../chrome/Cta";
import { Intro } from "../chrome/Intro";
import { HeroCard } from "./HeroCard";

export function Hero() {
  return (
    <Intro
      kicker="Free and open source"
      title="Your church, answered"
      top={150}
      tablet={130}
      aside={<HeroCard />}
      actions={
        <>
          <Cta to="/signup">Get started free</Cta>
          <Cta to="/signin" variant="secondary">
            Sign in
          </Cta>
        </>
      }
    >
      Members, attendance, giving and events in one calm place. Ask a question
      and read the answer.
    </Intro>
  );
}

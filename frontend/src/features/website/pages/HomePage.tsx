import { ArrowLink } from "../components/chrome/ArrowLink";
import { RuleList } from "../components/chrome/RuleList";
import { Split } from "../components/chrome/Split";
import { AnswerStack } from "../components/home/AnswerStack";
import { Closing } from "../components/home/Closing";
import { GiftStrip } from "../components/home/GiftStrip";
import { Hero } from "../components/home/Hero";
import { Roadmap } from "../components/home/Roadmap";
import { TwoWays } from "../components/home/TwoWays";
import { FREE } from "../content/home";

// Pure composition: each band lives in components/home and owns its own words and styles.
export function HomePage() {
  return (
    <>
      <Hero />
      <Split
        kicker="Example answers"
        title="The answers are already there."
        lead="Every record you make comes back as something you can read at a glance. Tap a card to see another."
        align="center"
        gap={60}
      >
        <AnswerStack />
      </Split>
      <Split
        kicker="Free, and built in the open"
        title="Free for every church."
        lead="Better with every church that uses it."
        after={<ArrowLink to="/community">Help build it</ArrowLink>}
      >
        <RuleList rules={FREE} openFirst bare />
      </Split>
      <Split
        kicker="Roadmap"
        title="Where Echo is going."
        lead="A few big steps, chosen because they matter most to the churches using Echo. The order can change as churches tell us what they need."
        align="center"
        stack={{ tablet: 20, phone: 8 }}
      >
        <Roadmap />
      </Split>
      <TwoWays />
      <GiftStrip />
      <Closing />
    </>
  );
}

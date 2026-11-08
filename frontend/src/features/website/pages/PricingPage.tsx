import { Cta } from "../components/chrome/Cta";
import { RuleList } from "../components/chrome/RuleList";
import { Intro } from "../components/chrome/Intro";
import { Split } from "../components/chrome/Split";
import { Closing } from "../components/home/Closing";
import { GiftStrip } from "../components/home/GiftStrip";
import { Bill } from "../components/pricing/Bill";
import { QUESTIONS, SANCTUARY } from "../content/pricing";

export function PricingPage() {
  return (
    <>
      <Intro
        kicker="Pricing"
        title="Echo is free"
        top={150}
        columns="1.15fr 0.85fr"
        aside={<Bill />}
      >
        And it will be tomorrow too. We checked the price list. It is very
        short.
      </Intro>
      <Split
        kicker="For churches that need it private"
        title="Echo Sanctuary."
        lead="The same Echo, on your own servers, set up and looked after by the people who build it. Priced for your church, not per member."
        actions={<Cta to="/contact?topic=private">Talk to us</Cta>}
      >
        <RuleList rules={SANCTUARY} openFirst bare swipe />
      </Split>
      <GiftStrip
        title="Support Echo."
        text="Free because people chose to make it so. Every gift goes into the servers and the time behind each improvement."
      />
      <Split kicker="Questions" title="Short answers.">
        <RuleList rules={QUESTIONS} bare />
      </Split>
      <Closing second={{ label: "Talk to us", to: "/contact?topic=private" }} />
    </>
  );
}

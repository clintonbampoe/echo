import { ClosingCta } from "../components/chrome/ClosingCta";
import { Answers } from "../components/home/Answers";
import { Everything } from "../components/home/Everything";
import { Hero } from "../components/home/Hero";
import { OpenStrip } from "../components/home/OpenStrip";
import { Stories } from "../components/home/Stories";

// Pure composition: each part lives in components/home and owns its own words and styles.
export function HomePage() {
  return (
    <>
      <Hero />
      <Answers />
      <Stories />
      <Everything />
      <OpenStrip />
      <ClosingCta />
    </>
  );
}

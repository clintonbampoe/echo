import { formatNumber } from "@/lib/format";
import { Aside, Chip, DashCard, Figure } from "@/ui";
import { useProjects } from "../hooks";
import { CardFailed } from "./CardFailed";
import { Split } from "./Split";

const SHOWN = 4;

// 86400 -> "86.4k". Rows are narrow, so the thousands are shortened.
const short = (n: number) =>
  n >= 1000 ? `${(Math.round(n / 100) / 10).toString()}k` : formatNumber(n);

// What every project has raised, all time: the total, a bar cut by project, and the biggest ones with how far
// each is to its goal. The rest are gathered under one line.
export function ProjectsCard() {
  const { projects, failed, retry } = useProjects();
  const sorted = [...(projects ?? [])].sort((a, b) => b.raised - a.raised);
  const shown = sorted.slice(0, SHOWN);
  const rest = sorted.slice(SHOWN);
  const total = sorted.reduce((n, p) => n + p.raised, 0);
  const restRaised = rest.reduce((n, p) => n + p.raised, 0);
  return (
    <DashCard title="Projects" control={<Aside>of goal</Aside>}>
      {failed ? (
        <CardFailed onRetry={retry} />
      ) : (
        <>
          <Figure
            label="Raised so far"
            value={projects ? formatNumber(total) : "–"}
            unit="GHS raised"
          />
          <Split
            label="Raised by project"
            empty="Nothing raised yet"
            parts={[
              ...shown.map((p) => ({
                key: p.id,
                name: p.name,
                value: p.raised,
                text: short(p.raised),
                share:
                  p.target > 0
                    ? `${Math.round((p.raised / p.target) * 100)}%`
                    : "",
                tag: p.atRisk ? <Chip tone="warning">At risk</Chip> : undefined,
              })),
              ...(rest.length > 0
                ? [
                    {
                      key: "rest",
                      name: `Other, ${rest.length} projects`,
                      value: restRaised,
                      text: short(restRaised),
                    },
                  ]
                : []),
            ]}
          />
        </>
      )}
    </DashCard>
  );
}

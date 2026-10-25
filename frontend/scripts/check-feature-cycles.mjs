// Features may use each other only through index.ts, and never in a circle (A uses B uses A). A circle means a
// shared idea has no home of its own: move it to `ui`, `lib` or the feature that owns it. Fails when one is found.
import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";

const root = new URL("../src/features", import.meta.url).pathname;
const features = readdirSync(root).filter((f) =>
  statSync(join(root, f)).isDirectory(),
);

const isShipped = (path) =>
  /\.(ts|tsx)$/.test(path) &&
  !/\.test\.(ts|tsx)$/.test(path) &&
  !path.includes("/testing/");

function files(dir) {
  return readdirSync(dir).flatMap((name) => {
    const path = join(dir, name);
    return statSync(path).isDirectory() ? files(path) : [path];
  });
}

const uses = new Map(features.map((f) => [f, new Set()]));
for (const feature of features) {
  for (const path of files(join(root, feature)).filter(isShipped)) {
    const text = readFileSync(path, "utf8");
    for (const m of text.matchAll(/from\s+["']@\/features\/([\w-]+)["']/g)) {
      if (m[1] !== feature && uses.has(m[1])) uses.get(feature).add(m[1]);
    }
  }
}

const found = new Set();
function walk(feature, trail) {
  if (trail.includes(feature)) {
    const loop = trail.slice(trail.indexOf(feature));
    const start = loop.indexOf([...loop].sort()[0]);
    found.add(
      [...loop.slice(start), ...loop.slice(0, start), loop[start]].join(" -> "),
    );
    return;
  }
  for (const next of uses.get(feature)) walk(next, [...trail, feature]);
}
for (const feature of features) walk(feature, []);

if (found.size > 0) {
  console.error("Features use each other in a circle:");
  for (const loop of found) console.error("  " + loop);
  process.exit(1);
}
console.log(`No feature circles (${features.length} features).`);

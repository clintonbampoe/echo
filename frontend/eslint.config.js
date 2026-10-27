import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";
import { defineConfig, globalIgnores } from "eslint/config";

// Layers, lowest to highest: lib -> ui -> features -> pages -> app.
// A layer may import from the ones below it and never from the ones above.
const above = (...layers) =>
  layers.map((l) => ({
    group: [`@/${l}`, `@/${l}/**`],
    message: `This layer cannot import from ${l}/. Dependencies only point down: app -> pages -> features -> ui -> lib.`,
  }));

// Files that exist for tests. They may import fixtures and test kits; shipped code may not.
const TEST_FILES = [
  "src/**/*.test.{ts,tsx}",
  "src/**/testing/**",
  "src/test/**",
];
// A test may reach into another feature's testing/ for its fixtures, and nothing else.
const anotherFeatureForTests = {
  group: [
    "@/features/*/**",
    "!@/features/*/testing",
    "!@/features/*/testing/**",
  ],
  message:
    "Import another feature only through its index.ts, or its testing/ fixtures from a test.",
};
const testingFolder = {
  group: ["**/testing", "**/testing/**", "@/features/*/testing"],
  message:
    "testing/ holds fixtures for tests. Shipped code does not import it.",
};
const anotherFeature = {
  group: ["@/features/*/**"],
  message:
    "Import another feature only through its index.ts (@/features/<name>). Inside your own feature use relative imports.",
};
const restrict = (options) => ({
  "@typescript-eslint/no-restricted-imports": ["error", options],
});

// Shipped code in a layer also may not import testing/; the layer's own tests and fixtures may.
function layerRules(dir, patterns, testPatterns = patterns) {
  return [
    {
      files: [`src/${dir}/**/*.{ts,tsx}`],
      ignores: TEST_FILES,
      rules: restrict({ patterns: [...patterns, testingFolder] }),
    },
    {
      files: [`src/${dir}/**/*.test.{ts,tsx}`, `src/${dir}/**/testing/**`],
      rules: restrict({ patterns: testPatterns }),
    },
  ];
}

// What each folder inside a feature may not reach for. Order inside a feature: api -> hooks -> model -> components
// -> pages. The pure folders (api, model, forms) know nothing about React or the cache.
const REACT = [
  "react",
  "react-dom",
  "react-router-dom",
  "react-hook-form",
  "@hookform/*",
  "@tanstack/react-query",
];
const reach = (...folders) => ({
  group: folders.flatMap((f) => [`**/${f}`, `**/${f}/**`]),
  message: `This folder cannot import from ${folders.join(", ")}. Order in a feature: api -> hooks -> model -> components -> pages.`,
});
const apiValues = {
  group: [
    "../api",
    "../api/**",
    "../../api",
    "../../api/**",
    "../../../api",
    "../../../api/**",
  ],
  allowTypeImports: true,
  message:
    "Only types come from api/ here. The calls go through hooks/ (the cache).",
};
function featureFolderRules() {
  const base = [...above("pages", "app"), anotherFeature, testingFolder];
  const folder = (dir, patterns, paths = []) => ({
    files: [`src/features/*/${dir}/**/*.{ts,tsx}`],
    ignores: TEST_FILES,
    rules: restrict({ paths, patterns: [...base, ...patterns] }),
  });
  const noReact = REACT.map((name) => ({
    name,
    message: "No React or cache here.",
  }));
  const typesOnlyFromUi = {
    name: "@/ui",
    allowTypeImports: true,
    message: "This folder takes only types from ui (import type).",
  };
  const pure = [...noReact, typesOnlyFromUi];
  return [
    folder(
      "api",
      [reach("hooks", "model", "forms", "components", "pages")],
      pure,
    ),
    folder("model", [reach("hooks", "components", "pages"), apiValues], pure),
    folder("forms", [reach("hooks", "components", "pages"), apiValues], pure),
    folder("hooks", [reach("components", "pages")]),
    folder("components", [reach("pages"), apiValues]),
  ];
}

const houseSyntax = [
  {
    selector: 'JSXAttribute[name.name="dangerouslySetInnerHTML"]',
    message: "dangerouslySetInnerHTML is banned: it is an XSS hole.",
  },
  { selector: "ExportDefaultDeclaration", message: "Use named exports." },
  {
    selector: "TSEnumDeclaration",
    message: "Use a string union type instead of an enum.",
  },
  {
    selector: "ClassDeclaration",
    message: "Use plain functions and types. (ApiError is the one exception.)",
  },
  {
    selector: "CallExpression[callee.name=/^(useMemo|useCallback)$/]",
    message:
      "Do not memoize unless a measurement says so. Explain it in a comment if you must.",
  },
];

// A feature is a closed folder: a relative import that climbs out of it is a back door into another feature.
// A file `depth` folders below the feature's root may climb at most `depth` levels.
function stayInFeature() {
  return [0, 1, 2, 3, 4].map((depth) => ({
    files: [`src/features/*/${"*/".repeat(depth)}*.{ts,tsx}`],
    ignores: TEST_FILES,
    rules: {
      "no-restricted-syntax": [
        "error",
        ...houseSyntax,
        {
          selector: `ImportDeclaration[source.value=/^(\\.\\.\\/){${depth + 1},}/]`,
          message:
            "A relative import cannot leave its feature. Use @/features/<name> (its index.ts).",
        },
      ],
    },
  }));
}

export default defineConfig([
  globalIgnores(["dist", "src/ui/layout/Icon.tsx"]),
  {
    files: ["**/*.{ts,tsx}"],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: { globals: globals.browser },
    rules: {
      "no-unused-vars": "off",
      "@typescript-eslint/no-unused-vars": [
        "error",
        {
          vars: "all",
          args: "after-used",
          varsIgnorePattern: "^_",
          argsIgnorePattern: "^_",
        },
      ],
    },
  },
  {
    // House rules for all app code.
    files: ["src/**/*.{ts,tsx}"],
    rules: {
      "no-console": ["error", { allow: ["warn", "error"] }],
      complexity: ["error", 10],
      "max-lines": [
        "error",
        { max: 260, skipBlankLines: true, skipComments: true },
      ],
      "max-lines-per-function": [
        "warn",
        { max: 100, skipBlankLines: true, skipComments: true, IIFEs: true },
      ],
      "@typescript-eslint/consistent-type-definitions": ["error", "type"],
      "@typescript-eslint/consistent-type-imports": "error",
      "no-restricted-syntax": ["error", ...houseSyntax],
    },
  },
  {
    // ApiError is the one class in the codebase, so this file keeps every rule above except the class ban.
    files: ["src/lib/api/errors.ts"],
    rules: {
      "no-restricted-syntax": [
        "error",
        { selector: "ExportDefaultDeclaration", message: "Use named exports." },
        {
          selector: "TSEnumDeclaration",
          message: "Use a string union type instead of an enum.",
        },
      ],
    },
  },
  ...layerRules("lib", above("ui", "features", "pages", "app")),
  ...layerRules("ui", above("features", "pages", "app")),
  ...layerRules(
    "features",
    [...above("pages", "app"), anotherFeature],
    [...above("pages", "app"), anotherFeatureForTests],
  ),
  ...layerRules(
    "pages",
    [...above("app"), anotherFeature],
    [...above("app"), anotherFeatureForTests],
  ),
  ...featureFolderRules(),
  ...stayInFeature(),
  {
    // Tests may be long and may poke at internals.
    files: ["src/**/*.test.{ts,tsx}", "src/test/**"],
    rules: {
      "max-lines": "off",
      "max-lines-per-function": "off",
      "no-restricted-syntax": "off",
      "react-refresh/only-export-components": "off",
    },
  },
]);

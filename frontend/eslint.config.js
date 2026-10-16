import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";
import { defineConfig, globalIgnores } from "eslint/config";

// Layers, lowest to highest: lib -> ui -> features -> app.
// A layer may import from the ones below it and never from the ones above.
const above = (...layers) =>
  layers.map((l) => ({
    group: [`@/${l}`, `@/${l}/**`],
    message: `This layer cannot import from ${l}/. Dependencies only point down: app -> features -> ui -> lib.`,
  }));

export default defineConfig([
  globalIgnores(["dist", "src/ui/Icon.tsx"]),
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
      "no-restricted-syntax": [
        "error",
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
          message:
            "Use plain functions and types. (ApiError is the one exception.)",
        },
        {
          selector: "CallExpression[callee.name=/^(useMemo|useCallback)$/]",
          message:
            "Do not memoize unless a measurement says so. Explain it in a comment if you must.",
        },
      ],
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
  {
    files: ["src/lib/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": [
        "error",
        { patterns: above("ui", "features", "app") },
      ],
    },
  },
  {
    files: ["src/ui/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": [
        "error",
        { patterns: above("features", "app") },
      ],
    },
  },
  {
    files: ["src/features/**/*.{ts,tsx}"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            ...above("app"),
            {
              group: ["@/features/*/**"],
              message:
                "Import another feature only through its index.ts (@/features/<name>). Inside your own feature use relative imports.",
            },
          ],
        },
      ],
    },
  },
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

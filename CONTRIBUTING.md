# Contributing to Echo

First off, thank you for considering contributing! It means a lot. Whether you're fixing a typo, reporting a bug, or adding a whole new feature—every bit helps.

We want to make contributing as smooth as possible. Here's how the project works.

## How to report an issue

Open a GitHub Issue and pick the right template for the problem you're trying to fix or draw our attention to:

- **Bug report** – Something is broken. Include steps to reproduce and what you expected.
- **Feature request** – An idea or enhancement. Explain the use case and why it matters.
- **Security vulnerability** – **Do not** file a public issue for non critical security concerns. Report for critical exploitable vulnerabilities, see [SECURITY.md](./SECURITY.md)
- **Chore / Maintenance** – Refactors, dependency bumps, or tooling improvements.

Before opening a new issue, search to see if someone already reported it. If you find an existing one, add a comment instead of creating a duplicate.

## How to contribute code

1. **Fork the repo** (if you're an external contributor) or **create a branch** (if you're on the core team).
   Branch name: a prefix and a short description, such as `feat/member-search`, `fix/login-redirect` or `chore/bump-vite`.

2. **Make your changes** – keep them focused. One PR per logical change, please.

3. **Write tests** for your changes.
   > **Note:** We're currently building out our test suite. The project started as a small experiment, but with more contributors joining, we're taking testing seriously now.
   > If you're adding new features, please include tests. If you're fixing a bug, add a test that would have caught it.
   > If you're unsure how to test something, ask in the PR—we're figuring this out together.

4. **Run the checks** locally and make sure they pass:

   ```bash
   dotnet test --solution backend/Echo.slnx --filter "Category=Unit"
   cd frontend && npm run check
   ```

   (If tests don't exist for the area you're touching yet, that's okay—but we'll ask you to at least manually verify your changes.)

5. **Update the docs** in the same pull request if you change how something works or is run. `docs/check-links.sh` checks the links.

6. **Open a Pull Request** against the `main` branch. Use the PR template—it helps us review faster. CI runs the `api`, `frontend` and `infra` checks and a secret scan on every pull request.

## Code style & conventions

- We follow the rules defined in the `.editorconfig` file at the repository root. Your editor should pick them up automatically.
- Write meaningful commit messages (e.g., `Add validation to registration endpoint`, not `fix stuff`).
- Keep functions and methods small and focused. If it's doing more than one thing, split it up.

## Review process

- At least one maintainer will review your PR.
- We may ask for changes—that's normal. It's about improving the code and keeping it very maintainable, please bear with us.
- Once approved, a maintainer will merge it.

## Getting help

If you're stuck, open a **Discussion** or comment on the relevant issue. We're happy to point you in the right direction.

Thanks again for contributing!

# Contributing

Thank you for your interest in contributing to Echo. To maintain a clean and efficient development process, please follow these guidelines.

## Project Tracking

All project management and task tracking is automated so that devs can focus on writing quality code. Please read the following text below carefully and minimize the time spent on administrative tasks. Pushing your branch and managing your draft pull requests will handle all card movement on the board automatically. Hence, please read the text below carefully, as understanding it reduces the amount of work drastically.
Thank you.

## Getting Started With Linear

We use Linear to manage our backlog and assign work. Before writing any code, locate your assigned issue in Linear to check its point estimation and copy the pre-formatted branch name using `Ctrl + Shift + .`.

Creating your branch locally with this exact name connects your terminal directly to our tracking system.

## Development Workflow

Our development loop relies on GitHub Draft Pull Requests to define the scope of work early and prevent conflicting architectural changes.

1. Create your local branch from main using the copied Linear name (the branch name should follow the format: prefix/{the copied Linear name} e.g feat/clnt-11-implement-clienr-app-login-page).
2. Make an initial setup commit with your basic file structure and push it to GitHub.
3. Open a Draft Pull Request on GitHub instead of a standard one. This signals that work is underway and automatically moves your Linear card to In Progress.
4. Use Markdown checkboxes in the pull request description to outline your technical plan and include `Closes {Identifier}-{Number}` at the bottom.
5. Push your commits regularly as you work. Each push updates the draft automatically.
6. Check off your description boxes as you complete parts of the task.
7. Click Ready for Review at the bottom of the GitHub page once all unit tests pass. This alerts your teammate for a final code review and blocks accidental merges until approval.

Merging the finalized pull request will automatically close the linked Linear issue.
NB: For the issue identifiers in Linear, just use a **four letter name that is easily identifiable**, I use CLNT-{number}. It's just a convenient way for us to know who is working on what.

## Code Standards

To keep the codebase consistent, please adhere to these practices:

* Follow the established architectural conventions, naming styles, and patterns of the repository.
* Keep logic decoupled to ensure business rules, user interfaces, and data access remain separate.
* **Update all technical documentation**, schemas, and diagrams immediately when making structural changes to keep team mates up to date *(We can't read your mind)*.
* Write corresponding unit tests for all new logic to prevent regressions.

## Reporting Issues

If you find a bug or have a suggestion for a new feature, open an issue in Linear. Provide enough detailed context so the problem can be reproduced or evaluated effectively.

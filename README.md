# Echo

Echo is a church management web application designed to help religious organizations centralize their administration and financial records. The platform acts as a unified hub for tracking church operations, ensuring financial transparency and data organization for both small and large congregations.

## Summary

**Financial Management**: Specialized modules to record tithes and contributions with visual dashboards for analytical insights.

**Member and Attendance Tracking**: Tools for maintaining a member database and tracking attendance for various church services.

**Asset and Project Management**: A system to catalog church assets and monitor the progress of specific church projects.

**Event Coordination**: Features for planning church events, managing teams, and organizing service schedules.

## Tech Stack

The project uses a client-server architecture with the following technologies

- **React** frontend for building the user interface
- **C# Web API** for the server-side logic and database communication
- **PostgreSQL** for relational data storage.
- **Entity Framework Core** for managing database operations within the API.

## Requirements

To develop or run this project locally, the following software must be installed:

- .NET 10 SDK
- Node.js
- PostgreSQL
- Visual Studio / Rider / VS Code  (or any text editor of choice)

## Setting Up In VS Code

Follow these steps to initialize your development environment in VS Code.

### 1. Required Extensions

Upon opening this folder, VS Code will prompt you to install recommended extensions (based on `extensions.json`).

- Click **Install All**.
- This ensures the **C# Dev Kit** and **Debugger** are active.

### 2. Building & Running the API

The repository includes pre-defined tasks and launch profiles to handle the build and execution process.

- Press `Ctrl + Shift + B`. This triggers the `build: Backend.Api.Core` task defined in `tasks.json`.

- Press `F5`. This will run the following tasks:
    1. Run the build task.
    2. Launch the `Backend.Api.Core` project using the **.NET 10** profile from `launch.json`.
    3. Open your default browser once the API is listening.

### 3. Automation Features

The `settings.json` file applies the following behaviors automatically:

- Code is automatically formatted to project standards whenever you save.
- Unused namespaces are stripped on save.
- The `bin` and `obj` directories are hidden from the file explorer to reduce clutter.

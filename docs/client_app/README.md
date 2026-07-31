# Echo Frontend Application Documentation

This document provides a comprehensive overview of the Echo frontend codebase. It is designed to help new developers quickly understand the project structure, architectural decisions, and development workflows.

## 1. Project Overview & Tech Stack

The Echo frontend is a Single Page Application (SPA) built to manage church administration, member databases, events, and finances.

**Core Tech Stack:**
*   **Framework:** React 19
*   **Language:** TypeScript
*   **Build Tool:** Vite 8
*   **Styling:** Vanilla CSS (CSS Variables, Flexbox, CSS Grid) - intentionally avoiding utility classes like Tailwind to maintain custom, bespoke design tokens.
*   **Icons:** Custom SVG React components.

## 2. Folder Structure

The application source code lives inside `frontend/src/`. Here is the breakdown of the directory structure:

```text
frontend/src/
├── assets/          # Static files, images, SVGs, and global assets
├── components/      # React components (Pages, Shared Components, and UI Elements)
├── context/         # React Context providers for global state (Auth, Layout)
├── services/        # API service modules for interacting with the backend
├── styles/          # Component-specific CSS files (e.g., Events.css, Finance.css)
├── types/           # Global TypeScript interfaces and type definitions
├── utils/           # Helper functions (e.g., exportUtils.ts for CSV generation)
├── App.tsx          # Main application wrapper and router
├── index.css        # Global CSS tokens, reset, and base typography
└── main.tsx         # Vite entry point, renders App to the DOM
```

## 3. Pages and Routing Organization

Currently, the application uses **State-Based Routing** rather than a library like `react-router-dom`.

*   **How it works:** The main `App.tsx` component maintains an `activeTab` state (e.g., `'dashboard'`, `'events'`, `'members'`).
*   **Sidebar Navigation:** The `<Sidebar />` component triggers state changes by passing the selected tab ID up to `App.tsx`.
*   **Conditional Rendering:** Inside `App.tsx`, a `switch`-like block conditionally mounts the correct page component (e.g., `{activeTab === 'events' && <Events />}`).
*   **Deep Linking:** Note that because routing is state-based, there are currently no URL routes (e.g., `/events`). All navigation happens seamlessly within the SPA without page reloads.

## 4. State Management

The application avoids heavy state management libraries like Redux in favor of native React APIs:

1.  **Global State (React Context):**
    *   `AuthContext.tsx`: Manages user authentication, login state, and tokens. Wraps the entire application.
    *   `LayoutContext.tsx`: Manages the Topbar UI. Page components use the `useLayout` hook to dynamically inject their own page titles, breadcrumbs, and Topbar call-to-action buttons (CTAs) like "Export" or "Search".
2.  **Local State (useState / useReducer):**
    *   Individual page components (like `Events.tsx` or `Members.tsx`) manage their own complex state (e.g., toggling slide-out panels, master-detail views, or form data) using standard React hooks.

## 5. API Calls and Services

All interactions with the backend C# API are abstracted into the `services/` directory.

*   **Structure:** Each domain has its own service file (e.g., `titheService.ts`, `memberService.ts`).
*   **Usage:** React components should *never* make raw `fetch` calls directly. Instead, they import the necessary functions from the `services/` directory.
*   **Mock Data:** Currently, several pages are using hardcoded `mockData` arrays for rapid UI prototyping. As the backend endpoints are finalized, these arrays will be swapped out for `useEffect` hooks that call the service layer.

## 6. Shared Components

Shared, reusable components live in the `src/components/` directory alongside page components. Key shared components include:

*   **`Icons.tsx`**: A massive library of pure SVG icons wrapped in React components (e.g., `<ChevronLeftIcon />`, `<MapPinIcon />`). Always import icons from here rather than using image tags.
*   **`ExportPanel.tsx`**: A reusable slide-out panel for generating CSV reports. It accepts an `exportConfig` prop (columns and rows) and handles the actual file generation via `utils/exportUtils.ts`.
*   **`Sidebar.tsx` & `Topbar.tsx`**: The core layout shells. The Topbar is deeply integrated with `LayoutContext` so pages can inject action buttons into it.

## 7. Environment Variables & Local Setup

The project uses Vite's built-in environment variable handling.

1.  **`.env` Files**: Look for a `.env` or `.env.example` file in the root `echo/` or `echo/frontend/` directory.
2.  **Prefixing**: Any variables that need to be exposed to the client-side code *must* be prefixed with `VITE_` (e.g., `VITE_API_BASE_URL=http://localhost:5000/api`).
3.  **Running Locally**:
    *   Ensure Node.js is installed.
    *   Navigate to `echo/frontend/`
    *   Run `npm install` to grab dependencies.
    *   Run `npm run dev` to start the Vite HMR server on `http://localhost:5173`.
    *   Run `npm run build` to compile the TypeScript and bundle the app for production.

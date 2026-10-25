import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { App } from "@/app/App";
import { applyPreferences } from "@/lib/preferences";
import "@/styles/index.css";

applyPreferences();

const root = document.getElementById("root");
if (!root) throw new Error("Missing #root element in index.html");

createRoot(root).render(
  <StrictMode>
    <App />
  </StrictMode>,
);

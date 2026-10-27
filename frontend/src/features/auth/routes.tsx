import type { RouteObject } from "react-router-dom";
import { AuthShell } from "./components/AuthShell";

// Every URL that belongs to auth. Paths for the two emailed links (/verify-email and
// /reset-password) are fixed by the backend's link builder: change them there too.
export const authRoutes: RouteObject[] = [
  {
    Component: AuthShell,
    children: [
      {
        path: "signin",
        lazy: async () => ({
          Component: (await import("./pages/SignInPage")).SignInPage,
        }),
      },
      {
        path: "signup",
        lazy: async () => ({
          Component: (await import("./pages/SignUpAccountPage"))
            .SignUpAccountPage,
        }),
      },
      {
        path: "signup/church",
        lazy: async () => ({
          Component: (await import("./pages/SignUpChurchPage"))
            .SignUpChurchPage,
        }),
      },
      {
        path: "verify",
        lazy: async () => ({
          Component: (await import("./pages/VerifySentPage")).VerifySentPage,
        }),
      },
      {
        path: "verify-email",
        lazy: async () => ({
          Component: (await import("./pages/VerifyEmailPage")).VerifyEmailPage,
        }),
      },
      {
        path: "forgot",
        lazy: async () => ({
          Component: (await import("./pages/ForgotPage")).ForgotPage,
        }),
      },
      {
        path: "reset-password",
        lazy: async () => ({
          Component: (await import("./pages/ResetPasswordPage"))
            .ResetPasswordPage,
        }),
      },
      {
        path: "join",
        lazy: async () => ({
          Component: (await import("./pages/JoinCodePage")).JoinCodePage,
        }),
      },
      {
        path: "join/:code",
        lazy: async () => ({
          Component: (await import("./pages/ConfirmChurchPage"))
            .ConfirmChurchPage,
        }),
      },
      {
        path: "join/:code/account",
        lazy: async () => ({
          Component: (await import("./pages/JoinPage")).JoinPage,
        }),
      },
    ],
  },
];

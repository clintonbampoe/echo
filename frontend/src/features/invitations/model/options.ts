import type { UserRole } from "@/features/account";

export const roleOptions: { value: UserRole; label: string; hint?: string }[] =
  [
    { value: "Member", label: "Member" },
    {
      value: "Clerk",
      label: "Clerk",
      hint: "Clerks can record tithes and attendance. They cannot see finance.",
    },
    { value: "Accountant", label: "Accountant" },
    { value: "Admin", label: "Administrator" },
  ];

export const expiryOptions = [
  { value: "1", label: "In 1 day" },
  { value: "7", label: "In 7 days" },
  { value: "14", label: "In 14 days" },
  { value: "30", label: "In 30 days" },
];

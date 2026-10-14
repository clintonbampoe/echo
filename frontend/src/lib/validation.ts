import { z } from "zod";

// A text field that must not be empty or only spaces. The message is what the person reads.
export const required = (message: string) => z.string().trim().min(1, message);

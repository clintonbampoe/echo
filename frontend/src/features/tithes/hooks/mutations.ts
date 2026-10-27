import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { TitheUpdate } from "../api/tithes";
import { createTithe, deleteTithe, updateTithe } from "../api/tithes";
import { titheKeys } from "./keys";

// Recording, changing or deleting a tithe changes every list, number and chart, and a member's profile.
function useRefreshTithes() {
  const client = useQueryClient();
  // Not awaited: the form that saved moves on at once, and the lists refresh behind it.
  return () => void client.invalidateQueries({ queryKey: titheKeys.all });
}

export function useCreateTithe() {
  const refresh = useRefreshTithes();
  return useMutation({ mutationFn: createTithe, onSuccess: refresh });
}

export function useUpdateTithe() {
  const refresh = useRefreshTithes();
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & Partial<TitheUpdate>) =>
      updateTithe(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteTithe() {
  const refresh = useRefreshTithes();
  return useMutation({ mutationFn: deleteTithe, onSuccess: refresh });
}

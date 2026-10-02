import type { TransactionType } from "./transaction";

export interface TransactionCategory {
  id: number;
  name: string;
  categoryType: TransactionType;
}

export interface TransactionCategoryCreatePayload {
  name: string;
  categoryType: TransactionType;
}

export interface TransactionCategoryUpdatePayload {
  name?: string;
  categoryType?: TransactionType;
}

// Mirrors TransactionCategorySearchResponseDto — note `type`, not `categoryType`
export interface TransactionCategorySearchResult {
  id: number;
  name: string;
  type: TransactionType;
}

export interface CategoryForm {
  name: string;
  categoryType: TransactionType;
}

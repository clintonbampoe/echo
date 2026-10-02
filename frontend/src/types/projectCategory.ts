export interface ProjectCategory {
  id: number;
  name: string;
}

export interface ProjectCategoryCreatePayload {
  name: string;
}

export interface ProjectCategoryUpdatePayload {
  name?: string;
}

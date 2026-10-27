import {
  emptyProject,
  fromProject,
  projectSchema,
  toProjectInput,
  toProjectUpdate,
} from "./project";
import { aProject } from "../testing/fixtures";

const project = aProject({
  id: "p1",
  name: "Church roof renovation",
  categoryId: 1,
  categoryName: "Building",
  managerId: "m1",
  managerName: "Ama Boateng",
  status: "AtRisk",
  targetAmount: 80000,
  raisedAmount: 48200,
  startDate: "2026-03-01",
  endDate: "2026-12-31",
  description: "Fix the roof.",
  createdAt: "2026-01-01T00:00:00Z",
});

const filled = {
  ...emptyProject,
  name: "Roof",
  categoryId: "1",
  managerId: "m1",
  targetAmount: "80,000",
  startDate: "2026-03-01",
};
const problems = (values: typeof emptyProject, endLocked = false) =>
  projectSchema(endLocked)
    .safeParse(values)
    .error?.issues.map((i) => `${i.path.join(".")}: ${i.message}`) ?? [];

describe("project form", () => {
  it("accepts a complete project, with or without an end", () => {
    expect(problems(filled)).toEqual([]);
    expect(problems({ ...filled, endDate: "2026-12-31" })).toEqual([]);
  });

  it("says what is missing", () => {
    expect(problems(emptyProject)).toEqual([
      "name: Enter a project name",
      "categoryId: Choose a category",
      "managerId: Choose a manager",
      "targetAmount: Enter the target",
      "startDate: Enter the start date",
    ]);
  });

  it("keeps the target between 0.01 and 1,000,000", () => {
    for (const bad of ["0", "0.00", "1,000,000.01", "abc", "12.345"])
      expect(problems({ ...filled, targetAmount: bad })).toEqual([
        "targetAmount: Enter a target between 0.01 and 1,000,000",
      ]);
    expect(problems({ ...filled, targetAmount: "1,000,000" })).toEqual([]);
    expect(problems({ ...filled, targetAmount: "0.01" })).toEqual([]);
  });

  it("does not let the end come before the start, or an end that is set be taken away", () => {
    expect(problems({ ...filled, endDate: "2026-02-28" })).toEqual([
      "endDate: The end date cannot be before the start date",
    ]);
    expect(problems(filled, true)).toEqual([
      "endDate: An end date can be changed, not removed",
    ]);
  });

  it("fills the form from a project and turns it back into what the API takes", () => {
    const values = fromProject(project);
    expect(values).toMatchObject({
      categoryId: "1",
      targetAmount: "80,000.00",
      startDate: "2026-03-01",
      endDate: "2026-12-31",
      status: "AtRisk",
    });
    expect(toProjectInput(values)).toEqual({
      categoryId: 1,
      managerId: "m1",
      name: "Church roof renovation",
      targetAmount: 80000,
      status: "AtRisk",
      startDate: "2026-03-01",
      endDate: "2026-12-31",
      description: "Fix the roof.",
    });
  });

  it("leaves out an empty end and description when adding, and sends the description empty when saving", () => {
    expect(toProjectInput(filled)).toEqual({
      categoryId: 1,
      managerId: "m1",
      name: "Roof",
      targetAmount: 80000,
      status: "Planning",
      startDate: "2026-03-01",
    });
    expect(toProjectUpdate(filled).description).toBe("");
  });
});

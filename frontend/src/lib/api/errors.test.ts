import { parseProblem } from "./errors";

describe("parseProblem", () => {
  it("reads the errorCode and detail from a problem response", () => {
    const e = parseProblem(401, {
      title: "Unauthorized",
      detail: "Wrong email or password.",
      errorCode: "INVALID_CREDENTIALS",
    });
    expect(e.status).toBe(401);
    expect(e.errorCode).toBe("INVALID_CREDENTIALS");
    expect(e.message).toBe("Wrong email or password.");
  });

  it("keys validation errors by their full camelCase path", () => {
    const e = parseProblem(400, {
      errors: {
        "userDto.EmailAddress": ["Not a valid email."],
        "congregationDto.name": "Required.",
      },
    });
    expect(e.errorCode).toBe("VALIDATION_ERROR");
    expect(e.fieldErrors).toEqual({
      "userDto.emailAddress": "Not a valid email.",
      "congregationDto.name": "Required.",
    });
  });

  it("falls back to HTTP_<status> and a friendly message when the body is not a problem", () => {
    const e = parseProblem(502, "Bad gateway");
    expect(e.errorCode).toBe("HTTP_502");
    expect(e.message).toMatch(/try again/i);
  });
});

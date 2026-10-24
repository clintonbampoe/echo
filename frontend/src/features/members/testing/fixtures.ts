// A member for tests, with sensible values for everything. Override what the test is about.
import type { Member } from "../api/members";

export const aMember = (over: Partial<Member> = {}): Member => ({
  id: "m1",
  name: "Kwame Asante",
  firstName: "Kwame",
  lastName: "Asante",
  phoneNumber: "024 412 7788",
  emailAddress: "kwame@example.com",
  dateOfBirth: "1988-02-14",
  joinedDate: "2019-03-12",
  gender: "Male",
  residentialAddress: "14 Palm St",
  city: "Accra",
  hometown: "Kumasi",
  region: "GreaterAccra",
  maritalStatus: "Married",
  nextOfKin: "Efua Asante",
  emergencyContactName: "Efua Asante",
  emergencyContactPhoneNumber: "055 301 4408",
  status: "Active",
  createdAt: "2026-01-01T00:00:00Z",
  ...over,
});

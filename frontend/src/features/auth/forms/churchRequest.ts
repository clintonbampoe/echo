import type { AccountValues, ChurchValues } from "./schema";

// What the API wants when a church and its first person are created together.
export function churchRequest(v: ChurchValues, account: AccountValues) {
  return {
    congregationDto: {
      name: v.name,
      orgType: v.orgType,
      phoneNumber: v.phoneNumber,
      emailAddress: v.churchEmail,
      region: v.region,
      city: v.city,
      town: v.town,
      gpsAddress: v.gpsAddress,
    },
    userDto: {
      firstName: account.firstName,
      lastName: account.lastName,
      emailAddress: account.email,
      password: account.password,
    },
  };
}

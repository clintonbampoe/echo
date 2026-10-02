export interface EventRegistration {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  eventName: string;
  registrationDate: string;
  createdAt: string;
}

export interface EventRegistrationCreatePayload {
  memberId: string;
  eventId: string;
  registrationDate: string;
}

export interface EventRegistrationUpdatePayload {
  registrationDate?: string;
}

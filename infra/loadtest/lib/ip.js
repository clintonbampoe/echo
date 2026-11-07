// Each person reaches the API from an address of their own, and each address is used by one person only, so the rate
// limiters (which count by address) never refuse anyone and the results show how the server copes with the load. The
// API trusts X-Forwarded-For from the Docker network.
export const addressOf = ({ n }) => `10.${(n + 1) >> 16}.${((n + 1) >> 8) & 255}.${(n + 1) & 255}`;

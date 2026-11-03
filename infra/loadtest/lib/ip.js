// Each person reaches the API from an address of their own, so the rate limiters (which count by address) measure how
// the server copes with the load, and a sign-in burst from one church cannot lock that church out and fill the results
// with 429s. SHARED_ADDRESS=1 puts everyone in a church behind one address, the way an office is behind a router, to
// see how the limits treat a big church. The API trusts X-Forwarded-For from the Docker network.
const hash = (text) => {
  let h = 0;
  for (let i = 0; i < text.length; i++) h = (h * 31 + text.charCodeAt(i)) >>> 0;
  return h;
};

export const addressOf = (account) => {
  const h = hash(__ENV.SHARED_ADDRESS === '1' ? account.tenant : account.email);
  return `10.${(h % 250) + 1}.${(Math.floor(h / 250) % 250) + 1}.${(Math.floor(h / 62500) % 250) + 1}`;
};

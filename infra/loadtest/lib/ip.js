// Everyone in a church reaches the API from the church's one address, the way an office does behind a router.
// The API trusts X-Forwarded-For from the Docker network, so this is the address its rate limiter counts.
const hash = (text) => {
  let h = 0;
  for (let i = 0; i < text.length; i++) h = (h * 31 + text.charCodeAt(i)) >>> 0;
  return h;
};

export const addressOf = (tenant) => {
  const h = hash(tenant);
  return `10.${(h % 250) + 1}.${(Math.floor(h / 250) % 250) + 1}.${(Math.floor(h / 62500) % 250) + 1}`;
};

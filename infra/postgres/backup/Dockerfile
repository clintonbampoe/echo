FROM alpine:3.20

# Install client tools and dependencies
# postgresql18-client is the specific package for version 18
RUN apk add --no-cache \
  ca-certificates \
  curl \
  bash

# Install Postgres 18 Client from Edge
RUN apk add --no-cache \
  --repository=http://dl-cdn.alpinelinux.org/alpine/edge/main \
  postgresql18-client

# Install Supercronic
# Latest releases available at https://github.com/aptible/supercronic/releases
ENV SUPERCRONIC_URL=https://github.com/aptible/supercronic/releases/download/v0.2.49/supercronic-linux-amd64 \
  SUPERCRONIC_SHA1SUM=e63c11a9726b775a6a11801e81af4f3fb926aa68 \
  SUPERCRONIC=supercronic-linux-amd64

RUN curl -fsSLO "$SUPERCRONIC_URL" \
  && echo "${SUPERCRONIC_SHA1SUM}  ${SUPERCRONIC}" | sha1sum -c - \
  && chmod +x "$SUPERCRONIC" \
  && mv "$SUPERCRONIC" "/usr/local/bin/${SUPERCRONIC}" \
  && ln -s "/usr/local/bin/${SUPERCRONIC}" /usr/local/bin/supercronic

# Crontab is copied here
COPY infra/postgres/backup/crontab app/crontab

# Run Supercronic
CMD ["supercronic", "/app/crontab"]

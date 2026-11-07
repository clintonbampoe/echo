FROM alpine:3.20

RUN apk add --no-cache bash

RUN apk add --no-cache \
    --repository=http://dl-cdn.alpinelinux.org/alpine/edge/main \
    postgresql18-client

ARG SUPERCRONIC_VERSION=v0.2.49
ARG SUPERCRONIC_SHA1=e63c11a9726b775a6a11801e81af4f3fb926aa68

RUN wget -qO /usr/local/bin/supercronic \
    "https://github.com/aptible/supercronic/releases/download/${SUPERCRONIC_VERSION}/supercronic-linux-amd64" \
  && echo "${SUPERCRONIC_SHA1}  /usr/local/bin/supercronic" | sha1sum -c - \
  && chmod +x /usr/local/bin/supercronic

COPY infra/postgres/backup/crontab /app/crontab
COPY --chmod=755 infra/postgres/backup/backup.sh infra/postgres/backup/restore.sh /app/

CMD ["supercronic", "/app/crontab"]

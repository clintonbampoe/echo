FROM alpine:3.20

RUN apk add --no-cache bash

RUN apk add --no-cache \
  --repository=http://dl-cdn.alpinelinux.org/alpine/edge/main \
  postgresql18-client

ARG TARGETARCH
ARG SUPERCRONIC_VERSION=v0.2.49
ARG SUPERCRONIC_SHA1_AMD64=e63c11a9726b775a6a11801e81af4f3fb926aa68
ARG SUPERCRONIC_SHA1_ARM64=0b6c5bb743e0b0dafed1132198c81807927ac413

RUN if [ "$TARGETARCH" = "arm64" ]; then ARCH=arm64; SHA1="$SUPERCRONIC_SHA1_ARM64"; \
  else ARCH=amd64; SHA1="$SUPERCRONIC_SHA1_AMD64"; fi \
  && wget -qO /usr/local/bin/supercronic \
    "https://github.com/aptible/supercronic/releases/download/${SUPERCRONIC_VERSION}/supercronic-linux-${ARCH}" \
  && echo "${SHA1}  /usr/local/bin/supercronic" | sha1sum -c - \
  && chmod +x /usr/local/bin/supercronic

COPY infra/postgres/backup/crontab /app/crontab
COPY --chmod=755 infra/postgres/backup/backup.sh infra/postgres/backup/restore.sh /app/

CMD ["supercronic", "/app/crontab"]

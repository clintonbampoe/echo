FROM certbot/certbot

ARG SUPERCRONIC_VERSION=v0.2.49
ARG SUPERCRONIC_SHA1=e63c11a9726b775a6a11801e81af4f3fb926aa68

RUN wget -qO /usr/local/bin/supercronic \
    "https://github.com/aptible/supercronic/releases/download/${SUPERCRONIC_VERSION}/supercronic-linux-amd64" \
  && echo "${SUPERCRONIC_SHA1}  /usr/local/bin/supercronic" | sha1sum -c - \
  && chmod +x /usr/local/bin/supercronic

COPY infra/certbot/crontab /etc/crontab
COPY --chmod=755 infra/status/heartbeat.sh /app/heartbeat.sh

ENTRYPOINT ["supercronic"]
CMD ["/etc/crontab"]

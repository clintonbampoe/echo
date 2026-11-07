FROM grafana/loki:3.5.0

USER 10001
COPY --chown=10001:10001 infra/observability/loki/loki-config.yml \
  /etc/loki/config.yml

CMD ["-config.file=/etc/loki/config.yml", "-config.expand-env=true"]
